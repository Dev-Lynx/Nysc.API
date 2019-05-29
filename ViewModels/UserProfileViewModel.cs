using Nysc.API.Models;
using Nysc.API.Models.Entities;
using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.ViewModels
{
    public class UserProfileViewModel
    {
        // TODO: Remove File No. from user entity
        public string Id { get; set; }
        public string Username { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public int FileNo { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string LastName { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string OtherNames { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string Gender { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string MaritalStatus { get; set; }
        public string StateOfResidence { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string StateOfOrigin { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime DateOfBirth { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string Rank { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string Qualification { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string Location { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public string Department { get; set; }

        public string DisplayName { get; set; }

        public ICollection<PhotoViewModel> Photos { get; set; }

        public PhotoViewModel Signature { get; set; }
        public PhotoViewModel Passport { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; }
    }
}
