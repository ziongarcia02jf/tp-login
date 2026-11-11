using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SistemaLogin.Data;
using SistemaLogin.Security;

namespace SistemaLogin.Forms
{
    /// <summary>
    /// Formulario que usa el administrador para dar de alta un usuario.
    /// </summary>
    public class UserEditorForm : Form
    {
        private TextBox txtFullName;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirm;
        private ComboBox cboRole;
        private TextBox[] answerBoxes;
        private Label lblError;

        public string CreatedUsername { get; private set; }

        public UserEditorForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Agregar usuario";
            ClientSize = new Size(500, 610);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Agregar usuario";
            title.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 20;
            title.Top = 16;
            Controls.Add(title);

            AddLabel("Nombre completo", 20, 56);

            txtFullName = new TextBox();
            txtFullName.Left = 20;
            txtFullName.Top = 76;
            txtFullName.Width = 460;
            txtFullName.MaxLength = 80;
            txtFullName.TabIndex = 0;
            Controls.Add(txtFullName);

            AddLabel("Usuario", 20, 112);

            txtUsername = new TextBox();
            txtUsername.Left = 20;
            txtUsername.Top = 132;
            txtUsername.Width = 220;
            txtUsername.MaxLength = 20;
            txtUsername.TabIndex = 1;
            Controls.Add(txtUsername);

            AddLabel("Contraseña", 250, 112);

            txtPassword = new TextBox();
            txtPassword.Left = 250;
            txtPassword.Top = 132;
            txtPassword.Width = 230;
            txtPassword.MaxLength = 50;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.TabIndex = 2;
            Controls.Add(txtPassword);

            AddLabel("Repetir contraseña", 20, 168);

            txtConfirm = new TextBox();
            txtConfirm.Left = 20;
            txtConfirm.Top = 188;
            txtConfirm.Width = 220;
            txtConfirm.MaxLength = 50;
            txtConfirm.UseSystemPasswordChar = true;
            txtConfirm.TabIndex = 3;
            Controls.Add(txtConfirm);

            AddLabel("Rol", 250, 168);

            cboRole = new ComboBox();
            cboRole.Left = 250;
            cboRole.Top = 188;
            cboRole.Width = 230;
            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRole.Items.Add(UserStore.UserRole);
            cboRole.Items.Add(UserStore.AdminRole);
            cboRole.SelectedIndex = 0;
            cboRole.TabIndex = 4;
            Controls.Add(cboRole);

            GroupBox security = new GroupBox();
            security.Text = "Preguntas de seguridad";
            security.Left = 20;
            security.Top = 232;
            security.Width = 460;
            security.Height = 240;
            Controls.Add(security);

            answerBoxes = new TextBox[SecurityQuestions.All.Length];
            int top = 24;

            for (int index = 0; index < SecurityQuestions.All.Length; index++)
            {
                Label question = new Label();
                question.Text = SecurityQuestions.All[index];
                question.AutoSize = true;
                question.Left = 12;
                question.Top = top;
                security.Controls.Add(question);

                TextBox answer = new TextBox();
                answer.Left = 12;
                answer.Top = top + 20;
                answer.Width = 436;
                answer.MaxLength = 80;
                answer.TabIndex = 5 + index;
                answerBoxes[index] = answer;
                security.Controls.Add(answer);

                top += 62;
            }

            Label hint = new Label();
            hint.Text = "La contraseña debe tener al menos 4 caracteres.";
            hint.AutoSize = true;
            hint.ForeColor = Color.DimGray;
            hint.Left = 12;
            hint.Top = 210;
            security.Controls.Add(hint);

            lblError = new Label();
            lblError.Left = 20;
            lblError.Top = 484;
            lblError.Width = 460;
            lblError.Height = 38;
            lblError.ForeColor = Color.Firebrick;
            Controls.Add(lblError);

            Button btnCreate = new Button();
            btnCreate.Text = "Crear usuario";
            btnCreate.Left = 20;
            btnCreate.Top = 530;
            btnCreate.Width = 220;
            btnCreate.Height = 32;
            btnCreate.TabIndex = 8;
            btnCreate.Click += delegate { CreateUser(); };
            Controls.Add(btnCreate);

            Button btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Left = 260;
            btnCancel.Top = 530;
            btnCancel.Width = 220;
            btnCancel.Height = 32;
            btnCancel.TabIndex = 9;
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            Label footer = new Label();
            footer.Text = "El usuario también deberá responder las 3 preguntas al ingresar.";
            footer.AutoSize = true;
            footer.ForeColor = Color.DimGray;
            footer.Left = 20;
            footer.Top = 574;
            Controls.Add(footer);

            AcceptButton = btnCreate;
            CancelButton = btnCancel;

            Shown += delegate { txtFullName.Focus(); };
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

        private void CreateUser()
        {
            lblError.Text = string.Empty;

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirm.Text;
            string[] answers = new string[answerBoxes.Length];

            if (fullName.Length < 3)
            {
                Fail("Ingresá el nombre completo.", txtFullName);
                return;
            }

            if (!Regex.IsMatch(username, @"^[A-Za-z0-9._-]{4,20}$"))
            {
                Fail("El usuario debe tener entre 4 y 20 caracteres: letras, números, punto, guion o guion bajo.", txtUsername);
                return;
            }

            if (password.Length < 4)
            {
                Fail("La contraseña debe tener al menos 4 caracteres.", txtPassword);
                return;
            }

            if (password != confirm)
            {
                Fail("Las contraseñas no coinciden.", txtConfirm);
                return;
            }

            for (int index = 0; index < answerBoxes.Length; index++)
            {
                answers[index] = answerBoxes[index].Text.Trim();

                if (answers[index].Length == 0)
                {
                    Fail("Respondé las tres preguntas de seguridad.", answerBoxes[index]);
                    return;
                }
            }

            if (UserStore.UsernameExists(username))
            {
                Fail("Ese nombre de usuario ya está en uso.", txtUsername);
                return;
            }

            try
            {
                UserStore.AddUser(
                    username,
                    fullName,
                    cboRole.SelectedItem.ToString(),
                    password,
                    answers);

                CreatedUsername = username;

                MessageBox.Show(
                    this,
                    "Usuario creado correctamente.",
                    "Listo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Fail("No se pudo crear el usuario: " + ex.Message, null);
            }
        }

        private void Fail(string message, Control focus)
        {
            lblError.Text = message;
            if (focus != null)
                focus.Focus();
        }
    }
}
