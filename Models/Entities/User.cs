using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Nysc.API.Models.Entities;
using PhoneNumbers;

namespace Nysc.API.Models
{
    public class User : IdentityUser
    {
        #region Properties
        public int FileNo { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string StateOfResidence { get; set; }
        public string StateOfOrigin { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Rank { get; set; }
        public string Qualification { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public OneTimePassword OneTimePassword { get; set; }
        public ICollection<Photo> Photos { get; set; } = new Collection<Photo>();

        [NotMapped]
        public string FormattedPhoneNumber
        {
            get
            {
                var phoneUtil = PhoneNumberUtil.GetInstance();
                var numberProto = phoneUtil.Parse(PhoneNumber, "NG");
                return phoneUtil.Format(numberProto, PhoneNumberFormat.E164);
            }
        }
        #endregion       

        #region Constructors
        public User() { }
        #endregion
    }
}