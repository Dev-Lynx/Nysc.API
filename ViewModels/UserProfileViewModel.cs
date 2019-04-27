using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.ViewModels
{
    public class UserProfileViewModel
    {
        public int FileNo { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string StateOfResidence { get; set; }
        public string StateOfOrigin { get; set; }
        public DateTime DateOfBirth { get; set; }
		public string Rank { get; set; }
        public string Qualification { get; set; }
		public string Location { get; set; }
        public string Department { get; set; }
        public PhotoViewModel Signature { get; set; }
        public PhotoViewModel Passport { get; set; }
    }
}
