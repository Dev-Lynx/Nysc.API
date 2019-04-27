using AutoMapper;
using Nysc.API.Models;
using Nysc.API.Models.Entities;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.ViewModels.Mappings
{
    public class ViewModelToEntityProfile : Profile
    {
        #region Constructors
        public ViewModelToEntityProfile()
        {
            CreateMap<User, UserProfileViewModel>().ForMember(dest => dest.Passport, opt =>
            {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.Type == PhotoType.Passport && p.Active));
            }).ForMember(dest => dest.Signature, opt =>
            {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.Type == PhotoType.Signature && p.Active));
            });

            CreateMap<User, UserLoginViewModel>()
                .ForMember(dest => dest.Password, opt => opt.AllowNull())
                .ForMember(dest => dest.PhoneNumber, opt => opt.AddTransform(t => HidePhoneNumber(t, 5)));

            CreateMap<UserProfileViewModel, User>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.OneTimePassword, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore());

            CreateMap<PhotoUploadViewModel, Photo>().
                ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<Photo, PhotoViewModel>();
        }
        #endregion

        #region Methods
        string HidePhoneNumber(string phoneNumber, int hideLimit = 5)
        {
            PhoneNumberUtil numberUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber number = numberUtil.Parse(phoneNumber, "NG");
            phoneNumber = numberUtil.Format(number, PhoneNumberFormat.INTERNATIONAL);

            string resultNumber = "";
            int limit = phoneNumber.Length - 1;
            int hiddenCount = 0;

            for (int i = limit; i >= 0; i--)
            {
                // display the last two characters in the phone number
                if (i > limit - 2) resultNumber = phoneNumber[i] + resultNumber;
                // hide the next set of numbers till the limit is reached
                else if (char.IsNumber(phoneNumber[i]) && hiddenCount < hideLimit)
                {
                    hiddenCount++;
                    resultNumber = "*" + resultNumber;
                }
                // Add everything else
                else resultNumber = phoneNumber[i] + resultNumber;
            }
            return resultNumber;  
        }
        #endregion
    }
}
