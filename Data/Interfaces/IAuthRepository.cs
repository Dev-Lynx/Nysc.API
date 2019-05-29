using Nysc.API.Models;
using Nysc.API.ViewModels;
using System.Threading.Tasks;

namespace Nysc.API.Data.Interfaces
{
    public interface IAuthRepository
    {
        #region Properties
        Task<User> GetUserById(string id);
        Task<string> Login(UserLoginViewModel model);

        Task<bool> GenerateOTP(User user);
        Task<string> ValidateOTP(User user, string code);
        Task<User> GetUser(string username);
        Task<bool> UserIsInRole(User user, string role);
        #endregion
    }
}
