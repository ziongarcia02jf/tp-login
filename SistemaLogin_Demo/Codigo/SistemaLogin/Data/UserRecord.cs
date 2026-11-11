using System;
using System.Xml.Serialization;

namespace SistemaLogin.Data
{
    /// <summary>
    /// Datos de una cuenta. La contraseña y las respuestas de seguridad
    /// nunca se guardan en texto plano: solo se almacenan sus hashes.
    /// </summary>
    public class UserRecord
    {
        [XmlAttribute("usuario")]
        public string Username { get; set; }

        public string FullName { get; set; }

        /// <summary>
        /// "Administrador" o "Usuario".
        /// </summary>
        public string Role { get; set; }

        public string PasswordSalt { get; set; }

        public string PasswordHash { get; set; }

        public string SecuritySalt { get; set; }

        public string Answer1Hash { get; set; }

        public string Answer2Hash { get; set; }

        public string Answer3Hash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastLoginAt { get; set; }

        [XmlIgnore]
        public DateTime PreviousLoginAt { get; set; }

        [XmlIgnore]
        public bool IsAdmin
        {
            get { return string.Equals(Role, "Administrador", StringComparison.OrdinalIgnoreCase); }
        }

        public override string ToString()
        {
            return Username + "  |  " + FullName + "  |  " + Role;
        }
    }
}
