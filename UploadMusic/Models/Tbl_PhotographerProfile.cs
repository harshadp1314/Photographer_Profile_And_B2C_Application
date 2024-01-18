//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UploadMusic.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tbl_PhotographerProfile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_PhotographerProfile()
        {
            this.Tbl_ImagePortFolio = new HashSet<Tbl_ImagePortFolio>();
            this.Tbl_VideoPortFolio = new HashSet<Tbl_VideoPortFolio>();
        }
    
        public int PhotographerID { get; set; }
        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public string PhotographerAddress { get; set; }
        public string Email { get; set; }
        public string About { get; set; }
        public string CurrentLocation { get; set; }
        public string AlsoShootIn { get; set; }
        public string ProductAndServices { get; set; }
        public string PaymentOption { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string Equipments { get; set; }
        public byte[] CoverImage { get; set; }
        public string ServiceDescription { get; set; }
        public string StudioName { get; set; }
        public byte[] Logo { get; set; }
        public string TeamSize { get; set; }
        public string Website { get; set; }
        public string Product { get; set; }
        public string ServiceOffered { get; set; }
        public string LanguageKnown { get; set; }
        public string YearOfExperience { get; set; }
        public string Achievement { get; set; }
        public byte[] OwnerPhotograph { get; set; }
        public string FacebookLink { get; set; }
        public string InstagramLink { get; set; }
        public string GoogleMap { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_ImagePortFolio> Tbl_ImagePortFolio { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_VideoPortFolio> Tbl_VideoPortFolio { get; set; }
    }
}
