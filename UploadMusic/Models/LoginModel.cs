using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class LoginModel
    {
        public int RegistrationID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Contact { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEmailVerified { get; set; }
        public string ActivationCode { get; set; }
        public string ResetPasswordCode { get; set; }
        public bool RememberMe { get; set; }
        
    }
}