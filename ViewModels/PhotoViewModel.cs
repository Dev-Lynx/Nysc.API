﻿using Nysc.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.ViewModels
{
    public class PhotoViewModel
    {
        public PhotoType Type { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string UserIdentity { get; set; }
    }
}
