using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class HireModel
    {
        public string TypeOfEvent { get; set; }
        public string DateOfEvent { get; set; }
        public string PlaceOfEvent { get; set; }
        public string TimeOfEvent { get; set; }
        public string Message { get; set; }
        public List<SelectModel> TypeOfPhotohraphy { get; set; }
        public string TypeOfPhotography { get; set; }
        public int PhotographyCategoriesID { get; set; }
        public string Parameter { get; set; }
        public string Email { get; set; }
        public int HireID { get; set; }
        public int RegistrationID { get; set; }
        public List<string> selectedphotographers { get; set; }
        public string PhotographerType { get; set; }

    }
}