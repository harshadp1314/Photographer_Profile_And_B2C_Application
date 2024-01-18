using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class PhotographerProfileModel
    {
        
        public PhotoGrapherDetail objPhotoGrapherDetail { get; set; }
        public List<PortFolioModel> PortFoliosImages { get; set; }
        public List<PortFolioModel> PortFoliosVideos { get; set; }
        public List<RatingModel> objrating { get; set; }
        public EmailDetailsModel EmailDetailsModel { get; set; }
        public List<PhotoBookModel> Photobook{ get; set; }

    }
    public class PhotoGrapherDetail
    {
        public int RegistrationID { get; set; }
        public string EncodedPhotographerID { get; set; }
        public int PhotographerID { get; set; }
        public string SecurityCode { get; set; }
        public string Title { get; set; }
        public byte[] Cover { get; set; }
        public string OwnerPhotograph { get; set; }
        public string PhotographerName { get; set; }
        public string CurrentLocation { get; set; }
        public string PhotographerAddress { get; set; }
        public string AlsoShootIn { get; set; }
        public string About { get; set; }
        public string[] ProductAndServices { get; set; }
        public string[] PaymentOption { get; set; }
        public string CoverImage { get; set; }
        public string[] Equipments { get; set; }
        public string ServiceDescription { get; set; }
        public string TeamSize { get; set; }
        public string Website { get; set; }
        public string[] Product { get; set; }
        public string[]  ServiceOffered { get; set; }
        public string  LanguageKnown { get; set; }
        public string YearOfExperience { get; set; }
        public string Achievement { get; set; }
        public string Logo { get; set; }
        public string StudioName { get; set; }
        public string PhotographerEmail { get; set; }
        public string PhoneNo { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string GoogleMap { get; set; }
        public string YoutubeLink { get; set; }
        //New change for getting Verfied Photographer Status 
        public int IsEmailVerified { get; set; }

        public int EmailActivation { get; set; }

        public int AllowDelete { get; set; }
        public int IsProfileCompleted { get; set; }

        public int imageportfolio { get; set; }

        public List<PhotoGrapherDetail> photoGrapherDetails { get; set; }
    }

    public class PortFolioModel
    {
        //public byte[] ImagesFileBytes { get; set; }
        //public byte[] VideosFileBytes { get; set; }
        public string ImagesFile { get; set; }
        public string VideosFile { get; set; }
        public string Content { get; set; }
    }

    public class EmailDetailsModel
    {
        public string CustName { get; set; }
        public string CustEmail { get; set; }
        public string CustPhone { get; set; }
        public string CustMsg { get; set; }
        public string Category { get; set; }
        public int PhotographerID { get; set; }
        public string PhotographerEmail { get; set; }
        public string PixthonEmail { get; set; }
    }
}