using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SistemaLogin.Data;

namespace SistemaLogin.Forms
{
    public class MainForm : Form
    {
        private readonly UserRecord account;
        private ListBox listUsers;
        private Button btnDelete;
        private Label lblStatus;

        public bool LogoutRequested { get; private set; }

        public MainForm(UserRecord account)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            this.account = account;
            BuildUi();

            if (account.IsAdmin)
                RefreshUsers();
        }

        private void BuildUi()
        {
            Text = "Panel principal";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = account.IsAdmin ? "Panel de administración" : "Panel de usuario";
            title.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 20;
            title.Top = 16;
            Controls.Add(title);

            if (account.IsAdmin)
                BuildAdminUi();
            else
                BuildUserUi();
        }

        private void BuildAdminUi()
        {
            ClientSize = new Size(640, 430);

            Label label = new Label();
            label.Text = "Usuarios registrados:";
            label.AutoSize = true;
            label.Left = 20;
            label.Top = 58;
            Controls.Add(label);

            listUsers = new ListBox();
            listUsers.Left = 20;
            listUsers.Top = 80;
            listUsers.Width = 600;
            listUsers.Height = 230;
            listUsers.TabIndex = 0;
            listUsers.SelectedIndexChanged += delegate { UpdateDeleteButton(); };
            listUsers.DoubleClick += delegate { ShowSelectedUser(); };
            Controls.Add(listUsers);

            Button btnAdd = new Button();
            btnAdd.Text = "Agregar usuario";
            btnAdd.Left = 20;
            btnAdd.Top = 322;
            btnAdd.Width = 150;
            btnAdd.Height = 32;
            btnAdd.TabIndex = 1;
            btnAdd.Click += delegate { AddUser(); };
            Controls.Add(btnAdd);

            btnDelete = new Button();
            btnDelete.Text = "Eliminar usuario";
            btnDelete.Left = 180;
            btnDelete.Top = 322;
            btnDelete.Width = 150;
            btnDelete.Height = 32;
            btnDelete.TabIndex = 2;
            btnDelete.Enabled = false;
            btnDelete.Click += delegate { DeleteSelectedUser(); };
            Controls.Add(btnDelete);

            Button btnRefresh = new Button();
            btnRefresh.Text = "Actualizar";
            btnRefresh.Left = 340;
            btnRefresh.Top = 322;
            btnRefresh.Width = 100;
            btnRefresh.Height = 32;
            btnRefresh.TabIndex = 3;
            btnRefresh.Click += delegate { RefreshUsers(); };
            Controls.Add(btnRefresh);

            Button btnLogout = new Button();
            btnLogout.Text = "Cerrar sesión";
            btnLogout.Left = 450;
            btnLogout.Top = 322;
            btnLogout.Width = 170;
            btnLogout.Height = 32;
            btnLogout.TabIndex = 4;
            btnLogout.Click += delegate { Logout(); };
            Controls.Add(btnLogout);

            lblStatus = new Label();
            lblStatus.Left = 20;
            lblStatus.Top = 366;
            lblStatus.Width = 600;
            lblStatus.Height = 44;
            Controls.Add(lblStatus);
        }

        private void BuildUserUi()
        {
            ClientSize = new Size(640, 360);

            Label welcome = new Label();
            welcome.Text = "Bienvenido, " + account.FullName;
            welcome.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            welcome.AutoSize = true;
            welcome.Left = 20;
            welcome.Top = 58;
            Controls.Add(welcome);

            AddInfoLabel("Usuario: " + account.Username, 20, 100);
            AddInfoLabel("Rol: Usuario común", 20, 128);

            string lastAccess = account.PreviousLoginAt == DateTime.MinValue
                ? "Primer inicio de sesión"
                : account.PreviousLoginAt.ToString("dd/MM/yyyy HH:mm");
            AddInfoLabel("Último acceso: " + lastAccess, 20, 156);

            string createdAt = account.CreatedAt == DateTime.MinValue
                ? "—"
                : account.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            AddInfoLabel("Cuenta creada: " + createdAt, 20, 184);

            Label note = new Label();
            note.Text = "Para ingresar respondiste correctamente las 3 preguntas de seguridad.";
            note.Left = 20;
            note.Top = 226;
            note.Width = 600;
            note.Height = 34;
            note.ForeColor = Color.DimGray;
            Controls.Add(note);

            Button btnLogout = new Button();
            btnLogout.Text = "Cerrar sesión";
            btnLogout.Left = 20;
            btnLogout.Top = 286;
            btnLogout.Width = 170;
            btnLogout.Height = 32;
            btnLogout.TabIndex = 0;
            btnLogout.Click += delegate { Logout(); };
            Controls.Add(btnLogout);
        }

        private void AddInfoLabel(string text, int left, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Left = left;
            label.Top = top;
            Controls.Add(label);
        }

        private void RefreshUsers()
        {
            List<UserRecord> users = UserStore.GetAllUsers();

            listUsers.BeginUpdate();
            listUsers.Items.Clear();
            foreach (UserRecord user in users)
                listUsers.Items.Add(user);
            listUsers.EndUpdate();

            int admins = users.FindAll(user => user.IsAdmin).Count;
            lblStatus.Text = users.Count + " usuario(s) registrado(s). " + admins + " administrador(es).";
            UpdateDeleteButton();
        }

        private void UpdateDeleteButton()
        {
            if (btnDelete != null)
                btnDelete.Enabled = listUsers.SelectedItem != null;
        }

        private void AddUser()
        {
            using (UserEditorForm form = new UserEditorForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshUsers();
                    SelectUser(form.CreatedUsername);
                    lblStatus.Text = "Usuario \"" + form.CreatedUsername + "\" agregado.";
                }
            }
        }

        private void DeleteSelectedUser()
        {
            UserRecord selected = listUsers.SelectedItem as UserRecord;
            if (selected == null)
                return;

            if (string.Equals(selected.Username, account.Username, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    this,
                    "No podés eliminar tu propia cuenta.",
                    "Acción no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                "¿Eliminar al usuario \"" + selected.Username + "\"?",
                "Eliminar usuario",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                if (UserStore.DeleteUser(selected.Username))
                {
                    RefreshUsers();
                    lblStatus.Text = "Usuario \"" + selected.Username + "\" eliminado.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowSelectedUser()
        {
            UserRecord user = listUsers.SelectedItem as UserRecord;
            if (user == null)
                return;

            string lastAccess = user.LastLoginAt == DateTime.MinValue
                ? "Nunca"
                : user.LastLoginAt.ToString("dd/MM/yyyy HH:mm");

            MessageBox.Show(
                this,
                "Usuario: " + user.Username + "\r\n"
                + "Nombre: " + user.FullName + "\r\n"
                + "Rol: " + user.Role + "\r\n"
                + "Creado: " + user.CreatedAt.ToString("dd/MM/yyyy HH:mm") + "\r\n"
                + "Último acceso: " + lastAccess,
                "Detalle del usuario",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SelectUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;

            foreach (object item in listUsers.Items)
            {
                UserRecord user = item as UserRecord;
                if (user != null && string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    listUsers.SelectedItem = user;
                    return;
                }
            }
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show(
                this,
                "¿Cerrar la sesión actual?",
                "Cerrar sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                LogoutRequested = true;
                Close();
            }
        }
    }
}
