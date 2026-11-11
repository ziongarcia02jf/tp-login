using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaLogin.Data;

namespace SistemaLogin.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPassword;
        private CheckBox chkShow;
        private Label lblError;

        public LoginForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Iniciar sesión";
            ClientSize = new Size(400, 360);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Iniciar sesión";
            title.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 24;
            title.Top = 20;
            Controls.Add(title);

            AddLabel("Usuario", 24, 68);

            txtUser = new TextBox();
            txtUser.Left = 24;
            txtUser.Top = 88;
            txtUser.Width = 352;
            txtUser.MaxLength = 20;
            txtUser.TabIndex = 0;
            Controls.Add(txtUser);

            AddLabel("Contraseña", 24, 126);

            txtPassword = new TextBox();
            txtPassword.Left = 24;
            txtPassword.Top = 146;
            txtPassword.Width = 352;
            txtPassword.MaxLength = 50;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.TabIndex = 1;
            Controls.Add(txtPassword);

            chkShow = new CheckBox();
            chkShow.Text = "Mostrar contraseña";
            chkShow.AutoSize = true;
            chkShow.Left = 24;
            chkShow.Top = 180;
            chkShow.TabIndex = 2;
            chkShow.CheckedChanged += delegate
            {
                txtPassword.UseSystemPasswordChar = !chkShow.Checked;
            };
            Controls.Add(chkShow);

            lblError = new Label();
            lblError.Left = 24;
            lblError.Top = 206;
            lblError.Width = 352;
            lblError.Height = 38;
            lblError.ForeColor = Color.Firebrick;
            Controls.Add(lblError);

            Button btnLogin = new Button();
            btnLogin.Text = "Ingresar";
            btnLogin.Left = 24;
            btnLogin.Top = 252;
            btnLogin.Width = 170;
            btnLogin.Height = 32;
            btnLogin.TabIndex = 3;
            btnLogin.Click += delegate { TryLogin(); };
            Controls.Add(btnLogin);

            Button btnExit = new Button();
            btnExit.Text = "Salir";
            btnExit.Left = 206;
            btnExit.Top = 252;
            btnExit.Width = 170;
            btnExit.Height = 32;
            btnExit.TabIndex = 4;
            btnExit.Click += delegate { Close(); };
            Controls.Add(btnExit);

            Label demo = new Label();
            demo.Text = "Demo: admin / admin123   ·   usuario / usuario123\r\nLas respuestas de seguridad están en el README.";
            demo.Left = 24;
            demo.Top = 298;
            demo.Width = 352;
            demo.Height = 42;
            demo.ForeColor = Color.DimGray;
            Controls.Add(demo);

            AcceptButton = btnLogin;
            CancelButton = btnExit;

            Shown += delegate { txtUser.Focus(); };
        }

        private void AddLabel(string text, int left, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Left = left;
            label.Top = top;
            Controls.Add(label);
        }

        private void TryLogin()
        {
            lblError.Text = string.Empty;
            string username = txtUser.Text.Trim();
            string password = txtPassword.Text;

            if (username.Length == 0)
            {
                lblError.Text = "Ingresá tu nombre de usuario.";
                txtUser.Focus();
                return;
            }

            if (password.Length == 0)
            {
                lblError.Text = "Ingresá tu contraseña.";
                txtPassword.Focus();
                return;
            }

            UserRecord account = UserStore.ValidateCredentials(username, password);
            if (account == null)
            {
                lblError.Text = "Usuario o contraseña incorrectos.";
                txtPassword.SelectAll();
                txtPassword.Focus();
                return;
            }

            using (VerificationForm verification = new VerificationForm(account))
            {
                if (verification.ShowDialog(this) != DialogResult.OK)
                {
                    lblError.Text = string.Empty;
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }
            }

            UserStore.RegisterLogin(account);
            OpenMain(account);
        }

        private void OpenMain(UserRecord account)
        {
            Hide();

            using (MainForm main = new MainForm(account))
            {
                main.ShowDialog();

                if (main.LogoutRequested)
                {
                    txtPassword.Clear();
                    lblError.Text = string.Empty;
                    Show();
                    Activate();
                    txtPassword.Focus();
                }
                else
                {
                    Close();
                }
            }
        }
    }
}
