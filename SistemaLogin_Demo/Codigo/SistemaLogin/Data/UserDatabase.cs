using System.Collections.Generic;
using System.Xml.Serialization;

namespace SistemaLogin.Data
{
    [XmlRoot("Usuarios")]
    public class UserDatabase
    {
        [XmlElement("Usuario")]
        public List<UserRecord> Users { get; set; }

        public UserDatabase()
        {
            Users = new List<UserRecord>();
        }
    }
}
