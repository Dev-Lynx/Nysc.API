using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models.Entities
{
    public class OneTimePassword
    {
        #region Properties

        #region Statics
        public static TimeSpan LifeSpan { get; } = TimeSpan.FromMinutes(5);
        #endregion

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
        public string Code { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public bool IsExpired => Created + LifeSpan < DateTime.Now;
        #endregion

        #region Constructors
        public OneTimePassword() { }
        public OneTimePassword(User user) { User = user; }
        #endregion

        #region Methods
        public bool Verify(string code) => Code == code;
        #endregion
    }
}
