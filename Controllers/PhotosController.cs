using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nysc.API.Data;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using Nysc.API.Models.Entities;
using Nysc.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        #region Properties

        #region Internals
        IMapper Mapper { get; }
        IConfiguration Configuration { get; }
        CloudSettings CloudSettings { get; }
        Cloudinary Cloudinary { get; }
        IResourceRepository Resources { get; }
        UserManager<User> UserManager { get; }
        UserDataContext UserDataContext { get; }
        #endregion

        #endregion

        #region Constructors
        public PhotosController(IConfiguration configuration, 
            UserManager<User> userManager, UserDataContext userDataContext, IMapper mapper, IResourceRepository resources)
        {
            UserManager = userManager;
            Configuration = configuration;
            Resources = resources;
            Mapper = mapper;
            CloudSettings = Configuration.GetSection("Storage:Cloud").Get<CloudSettings>();
            UserDataContext = userDataContext;

            Cloudinary = new Cloudinary(account: new Account(CloudSettings.CloudName
                , CloudSettings.Key, CloudSettings.Secret));
        }
        #endregion

        #region Methods

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            string userId = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(userId);

            if (user == null) return Unauthorized();

            var resourcePhoto = await Resources.GetPhoto(id);

            if (resourcePhoto == null) return BadRequest("The resquested photo was not found.");

            if (resourcePhoto.User.Id != user.Id)
                return Unauthorized();

            // TODO: Do not allow unauthorized users access.

            var photo = Mapper.Map<PhotoViewModel>(resourcePhoto);
            return Ok(photo);
        }

        [HttpPost("uploadPassport")]
        public async Task<IActionResult> UploadPassport([FromForm]PhotoUploadViewModel uploadPhoto)
        {
            string id = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);

            if (user == null) return Unauthorized();

            IFormFile file = uploadPhoto.File;
            UploadResult uploadResult = new ImageUploadResult();

            if (file.Length <= 0) return BadRequest("The provided file appears to be void.");

            // Upload the file to cloudinary
            using (var stream = file.OpenReadStream())
            {
                ImageUploadParams uploadParams =
                    new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(413).Height(531).
                            Crop("fill").Gravity("face")
                    };
                uploadResult = Cloudinary.Upload(uploadParams);
            }

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest("Failed to upload image.");

            uploadPhoto.Url = uploadResult.Uri.ToString();
            uploadPhoto.PublicID = uploadResult.PublicId;

            var photo = Mapper.Map<Photo>(uploadPhoto);
            photo.Type = PhotoType.Passport;

            // Load the photo collection
            await UserDataContext.Entry(user).Collection(u => u.Photos).LoadAsync();

            // Make sure there's only one passport in the photo collection.
            foreach (var resource in user.Photos.Where(p => p.Type == PhotoType.Passport && p.Active).ToList())
                resource.Active = false;

            user.Photos.Add(photo);

            await UserManager.UpdateAsync(user);
            await UserDataContext.SaveChangesAsync();

            var resultPhoto = Mapper.Map<PhotoViewModel>(photo);

            return CreatedAtRoute(nameof(GetPhoto), new { id = photo.Id }, resultPhoto);
        }

        [HttpPost("uploadSignature")]
        public async Task<IActionResult> UploadSignature([FromForm]PhotoUploadViewModel uploadPhoto)
        {
            string id = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);

            if (user == null) return Unauthorized();

            IFormFile file = uploadPhoto.File;
            UploadResult uploadResult = new ImageUploadResult();

            if (file.Length <= 0) return BadRequest("The provided file appears to be void.");
            using (var stream = file.OpenReadStream())
            {
                ImageUploadParams uploadParams =
                    new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).
                        Effect("grayscale").Gravity("object").Crop("fill")
                    };
                uploadResult = Cloudinary.Upload(uploadParams);
            }

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest("Failed to upload image.");

            uploadPhoto.Url = uploadResult.Uri.ToString();
            uploadPhoto.PublicID = uploadResult.PublicId;

            var photo = Mapper.Map<Photo>(uploadPhoto);
            photo.Type = PhotoType.Signature;

            // Load the photo collection
            await UserDataContext.Entry(user).Collection(u => u.Photos).LoadAsync();
            
            // Make sure there's only one signature in the photo collection.
            foreach (var resource in user.Photos.Where(p => p.Type == PhotoType.Signature && p.Active).ToList())
                resource.Active = false;

            user.Photos.Add(photo);

            await UserManager.UpdateAsync(user);
            var resultPhoto = Mapper.Map<PhotoViewModel>(photo);

            return CreatedAtRoute(nameof(GetPhoto), new { id = photo.Id }, resultPhoto);
        }
        #endregion
    }
}
