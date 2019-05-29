using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nysc.API.Data;
using Nysc.API.Data.Indexing;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using Nysc.API.Models.Entities;
using Nysc.API.ViewModels;
using Nysc.API.ViewModels.Mappings;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Properties

        #region Internals
        UserDataContext UserDataContext { get; }
        UserManager<User> UserManager { get; }
        SieveProcessor Sieve { get; }
        IMapper Mapper { get; }
        ISearchEngine<User> SearchEngine { get; }
        ISmsService SmsService { get; }
        #endregion

        #endregion

        #region Constructors
        public IdentityController(UserDataContext userDataContext,
        UserManager<User> userManager, SieveProcessor sieve,
        IMapper mapper, ISearchEngine<User> searchEngine,
        ISmsService smsService)
        {
            UserDataContext = userDataContext;
            UserManager = userManager;
            Sieve = sieve;
            Mapper = mapper;
            SearchEngine = searchEngine;
            SmsService = smsService;
        }
        #endregion

        #region Methods
        [HttpGet]
        public async Task<JsonResult> Get([FromQuery]SearchableSieveModel model)
        {
            /*
            string role = UserRole.RegularUser.ToString();
            string roleFilter = $"role=={role}";

            if (!model.Filters.Contains(roleFilter, StringComparison.OrdinalIgnoreCase))
                model.Filters += "," + roleFilter;
            */


            Core.Log.Debug("Sieve Request Recieved!");
            Core.Log.Debug($"Page: {model.Page}\nPage Size: {model.PageSize}" +
                    $"\nSorts: {model.Sorts}\nFilters: {model.Filters}\nSearch Query: {model.SearchQuery}");


            var data = UserDataContext.Users.AsNoTracking()
                .ProjectTo<UserProfileViewModel>(Mapper.ConfigurationProvider);

            if (!string.IsNullOrWhiteSpace(model.SearchQuery))
            {
                var results = SearchEngine.Find(model.SearchQuery).ToList();
                data = data.Where(u => results.Any(r => r == u.Id));
            }

            var processedData = Sieve.Apply(model, data, applyPagination: false);
            int totalCount = await processedData.CountAsync();


            Core.Log.Debug($"TotalCount: {totalCount}");

            processedData = Sieve.Apply(model, processedData, applySorting: false, applyFiltering: false);

            Request.HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());

            return new JsonResult(await processedData.ToListAsync());
        }
        
        [HttpGet("single")]
        public async Task<IActionResult> GetSingleIdentity([FromQuery]string id)
        {
            User user = await UserManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            return new JsonResult(user);
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetActivities([FromQuery]string id)
        {
            User user = await UserManager.FindByIdAsync(id);

            if (user == null) return NotFound("The requested user does not exist");

            await UserDataContext.Entry(user).Collection(c => c.Activities).LoadAsync();

            List<SimpleUserActivity> activities = new List<SimpleUserActivity>();

            foreach (var activity in user.Activities)
            {
                if (activity is CoordinatorActivity)
                    await UserDataContext.Entry(activity).Reference(nameof(CoordinatorActivity.Coordinator)).LoadAsync();
                activities.Add(Mapper.Map<SimpleUserActivity>(activity));
            }
                

            return new JsonResult(activities);
        }

        [HttpPost("Approve")]
        public async Task<IActionResult> ApproveIdentity([FromBody]string userId)
        {
            return await UpdateApprovalStatus(userId, ApprovalStatus.Approved);
        }

        [HttpPost("RevokeEvaluation")]
        public async Task<IActionResult> RevokeAccountEvaluation([FromBody]string userId)
        {
            return await UpdateApprovalStatus(userId, ApprovalStatus.Idle);
        }

        [HttpPost("Decline")]
        public async Task<IActionResult> DeclineIdentity([FromBody]DeclineUserViewModel model)
        {
            var result = await UpdateApprovalStatus(model.Identity, ApprovalStatus.Rejected);

            if (!(result is OkResult)) return result;

            User user = await UserManager.FindByIdAsync(model.Identity);

            if (string.IsNullOrWhiteSpace(model.Message)) return result;

            await SmsService.SendMessage(user.PhoneNumber, $"{model.Message}. " +
                $"Please visit https://nysc-portal.herokuapp.com " +
                "to update your details");
            return result;
        }

        async Task<IActionResult> UpdateApprovalStatus(string userId, ApprovalStatus status)
        {
            string id = User.FindFirst("id").Value;
            User coordinator = await UserManager.FindByIdAsync(id);

            if (coordinator == null) return Unauthorized();

            User user = await UserManager.FindByIdAsync(userId);
            if (user == null) return NotFound("The requested user was not found");

            if (user.ApprovalStatus == status) return Ok();

            user.ApprovalStatus = status;
            await UserDataContext.Entry(user).Collection(u => u.Activities).LoadAsync();

            Activity activityType = status == ApprovalStatus.Approved ? Activity.AccountApproved : Activity.UpdateRequested;

            CoordinatorActivity activity = new CoordinatorActivity(activityType, coordinator);
            user.Activities.Add(activity);

            await UserManager.UpdateAsync(user);
            await UserDataContext.SaveChangesAsync();
            return Ok();
        }

        #endregion
    }
}
