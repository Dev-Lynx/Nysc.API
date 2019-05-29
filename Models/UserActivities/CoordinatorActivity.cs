using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nysc.API.ViewModels;

namespace Nysc.API.Models.Entities
{
    public class CoordinatorActivity : UserActivityBase
    {
        public User Coordinator { get; set; }

        public CoordinatorActivity() { }
        public CoordinatorActivity(Activity activity, User coordinator)
        {
            Coordinator = coordinator;
            ActivityType = activity;
        }

        public override SimpleUserActivity Simplify()
        {
            switch (ActivityType)
            {
                case Activity.UpdateRequested:
                    Description = $"{Coordinator.LastName} " +
                        $"{Coordinator.OtherNames} requested an account update.";
                    break;

                case Activity.AccountApproved:
                    Description = $"{Coordinator.LastName} " +
                        $"{Coordinator.OtherNames} approved account details.";
                    break;
            }

            return base.Simplify();
        }
    }
}
