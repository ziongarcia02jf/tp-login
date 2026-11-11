using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SistemaLogin.Security;

namespace SistemaLogin.Data
{
    /// <summary>
    /// Administra el archivo local de usuarios.
    /// Los datos se guardan en %AppData%\SistemaLogin\usuarios.xml
    /// </summary>
    public static class UserStore
    {
        public const string AdminRole = "Administrador";
        public const string UserRole = "Usuario";

        private static readonly string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SistemaLogin");

        private static readonly string UsersFile = Path.Combine(DataFolder, "usuarios.xml");

        public static string StoragePath
        {
            get { return UsersFile; }
        }

        public static UserDatabase Load()
        {
            if (!File.Exists(UsersFile))
            {
                UserDatabase seeded = CreateDefaultDatabase();
                try
                {
                    Save(seeded);
                }
                catch
                {
                    // Si no se puede guardar, la aplicación seguirá funcionando en memoria.
                }
                return seeded;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UserDatabase));
                using (FileStream stream = File.OpenRead(UsersFile))
                {
                    UserDatabase database = serializer.Deserialize(stream) as UserDatabase;
                    if (database == null)
                        return new UserDatabase();
                    if (database.Users == null)
                        database.Users = new List<UserRecord>();
                    return database;
                }
            }
            catch
            {
                // Si el archivo está dañado, guardamos una copia y volvemos a los usuarios de demo.
                try
                {
                    File.Copy(UsersFile, UsersFile + ".bak", true);
                }
                catch
                {
                    // Si no se puede hacer la copia, seguimos igual.
                }

                UserDatabase seeded = CreateDefaultDatabase();
                try
                {
                    Save(seeded);
                }
                catch
                {
                    // Si no se puede guardar, la aplicación seguirá funcionando en memoria.
                }
                return seeded;
            }
        }

        public static void Save(UserDatabase database)
        {
            if (database == null) throw new ArgumentNullException("database");

            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);

            string temporaryFile = UsersFile + ".tmp";
            XmlSerializer serializer = new XmlSerializer(typeof(UserDatabase));

            using (FileStream stream = File.Create(temporaryFile))
            {
                serializer.Serialize(stream, database);
            }

            if (File.Exists(UsersFile))
                File.Copy(UsersFile, UsersFile + ".bak", true);

            File.Copy(temporaryFile, UsersFile, true);
            File.Delete(temporaryFile);
        }

        public static List<UserRecord> GetAllUsers()
        {
            return Load().Users
                .OrderByDescending(user => user.IsAdmin)
                .ThenBy(user => user.Username, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return Load().Users.Any(user =>
                string.Equals(user.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static UserRecord Find(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return Load().Users.FirstOrDefault(user =>
                string.Equals(user.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Comprueba usuario y contraseña sin registrar el ingreso.
        /// El ingreso se registra recién cuando también se responden las 3 preguntas.
        /// </summary>
        public static UserRecord ValidateCredentials(string username, string password)
        {
            UserRecord user = Find(username);
            if (user == null)
                return null;

            if (!PasswordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
                return null;

            return user;
        }

        public static bool VerifyAnswers(UserRecord user, string[] answers)
        {
            if (user == null || answers == null || answers.Length != SecurityQuestions.All.Length)
                return false;

            return PasswordHasher.Verify(
                       SecurityQuestions.Normalize(answers[0]),
                       user.SecuritySalt,
                       user.Answer1Hash)
                   && PasswordHasher.Verify(
                       SecurityQuestions.Normalize(answers[1]),
                       user.SecuritySalt,
                       user.Answer2Hash)
                   && PasswordHasher.Verify(
                       SecurityQuestions.Normalize(answers[2]),
                       user.SecuritySalt,
                       user.Answer3Hash);
        }

        /// <summary>
        /// Guarda la fecha del último acceso después de una autenticación completa.
        /// </summary>
        public static void RegisterLogin(UserRecord user)
        {
            if (user == null)
                return;

            UserDatabase database = Load();
            UserRecord stored = database.Users.FirstOrDefault(item =>
                string.Equals(item.Username, user.Username, StringComparison.OrdinalIgnoreCase));

            if (stored == null)
                return;

            stored.PreviousLoginAt = stored.LastLoginAt;
            stored.LastLoginAt = DateTime.Now;

            user.PreviousLoginAt = stored.PreviousLoginAt;
            user.LastLoginAt = stored.LastLoginAt;

            try
            {
                Save(database);
            }
            catch
            {
                // Si no se puede guardar el último acceso, el ingreso continúa igual.
            }
        }

        public static void AddUser(
            string username,
            string fullName,
            string role,
            string password,
            string[] answers)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("El nombre de usuario es obligatorio.", "username");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es obligatoria.", "password");
            if (answers == null || answers.Length != SecurityQuestions.All.Length)
                throw new ArgumentException("Se deben responder las tres preguntas de seguridad.", "answers");

            foreach (string answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer))
                    throw new ArgumentException("Las tres respuestas de seguridad son obligatorias.", "answers");
            }

            string normalizedRole = IsValidRole(role) ? role : UserRole;

            if (UsernameExists(username))
                throw new InvalidOperationException("Ese nombre de usuario ya está en uso.");

            UserDatabase database = Load();
            string passwordSalt = PasswordHasher.CreateSalt();
            string securitySalt = PasswordHasher.CreateSalt();

            UserRecord user = new UserRecord();
            user.Username = username.Trim();
            user.FullName = (fullName ?? string.Empty).Trim();
            user.Role = normalizedRole;
            user.PasswordSalt = passwordSalt;
            user.PasswordHash = PasswordHasher.Hash(password, passwordSalt);
            user.SecuritySalt = securitySalt;
            user.Answer1Hash = PasswordHasher.Hash(SecurityQuestions.Normalize(answers[0]), securitySalt);
            user.Answer2Hash = PasswordHasher.Hash(SecurityQuestions.Normalize(answers[1]), securitySalt);
            user.Answer3Hash = PasswordHasher.Hash(SecurityQuestions.Normalize(answers[2]), securitySalt);
            user.CreatedAt = DateTime.Now;
            user.LastLoginAt = DateTime.MinValue;

            database.Users.Add(user);
            Save(database);
        }

        public static bool DeleteUser(string username)
        {
            UserDatabase database = Load();
            UserRecord user = database.Users.FirstOrDefault(item =>
                string.Equals(item.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return false;

            if (user.IsAdmin && CountAdmins(database) <= 1)
                throw new InvalidOperationException("No se puede eliminar al último administrador.");

            database.Users.Remove(user);
            Save(database);
            return true;
        }

        public static int CountAdmins()
        {
            return CountAdmins(Load());
        }

        private static int CountAdmins(UserDatabase database)
        {
            return database.Users.Count(user => user.IsAdmin);
        }

        public static bool IsValidRole(string role)
        {
            return string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, UserRole, StringComparison.OrdinalIgnoreCase);
        }

        private static UserDatabase CreateDefaultDatabase()
        {
            UserDatabase database = new UserDatabase();

            database.Users.Add(CreateUser(
                "admin",
                "Administrador del sistema",
                AdminRole,
                "admin123",
                new string[] { "firulais", "buenos aires", "san martin" }));

            database.Users.Add(CreateUser(
                "usuario",
                "Usuario Común",
                UserRole,
                "usuario123",
                new string[] { "luna", "cordoba", "belgrano" }));

            return database;
        }

        private static UserRecord CreateUser(
            string username,
            string fullName,
            string role,
            string password,
            string[] answers)
        {
            string passwordSalt = PasswordHasher.CreateSalt();
            string securitySalt = PasswordHasher.CreateSalt();

            UserRecord user = new UserRecord();
            user.Username = username;
            user.FullName = fullName;
            user.Role = role;
            user.PasswordSalt = passwordSalt;
            user.PasswordHash = PasswordHasher.Hash(password, passwordSalt);
            user.SecuritySalt = securitySalt;
            user.Answer1Hash = PasswordHasher.Hash(SecurityQuestions.Normalize(answers[0]), securitySalt);
            user.Answer2Hash = PasswordHasher.Hash(SecurityQuestions.Normalize(answers[1]), securitySalt);
            user.Answer3Hash = PasswordHasher.Hash(SecurityQuestions.Normalize(answers[2]), securitySalt);
            user.CreatedAt = DateTime.Now;
            user.LastLoginAt = DateTime.MinValue;
            return user;
        }
    }
}
