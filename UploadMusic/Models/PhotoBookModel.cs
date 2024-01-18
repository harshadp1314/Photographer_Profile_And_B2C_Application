using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UploadMusic.Models
{
    public class PhotoBookModel
    {
        [Required]
        [DataType(DataType.Upload)]
        [Display(Name = "Select File")]
        public HttpPostedFileBase files { get; set; }

        public string Parameter { get; set; }
        public string SecurityCode { get; set; }
        public string OrderNo { get; set; }
        public string Title { get; set; }
        public string PhotographerName { get; set; }
        public string PhotographerCategory { get; set; }
        public string Size { get; set; }
        public string StudioName { get; set; }
        public int PhotographerId { get; set; }
        public int PhotographyCategoriesID { get; set; }
        public int SizeID { get; set; }
        public string Email { get; set; }
        public string FolderPath { get; set; }
        public int NoofViews { get; set; }
        public int TypeofPhotobook { get; set; }
        
        public byte[] PhotoFileBytes { get; set; }


        public int AllowDelete { get; set; }
        public List<SelectModel> PhotographerList { get; set; }
        public List<SizeModel> SizeList { get; set; }
        



    }

    public class SelectModel
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string Selected { get; set; }
        
    }

    public class SizeModel
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string Selected { get; set; }

    }



    public class FileDetailsModel
    {
        public int Id { get; set; }
        [Display(Name = "Uploaded File")]
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public byte[] FileData { get; set; }
        public string PinCode { get; set; }

    }

    public class PhotoDetailsModel
    {
        public List<FileDetailsModel> photolist { get; set; }
        public FileDetailsModel front { get; set; }
        public FileDetailsModel back { get; set; }
        public string PinCode { get; set; }
        public string EncodedPhotographerID { get; set; }
        public int PhotographerID { get; set; }
        public string PhotographerName { get; set; }
        public string StudioName { get; set; }
        public string Title { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public int page_height { get; set; }
        public int page_width { get; set; }
        public int NoofViews { get; set; }
        public int TypeofPhotobook { get; set; }
    }

    public class PhotographerAuto
    {
        public int PhotographerID { get; set; }
        public string PhotographerName { get; set; }
    }

    public class StudioAuto
    {
        public int PhotographerID { get; set; }
        public string StudioName { get; set; }
    }

    public class StateAuto
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
    }

    public class CityAuto
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
    }

    public class CategoriesAuto
    {
        public int PhotographyCategoriesID { get; set; }
        public string Name { get; set; }
    }

    public class PhotoTypeAuto
    {
        public int PhotographyTypeID { get; set; }
        public string Name { get; set; }
    }

    
    //public class EmailDetails
    //{
    //    public string From { get; set; }
    //    public string To { get; set; }
    //    public string Receipientno { get; set; }
    //    public string Msgtxt { get; set; }

    //}



}