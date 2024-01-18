using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class RatingModel
    {
        public double NoOfStars { get; set; }
        public string Comment { get; set; }
        public string CommentedBy { get; set; }
        public string CommentedOn { get; set; }
        public int PhotographerID { get; set; }
        public string EncodedPhotographerID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string SecurityCode { get; set; }
    }
}