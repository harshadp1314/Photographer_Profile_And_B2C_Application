using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UploadMusic.Filters;
using UploadMusic.Models;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class PhotographerProfileController : Controller
    {
        // GET: PhotographerProfile
        [AllowAnonymous]
        //[Route("blog/album_design_at_pixthon")]
        public ActionResult Index()
    {
            return View();
        }

        public ActionResult Redirect(string objSecurityCode)
        {
            string ConnectionString = string.Empty;
            string url = string.Empty;
            string result = string.Empty;
            int IsProfileCompleted = 0;
            ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure


                PhotoGrapherDetail obj = new PhotoGrapherDetail();
                
                #region Getting RegistrationID From SecurityCode

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    
                    SqlCommand objSqlCommand = new SqlCommand("IsProfileCompleted", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_SecurityCode", objSecurityCode);
                    

                    SqlParameter @P_IsProfileCompleted = new SqlParameter("@P_IsProfileCompleted", SqlDbType.Int,10);
                    @P_IsProfileCompleted.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_IsProfileCompleted);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    int i = objSqlCommand.ExecuteNonQuery();

                    IsProfileCompleted = Convert.ToInt32(objSqlCommand.Parameters["@P_IsProfileCompleted"].Value);

                    


                }

                #endregion

                if(IsProfileCompleted == 1)
                {
                    obj = GetPhotographerDetails(objSecurityCode, null);
                    if (obj.PhotographerID != 0)
                    {

                        result = "Redirect";

                        url = Url.Action("PhotographerProfile", "PhotographerProfile", new { obj1 = objSecurityCode });
                    }
                    else
                    {
                        result = "Error";
                        url = Url.Action("Index", "PhotographerProfile");
                    }
                }
                else
                {
                    result = "CompleteProfile";
                    url = "";
                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            return Json(new { result = result, url = url });

        }

        [Route("PhotographerProfileRoute")]
        public ActionResult PhotographerProfile(string obj1, string PhotographerID)
        {
            if (string.IsNullOrEmpty(obj1) && string.IsNullOrEmpty(PhotographerID))
            {
                return HttpNotFound();
            }
            
            PhotographerProfileModel obj = new PhotographerProfileModel();
            Utility.Utility u = new Utility.Utility();
            if (PhotographerID != null)
            {
                bool port = checkingportfolio(Convert.ToInt32(Utility.Utility.Decode(PhotographerID)));
                if (port == false)
                {
                    //Portfolio Not Filled

                    return RedirectToAction("Registration", "Registration", new { id = PhotographerID });
                }
                if (CheckIfVerified(Convert.ToInt32(Utility.Utility.Decode(PhotographerID))) == true)
                {
                    int PhotographerID2 = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
                    obj.objPhotoGrapherDetail = GetPhotographerDetails(obj1, PhotographerID2);
                }
                else
                {
                    return RedirectToAction("Registration", "Registration", new { id = PhotographerID });

                }
            }
            else
            {
                obj.objPhotoGrapherDetail = GetPhotographerDetails(obj1, 0);
            }
            //if Login Registration id  matched Photographer ID s Registration iD then only show this
            //Get Registration iD of Login from cookie person || Admin  ||SuperAdmin 
            HttpCookie cookie = Request.Cookies["username"];
            if (cookie != null)
            {
                string Email = cookie.Value;
                int PhotographerIDCookie=getRegistrationID(Email);
                if(User.Identity.IsAuthenticated)
                {
                    if (PhotographerIDCookie == obj.objPhotoGrapherDetail.PhotographerID || User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                    {
                        
                            obj.Photobook = GetAllPhotobooks(obj.objPhotoGrapherDetail.PhotographerID);
                        
                    }
                }
               

            }
            
            obj.objPhotoGrapherDetail.EncodedPhotographerID = u.Encode(Convert.ToString(obj.objPhotoGrapherDetail.PhotographerID));
            obj.objrating = GetComments(obj.objPhotoGrapherDetail.PhotographerID);
            obj.PortFoliosImages = GetPortFolioOfPhotographer_Images(obj.objPhotoGrapherDetail.PhotographerID);
            obj.PortFoliosVideos = GetPortFolioOfPhotographer_Videos(obj.objPhotoGrapherDetail.PhotographerID);
            return View(obj);

        }

        public int getRegistrationID(string Email)
        {
            int RegistationID = 0;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select l.RegistrationID,p.PhotographerID,RoleID from logindetails l left join tbl_photographerprofile p on  l.Email=p.Email join UserRolesMapping u on l.RegistrationID=u.userID where p.Email='" + Email + "'", con))
                    {
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                RegistationID = Convert.ToInt32(rd["PhotographerID"]); 
                            }
                        }
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Message = ex.Message;
            }

            return RegistationID;
        }

        public List<PhotoBookModel> GetAllPhotobooks(int PhotographerID)
        {
            List<PhotoBookModel> photobookDetails = new List<PhotoBookModel>();
            PhotoBookModel photoBook = new PhotoBookModel();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT s.SecurityCode,Title,NoofViews,PhotographyCategoriesID,PhotoName,p.PhotoFileBytes,p.ImagePath FROM Tbl_SecurityCode  s JOIN Tbl_Photos p ON s.SecurityCode = p.SecurityCode  WHERE PhotographerID='" + PhotographerID + "' AND p.PhotoName='front'  ", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                photobookDetails.Add(new PhotoBookModel
                                {
                                    SecurityCode = Convert.ToString(rd["SecurityCode"]),
                                    Title = (Convert.ToString(rd["Title"])),
                                    NoofViews= Convert.ToInt32(rd["NoofViews"]),
                                    PhotographyCategoriesID= Convert.ToInt32(rd["PhotographyCategoriesID"]),
                                    PhotoFileBytes=(byte[])rd["PhotoFileBytes"]

                                });


                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Exception = ex.Message;
            }
            return photobookDetails;
        }

        public bool CheckIfVerified(int PhotographerID)
        {
            bool result = false;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select l.RegistrationID,p.PhotographerID,RoleID from logindetails l left join tbl_photographerprofile p on  l.Email=p.Email join UserRolesMapping u on l.RegistrationID=u.userID where PhotographerID='" + PhotographerID + "'", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            result = false;
                            while (rd.Read())
                            {
                                if (Convert.ToInt32(rd["RoleID"]) == 2)
                                {
                                    result = true;
                                }


                            }
                        }
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }

            return result;
        }

        public bool checkingportfolio(int PhotographerID)
        {
            bool status = false;
            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("checkingportfolioPhotographerID", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;


                    objSqlCommand.Parameters.AddWithValue("@PhotographerID", PhotographerID);



                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    objSqlConnection.Close();

                    adapt.Fill(dt);

                    string SuccessFlag = Convert.ToString(objSqlCommand.Parameters["@P_SUCCESSFLAG"].Value);
                    if (dt.Rows.Count > 0)
                    {
                        status = true;
                    }
                    else
                    {
                        status = false;
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
                status = false;
            }
            return status;
        }

        public PhotoGrapherDetail GetPhotographerDetails(string SecurityCode, int? PhotographerID)
        {
            PhotoGrapherDetail obj = new PhotoGrapherDetail();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    if (SecurityCode == null || SecurityCode == "")
                    {


                        obj.PhotographerID = Convert.ToInt32(PhotographerID);
                        cmd.CommandText = "GetPhotographerProfile_PhotographerID";
                        cmd.Parameters.AddWithValue("@PhotographerID", obj.PhotographerID);

                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                obj.Cover = (byte[])(sdr["CoverByte"]); ;
                                obj.Logo = Convert.ToString(sdr["Logo"]);
                                obj.PhotographerName = Convert.ToString(sdr["Name"]);
                                obj.About = Convert.ToString(sdr["About"]);
                                obj.AlsoShootIn = Convert.ToString(sdr["AlsoShootIn"]);
                                obj.CoverImage = Convert.ToString(sdr["CoverImage"]); ;
                                obj.CurrentLocation = Convert.ToString(sdr["CurrentLocation"]);
                                obj.Equipments = Convert.ToString(sdr["Equipments"]).Split(',');
                                obj.PaymentOption = Convert.ToString(sdr["PaymentOption"]).Split(',');
                                obj.PhotographerAddress = Convert.ToString(sdr["PhotographerAddress"]);
                                obj.ProductAndServices = Convert.ToString(sdr["ProductAndServices"]).Split(',');
                                obj.ServiceDescription = Convert.ToString(sdr["ServiceDescription"]);
                                obj.PhotographerID = Convert.ToInt32(sdr["PhotographerID"]);
                                obj.StudioName = Convert.ToString(sdr["StudioName"]);
                                obj.TeamSize = Convert.ToString(sdr["TeamSize"]);
                                obj.PhotographerEmail = Convert.ToString(sdr["Email"]);
                                obj.PhoneNo = Convert.ToString(sdr["PhoneNo"]);
                                obj.ServiceOffered = Convert.ToString(sdr["ServiceOffered"]).Split(',');
                                obj.Product = Convert.ToString(sdr["Product"]).Split(',');
                                obj.LanguageKnown = Convert.ToString(sdr["LanguageKnown"]);
                                obj.YearOfExperience = Convert.ToString(sdr["YearOfExperience"]);
                                obj.Achievement = Convert.ToString(sdr["Achievement"]);
                                obj.Website = Convert.ToString(sdr["Website"]);
                                obj.FacebookLink = Convert.ToString(sdr["FacebookLink"]);
                                obj.InstagramLink = Convert.ToString(sdr["InstagramLink"]);
                                obj.YoutubeLink = Convert.ToString(sdr["YoutubeLink"]);
                                obj.GoogleMap = Convert.ToString(sdr["GoogleMap"]);
                                obj.OwnerPhotograph = Convert.ToString(sdr["OwnerPhotograph"]);

                            }

                        }

                    }
                    else
                    {

                        cmd.CommandText = "GetPhotographerProfile_SecurityCode";
                        cmd.Parameters.AddWithValue("@SecurityCode", SecurityCode);

                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                obj.Logo = Convert.ToString(sdr["Logo"]);
                                obj.Title = Convert.ToString(sdr["Title"]);
                                obj.Cover = (byte[])(sdr["CoverByte"]); ;
                                obj.PhotographerName = Convert.ToString(sdr["Name"]);
                                obj.About = Convert.ToString(sdr["About"]);
                                obj.AlsoShootIn = Convert.ToString(sdr["AlsoShootIn"]);
                                obj.CoverImage = Convert.ToString(sdr["CoverImage"]); ;
                                obj.CurrentLocation = Convert.ToString(sdr["CurrentLocation"]);
                                obj.Equipments = Convert.ToString(sdr["Equipments"]).Split(',');
                                obj.PaymentOption = Convert.ToString(sdr["PaymentOption"]).Split(',');
                                obj.PhotographerAddress = Convert.ToString(sdr["PhotographerAddress"]);
                                obj.ProductAndServices = Convert.ToString(sdr["ProductAndServices"]).Split(',');
                                obj.ServiceDescription = Convert.ToString(sdr["ServiceDescription"]);
                                obj.SecurityCode = Convert.ToString(sdr["SecurityCode"]);
                                obj.PhotographerID = Convert.ToInt32(sdr["PhotographerID"]);
                                obj.StudioName = Convert.ToString(sdr["StudioName"]);
                                obj.TeamSize = Convert.ToString(sdr["TeamSize"]);
                                obj.PhotographerEmail = Convert.ToString(sdr["Email"]);
                                obj.PhoneNo = Convert.ToString(sdr["PhoneNo"]);
                                obj.ServiceOffered = Convert.ToString(sdr["ServiceOffered"]).Split(',');
                                obj.Product = Convert.ToString(sdr["Product"]).Split(',');
                                obj.LanguageKnown = Convert.ToString(sdr["LanguageKnown"]);
                                obj.YearOfExperience = Convert.ToString(sdr["YearOfExperience"]);
                                obj.Achievement = Convert.ToString(sdr["Achievement"]);
                                obj.Website = Convert.ToString(sdr["Website"]);
                                obj.FacebookLink = Convert.ToString(sdr["FacebookLink"]);
                                obj.InstagramLink = Convert.ToString(sdr["InstagramLink"]);
                                obj.YoutubeLink = Convert.ToString(sdr["YoutubeLink"]);
                                obj.GoogleMap = Convert.ToString(sdr["GoogleMap"]);
                                obj.OwnerPhotograph = Convert.ToString(sdr["OwnerPhotograph"]);
                                

                            }

                        }

                    }




                    con.Close();
                }

            }
            return obj;
        }

        public List<RatingModel> GetComments(int PhotographerID)
        {
            List<RatingModel> rate = new List<RatingModel>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetRatings", objSqlConnection);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", PhotographerID);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    //adapt.Fill(dt);

                    using (SqlDataReader rd = objSqlCommand.ExecuteReader())
                    {
                        while (rd.Read())
                        {

                            rate.Add(new RatingModel
                            {
                                NoOfStars = Convert.ToDouble(rd["NoOfStars"]),
                                Comment = Convert.ToString(rd["Comment"]),
                                CommentedBy = Convert.ToString(rd["CommentedBy"]),
                                CommentedOn = Convert.ToString(rd["CommentedOn"]).Substring(0, rd["CommentedOn"].ToString().IndexOf(" ")),
                                Title = Convert.ToString(rd["Title"]),
                                Name = Convert.ToString(rd["Name"]),
                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            return rate;
        }

        public List<PortFolioModel> GetPortFolioOfPhotographer_Images(int PhotographerID)
        {
            List<PortFolioModel> portFolios = new List<PortFolioModel>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetPhotographerPortfolio_Images";//"GetPhotographerPortfolio_Images";
                    cmd.Connection = con;

                    cmd.Parameters.AddWithValue("@PhotographerID", PhotographerID);
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            portFolios.Add(new PortFolioModel()
                            {

                                ImagesFile = Convert.ToString(sdr["ImagePath"])
                            });
                        }
                    }
                    con.Close();
                }



                return portFolios;
            }
        }

        public List<PortFolioModel> GetPortFolioOfPhotographer_Videos(int PhotographerID)
        {
            List<PortFolioModel> portFolios = new List<PortFolioModel>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetPhotographerPortfolio_Videos";
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@PhotographerID", PhotographerID);
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string iframevideo = Convert.ToString(sdr["VideoPath"]);
                            //HtmlDocument document = new HtmlDocument();
                            //document.LoadHtml(iframevideo);

                            //var image = document.DocumentNode.FirstChild;

                            //if (image.Attributes["width"] != null)
                            //{
                            //    image.Attributes["width"].Remove();
                            //    image.Attributes.Add("width", "560");
                            //}
                            //else
                            //{
                            //    image.Attributes.Add("width", "560");
                            //}
                            //if (image.Attributes["height"] != null)
                            //{
                            //    image.Attributes["height"].Remove();
                            //    image.Attributes.Add("height", "315");
                            //}
                            //else
                            //{
                            //    image.Attributes.Add("height", "315");
                            //}
                            portFolios.Add(new PortFolioModel()
                            {
                            
                            VideosFile = iframevideo
                            });
                        }
                    }
                    con.Close();
                }



                return portFolios;
            }
        }

        public ActionResult Enquiry(PhotographerProfileModel objEmail)
        {
            PhotoGrapherDetail obj = new PhotoGrapherDetail();
            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            string PixthonEmailmsg = "";
            string CustomerEmailmsg = "";
            string PhotographerEmailmsg = "";
            //string Smsmsg = "";
            string service = GetService(objEmail.EmailDetailsModel.Category);
            try
            {
                obj = GetPhotographerDetails("", objEmail.EmailDetailsModel.PhotographerID);
                string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];

                #region For Sending Email to Pixthon
                string PixthonMail = ConfigurationManager.AppSettings["EnquiryPixthon"];
                string mailbody = obj.StudioName + " has received an inquiry from " + objEmail.EmailDetailsModel.CustName + " for service " + service + "<br/> Customer name: " + objEmail.EmailDetailsModel.CustName + "<br/> Customer Email: " + objEmail.EmailDetailsModel.CustEmail + "<br/> Customer Contact No:" + objEmail.EmailDetailsModel.CustPhone + "<br/> Enquiry Message: " + objEmail.EmailDetailsModel.CustMsg + "<br/> Thanks"; ;
                string Subject = " Copy of Inquiry ";
                PixthonEmailmsg = utility.SendEmail(PixthonMail, EmailFrom, mailbody, Subject);
                #endregion

                #region For Sending Email to Photographer
                string PhotographerMail = obj.PhotographerEmail;
                string mailbodyPhotographer = "Hello,<br/> You have an enquiry for service " + service + "<br/> Customer name: " + objEmail.EmailDetailsModel.CustName + "<br/> Customer Email:" + objEmail.EmailDetailsModel.CustEmail + "<br/> Customer Contact No:" + objEmail.EmailDetailsModel.CustPhone + "<br/> Enquiry Message:" + objEmail.EmailDetailsModel.CustMsg + "<br/> Thanks";
                string SubjectPhotographer = "Enquiry Mail";
                PhotographerEmailmsg = utility.SendEmail(PhotographerMail, EmailFrom, mailbodyPhotographer, SubjectPhotographer);
                #endregion

                #region For Sending Email to Customer
                string CustomerMail = objEmail.EmailDetailsModel.CustEmail;
                string mailbodyCustomer = "Thank you for enquiring with " + obj.StudioName + ". We will get in touch with you shortly.<br/>Thanks for choosing us<br/><br/>Thanks,<br/>" + obj.PhotographerName + "<br/>" + obj.StudioName;
                string SubjectCustomer = "Thank you for your Inquiry ";
                CustomerEmailmsg = utility.SendEmail(CustomerMail, EmailFrom, mailbodyCustomer, SubjectCustomer);
                #endregion

                #region For Sending SMS
                //Smsmsg = utility.SendSMS(objEmail.EmailDetailsModel.Receipientno, objEmail.EmailDetailsModel.Msgtxt);
                #endregion

                if (PixthonEmailmsg == "Email Sent Successfully" && PhotographerEmailmsg == "Email Sent Successfully" && CustomerEmailmsg == "Email Sent Successfully")
                {
                    msg = "Email Sent Successfully";
                }

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                msg = "sent UNSuccessfully";
            }
            return Json(new { Message = msg });

        }

        public string GetService(string serviceid)
        {
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            string service = "";
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetService", objSqlConnection);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@PhotographyCategoriesID", Convert.ToInt32(serviceid));//1

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    //adapt.Fill(dt);

                    using (SqlDataReader rd = objSqlCommand.ExecuteReader())
                    {
                        while (rd.Read())
                        {

                            //Value = Convert.ToString(rd["PhotographyCategoriesID"]),
                            service = Convert.ToString(rd["Name"]);

                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            return service;
        }
        [HttpPost]
        public JsonResult GetPhotographyCategories(string Prefix)
        {

            List<SelectListItem> rate = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetPhotographyCategories", objSqlConnection);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;



                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    //adapt.Fill(dt);

                    using (SqlDataReader rd = objSqlCommand.ExecuteReader())
                    {
                        while (rd.Read())
                        {

                            rate.Add(new SelectListItem
                            {

                                Value = Convert.ToString(rd["PhotographyCategoriesID"]),
                                Text = Convert.ToString(rd["Name"]),

                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }

            return Json(rate, JsonRequestBehavior.AllowGet);


        }

        public ActionResult GetRatings(RatingModel rating)
        {
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("OverallRating", objSqlConnection);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", rating.PhotographerID);//1

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    //adapt.Fill(dt);

                    using (SqlDataReader rd = objSqlCommand.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            if (rd["Average"] == DBNull.Value)
                                rating.NoOfStars = 0;
                            else
                            rating.NoOfStars = Convert.ToDouble(rd["Average"]);
                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            return Json(new { response = rating }, JsonRequestBehavior.AllowGet);
            //return View("GetRating",rating.NoOfStars);
        }




    }
}