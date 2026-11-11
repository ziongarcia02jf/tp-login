using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaLogin.Data;
using SistemaLogin.Security;

namespace SistemaLogin.Forms
{
    /// <summary>
    /// Segundo paso del ingreso: las 3 preguntas de seguridad.
    /// </summary>
    public class VerificationForm : Form
    {
        private readonly UserRecord account;
        private readonly TextBox[] answerBoxes;
        private Label lblError;

        public VerificationForm(UserRecord account)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            this.account = account;
            answerBoxes = new TextBox[SecurityQuestions.All.Length];
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Preguntas de seguridad";
            ClientSize = new Size(480, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Preguntas de seguridad";
            title.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 20;
            title.Top = 16;
            Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Hola, " + account.FullName + ". Respondé las 3 preguntas para ingresar.";
            subtitle.Left = 20;
            subtitle.Top = 52;
            subtitle.Width = 440;
            subtitle.Height = 34;
            Controls.Add(subtitle);

            int top = 98;

            for (int index = 0; index < SecurityQuestions.All.Length; index++)
            {
                Label question = new Label();
                question.Text = SecurityQuestions.All[index];
                question.AutoSize = true;
                question.Left = 20;
                question.Top = top;
                Controls.Add(question);

                TextBox answer = new TextBox();
                answer.Left = 20;
                answer.Top = top + 20;
                answer.Width = 440;
                answer.MaxLength = 80;
                answer.TabIndex = index;
                answerBoxes[index] = answer;
                Controls.Add(answer);

                top += 66;
            }

            Label hint = new Label();
            hint.Text = "No se distinguen mayúsculas, minúsculas ni acentos.";
            hint.AutoSize = true;
            hint.ForeColor = Color.DimGray;
            hint.Left = 20;
            hint.Top = 296;
            Controls.Add(hint);

            lblError = new Label();
            lblError.Left = 20;
            lblError.Top = 318;
            lblError.Width = 440;
            lblError.Height = 38;
            lblError.ForeColor = Color.Firebrick;
            Controls.Add(lblError);

            Button btnVerify = new Button();
            btnVerify.Text = "Verificar e ingresar";
            btnVerify.Left = 20;
            btnVerify.Top = 360;
            btnVerify.Width = 210;
            btnVerify.Height = 32;
            btnVerify.TabIndex = 3;
            btnVerify.Click += delegate { Verify(); };
            Controls.Add(btnVerify);

            Button btnCancel = new Button();
            btnCancel.Text = "Volver";
            btnCancel.Left = 250;
            btnCancel.Top = 360;
            btnCancel.Width = 210;
            btnCancel.Height = 32;
            btnCancel.TabIndex = 4;
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnVerify;
            CancelButton = btnCancel;

            Shown += delegate { answerBoxes[0].Focus(); };
        }

        private void Verify()
        {
            lblError.Text = string.Empty;
            string[] answers = new string[answerBoxes.Length];

            for (int index = 0; index < answerBoxes.Length; index++)
            {
                answers[index] = answerBoxes[index].Text.Trim();

                if (answers[index].Length == 0)
                {
                    lblError.Text = "Respondé las tres preguntas.";
                    answerBoxes[index].Focus();
                    return;
                }
            }

            if (!UserStore.VerifyAnswers(account, answers))
            {
                lblError.Text = "Una o más respuestas son incorrectas.";
                answerBoxes[0].SelectAll();
                answerBoxes[0].Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
