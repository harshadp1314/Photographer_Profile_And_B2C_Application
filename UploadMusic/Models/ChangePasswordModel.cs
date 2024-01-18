using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class ChangePasswordModel
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string Flag { get; set; }
    }
}