using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _2inch.Models
{
    public class Auth
    { //This should grab the Name and Pass from the Login.
        public string Name { get; set; }
        public string Pass { get; set; } //Change to SecureString??
        public int PermissionLevel { get; set; }

        public Auth(string Name, int PermissionLevel) {
            this.Name = Name;
            this.PermissionLevel = PermissionLevel;
        }
    }
}
