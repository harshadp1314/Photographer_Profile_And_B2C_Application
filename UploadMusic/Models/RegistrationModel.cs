using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class RegistrationModel
    {
        public string Email { get; set; }
        public string PhotographerName { get; set; }
        public string StudioName { get; set; }
        public string Logo { get; set; }
        public string OwnerPhoto { get; set; }
        public string AboutMe { get; set; }
        public int YearOfExperience { get; set; }
        public string EventTypes { get; set; }
        public string Equipments { get; set; }
        public string PreferedLocation { get; set; }
        public string Achievements { get; set; }
        public string Products { get; set; }
        public string Website { get; set; }
        public string FacebookLink { get; set; }
        public string InstaLink { get; set; }
        public string YoutubeLink { get; set; }
        public string Contact { get; set; }
        public string Contact2 { get; set; }
        public string Address { get; set; }
        
        public List<PhotographyCategories> Services { get; set; }
        public List<TeamSize> TeamSize { get; set; }
        public List<Work> Work { get; set; }
        public string TeamSizestring { get; set; }
        public string WorkString { get; set; }

        public int PhotographerID { get; set; }
        public string EncodedPhotographerID { get; set; }
        public string CurrentLocation { get; set; }
        public string CurrentState { get; set; }
        public string[] PaymentOption { get; set; }
        public string CoverImage { get; set; }
        public string LanguageKnown { get; set; }
        public string GoogleMap { get; set; }
        public string FileDataImages { get; set; }
        public string  FileDataVideos { get; set; }

        public string PhotographyCategoriesstring { get; set; }
        public string SubWedding { get; set; }
        public string SubCorporate { get; set; }
        public string SubKid { get; set; }
        public string SubBirthday { get; set; }
        public string SubFashion { get; set; }
        public string SubCommercial { get; set; }
        public string SubWildlife { get; set; }
        public string SubOther { get; set; }
        public string ServiceProvide { get; set; }
       

    }

    public class FileDetail
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
    }
    public class PhotographyCategories
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PhotographySubCategories
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public bool IsChecked { get; set; }
    }

    public class TeamSize
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string IsChecked { get; set; }
    }

    public class Work
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string IsChecked { get; set; }
    }

    public class PhotographerType
    {
        public int ID { get; set; }
        public string PhotographyType { get; set; }
        public int CategoryID { get; set; }
        public bool IsChecked { get; set; }
    }
    
}