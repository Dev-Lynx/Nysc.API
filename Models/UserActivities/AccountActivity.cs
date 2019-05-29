using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nysc.API.Models.Entities;
using Nysc.API.ViewModels;

namespace Nysc.API.Models.UserActivities
{
    public class AccountActivity : UserActivityBase
    {
        #region Methods
        public override SimpleUserActivity Simplify()
        {
            string description = string.Empty;
            switch (ActivityType)
            {
                case Activity.AccountCreated:
                    Description = "Account successfully created.";
                    break;

                case Activity.AccountUpdated:
                    Description = "Account updated by user.";
                    break;
            };

            return base.Simplify();
        }
        #endregion
    }
}
