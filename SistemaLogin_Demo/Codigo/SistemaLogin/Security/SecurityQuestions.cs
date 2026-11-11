using System.Globalization;
using System.Text;

namespace SistemaLogin.Security
{
    /// <summary>
    /// Las tres preguntas fijas que se responden al crear una cuenta
    /// y que se vuelven a responder al iniciar sesión.
    /// </summary>
    public static class SecurityQuestions
    {
        public static readonly string[] All = new string[]
        {
            "¿Nombre de tu primera mascota?",
            "¿Ciudad donde naciste?",
            "¿Nombre de tu escuela primaria?"
        };

        /// <summary>
        /// Normaliza una respuesta: sin espacios sobrantes, sin distinguir
        /// mayúsculas de minúsculas y sin acentos.
        /// </summary>
        public static string Normalize(string answer)
        {
            string value = (answer ?? string.Empty).Trim().ToLowerInvariant();

            string decomposed = value.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder(decomposed.Length);

            foreach (char character in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
