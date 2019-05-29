using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lucene.Net.Documents;
using Microsoft.AspNetCore.Identity;
using Nysc.API.Data.Indexing;
using Nysc.API.Models.Entities;
using PhoneNumbers;

namespace Nysc.API.Models
{
    public enum UserRole
    {
        [Description("Administrator")]
        Administrator,
        [Description("Coordinator")]
        Coordinator,
        [Description("Regular User")]
        RegularUser
    }

    public enum ApprovalStatus
    {
        Idle, Approved, Rejected
    }

    public class User : IdentityUser
    {
        #region Properties
        [LuceneField(true, false, IsIdentity = true)]
        public override string Id { get => base.Id; set => base.Id = value; }
        [LuceneField]
        public int FileNo { get; set; }
        [LuceneField]
        public string LastName { get; set; }
        [LuceneField]
        public string OtherNames { get; set; }
        [LuceneField]
        public string Gender { get; set; }
        [LuceneField]
        public string MaritalStatus { get; set; }
        [LuceneField]
        public string StateOfResidence { get; set; } = string.Empty;
        [LuceneField]
        public string StateOfOrigin { get; set; }
        [LuceneField]
        public DateTime DateOfBirth { get; set; }
        [LuceneField]
        public string Rank { get; set; }
        [LuceneField]
        public string Qualification { get; set; }
        [LuceneField]
        public string Department { get; set; }
        [LuceneField]
        public string Location { get; set; }

        [LuceneField]
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        public string DisplayName { get; set; }

        public OneTimePassword OneTimePassword { get; set; }

        public ICollection<Photo> Photos { get; set; } = new Collection<Photo>();

        [NotMapped]
        public string FormattedPhoneNumber => Core.Phone.Format(Phone, PhoneNumberFormat.E164);
        [NotMapped]
        public string InternationalPhoneNumber => Core.Phone.Format(Phone, PhoneNumberFormat.INTERNATIONAL);

        public ApprovalStatus ApprovalStatus { get; set; }

        public ICollection<UserActivityBase> Activities { get; set; } = new Collection<UserActivityBase>();

        #region Internals
        PhoneNumber Phone
        {
            get
            {
                try
                {
                    var phone = Core.Phone.Parse(PhoneNumber, "NG");
                    return phone;
                }
                catch { return new PhoneNumber(); }
            }
        }
        #endregion

        #endregion

        #region Constructors
        public User() { }
        #endregion
    }
}