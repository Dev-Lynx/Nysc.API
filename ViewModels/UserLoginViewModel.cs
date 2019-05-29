using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.ViewModels
{
    public class UserLoginViewModel
    {
        public string Role { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RememberMe { get; set; }

        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }
}

