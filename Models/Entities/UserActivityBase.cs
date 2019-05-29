using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Nysc.API.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nysc.API.Models.Entities
{
    public enum Activity
    {
        AccountCreated,
        AccountUpdated,
        UpdateRequested,
        AccountApproved,
    }

    public class UserActivityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public Activity ActivityType { get; set; } 
        public User User { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        protected string Description { get; set; }

        public virtual SimpleUserActivity Simplify()
        {
            return new SimpleUserActivity()
            {
                Description = Description,
                ActivityType = ActivityType,
                Created = Created
            };
        }
    }
}
