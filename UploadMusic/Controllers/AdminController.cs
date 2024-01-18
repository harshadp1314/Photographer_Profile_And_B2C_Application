using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using UploadMusic.Models;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult AdminPage()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Register()
        {
            return View("Register"); //Made change here
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult ManageAdmin()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult HiredPhotographers()
        {
            return View();
        }

        #region Manage Admin
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult GetAdmin()
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<AdminDetailModel> photobookDetails = new List<AdminDetailModel>();
            Utility.Utility u = new Utility.Utility();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select RegistrationID,Email,Name,Contact from logindetails l join UserRolesMapping roles on l.RegistrationID=roles.UserID where RoleID=1", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                photobookDetails.Add(new AdminDetailModel
                                {
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]),
                                    Email = Convert.ToString(rd["Email"]),
                                    Contact = Convert.ToString(rd["Contact"]),
                                    Name = Convert.ToString(rd["Name"]),
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

            return Json(new { data = photobookDetails }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult DeleteAdmin(int id)
        {
            Utility.Utility u = new Utility.Utility();
            bool status = false;
           // string email = "";
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();
            SqlTransaction tran = null;

            if (id != 0)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();
                        tran = con.BeginTransaction();

                        using (SqlCommand cmd = new SqlCommand("DELETE FROM UserRolesMapping WHERE userID='" + id + "' ", con))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                            status = false;

                        }

                        using (SqlCommand cmd = new SqlCommand("DELETE FROM logindetails WHERE RegistrationID='" + id + "' ", con))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                            status = true;

                        }

                        tran.Commit();

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    if (tran != null)
                        tran.Rollback();
                    status = false;
                    string Exception = ex.Message;
                }
            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult AllowDeletePhotographerRequest()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult AllowDeletePhotoBookRequest()
        {

            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Get_DeletePhotoBookRequest()
        {

            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoBookModel> photobookDetails = new List<PhotoBookModel>();
            Utility.Utility u = new Utility.Utility();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Select SecurityCode,Title,OrderNo,pp.Name as PhotograherName,pp.StudioName,pp.Email,pc.Name as Category,TypeofOrientation,s.AllowDelete FROM tbl_securitycode s join tbl_photographerprofile pp on s.PhotographerID=pp.PhotographerID join Tbl_PhotographyCategories pc on s.PhotographyCategoriesID=pc.PhotographyCategoriesID join tbl_orientation o on o.OrientationID=s.sizeID where s.AllowDelete=1", con))
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
                                    OrderNo = Convert.ToString(rd["OrderNo"]),
                                    PhotographerName = Convert.ToString(rd["PhotograherName"]),
                                    StudioName = Convert.ToString(rd["StudioName"]),
                                    Email = Convert.ToString(rd["Email"]),
                                    PhotographerCategory = Convert.ToString(rd["Category"]),
                                    Size = Convert.ToString(rd["TypeofOrientation"]),
                                    AllowDelete = Convert.ToInt32(rd["AllowDelete"])
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

            return Json(new { data = photobookDetails }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Get_DeletePhotographerRequest()
        {

            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            Utility.Utility u = new Utility.Utility();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Select PhotographerID,Name,PhoneNo,PhotographerAddress,Email,About,CurrentLocation,RegistrationID,AllowDelete FROM Tbl_PhotographerProfile WHERE AllowDelete=1", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                photoGrapherDetails.Add(new PhotoGrapherDetail
                                {
                                    PhotographerID = Convert.ToInt32(rd["PhotographerID"]),
                                    EncodedPhotographerID = u.Encode(Convert.ToString(rd["PhotographerID"])),
                                    PhotographerName = rd["Name"].ToString(),
                                    PhoneNo = rd["PhoneNo"].ToString(),
                                    PhotographerAddress = rd["PhotographerAddress"].ToString(),
                                    PhotographerEmail = Convert.ToString(rd["Email"]),
                                    About = Convert.ToString(rd["About"]),
                                    CurrentLocation = Convert.ToString(rd["CurrentLocation"]),
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]),
                                    AllowDelete = Convert.ToInt32(rd["AllowDelete"])
                                });


                            }
                        }

                    }

                    for (int i = 0; i < photoGrapherDetails.Count; i++)
                    {

                        if (CheckIfVerified(photoGrapherDetails[i].PhotographerID) == true)
                        {
                            photoGrapherDetails[i].IsEmailVerified = 1;
                        }
                        else
                        {
                            photoGrapherDetails[i].IsEmailVerified = 0;
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

            return Json(new { data = photoGrapherDetails }, JsonRequestBehavior.AllowGet);
        }

        #region Getting And Deleting Hired Photographers

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult GetHiredPhotographers()
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<HireModel> hiredPhotographersList = new List<HireModel>();
             
            try
            {

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {

                    objSqlConnection.Open();
                    
                    SqlCommand cmd = new SqlCommand("GetHiredPhotographers", objSqlConnection);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtable = new DataTable();
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(@P_SuccessMessage);

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        
                        string photographerdetails = "";
                        
                        
                        //Get All photographer details
                        
                        while (sdr.Read())
                        {
                            List<string> detailarr = new List<string>();
                            if (sdr["selectedphotographers"] == null || (sdr["selectedphotographers"]) == DBNull.Value)
                            {
                                photographerdetails = "<strong style='font-size:smaller'><u>Current Location</u> :</strong>" + "-" + "<br/><strong style='font-size:smaller'><u>Contact Details</u> :</strong>" + "-" + "<br/><strong style='font-size:smaller'><u>EmailID</u> :</strong>" + "-";
                                detailarr.Add(photographerdetails);
                                
                            }
                            else
                            {
                                string[] info = Convert.ToString(sdr["selectedphotographers"]).Split(',');
                                foreach (string id in info)
                                {
                                    using (SqlCommand cmd1 = new SqlCommand("Select PhotographerID,Name,StudioName,PhoneNo,PhotographerAddress,Email,About,CurrentLocation,AlsoShootIn,ProductAndServices FROM Tbl_PhotographerProfile where PhotographerID=" + id, objSqlConnection))
                                    {
                                        using (SqlDataReader rd = cmd1.ExecuteReader())
                                        {
                                            while (rd.Read())
                                            {

                                                photographerdetails = "<strong style='font-size:smaller'><u>Current Location</u> :</strong>" + rd["CurrentLocation"].ToString() + "<br/><strong style='font-size:smaller'><u>Contact Details</u> :</strong>" + rd["PhoneNo"].ToString() + "<br/><strong style='font-size:smaller'><u>EmailID</u> :</strong>" + Convert.ToString(rd["Email"]);
                                                detailarr.Add(photographerdetails);

                                            }
                                        }
                                    }
                                   
                                    
                                }
                            }

                            
                            hiredPhotographersList.Add(new HireModel {

                            HireID = Convert.ToInt32(sdr["HireID"]),
                            Email = "<strong style='font-size:smaller'><u>Address</u> :</strong>" + Convert.ToString(sdr["PhotographerAddress"]) + "<br/><strong style='font-size:smaller'><u>Contact Details</u> :</strong>" + Convert.ToString(sdr["PhoneNo"]) +" /"+ Convert.ToString(sdr["Phone2"]) + "<br/><strong style='font-size:smaller'><u>EmailID</u> :</strong>" + Convert.ToString(sdr["Email"]),
                            TypeOfEvent = Convert.ToString(sdr["TypeOfEvent"]),
                            PlaceOfEvent = "<strong style='font-size:smaller'><u>Place of Event</u> :</strong>" + Convert.ToString(sdr["PlaceOfEvent"]) + "<br/><strong style='font-size:smaller'><u>Date of Event</u> :</strong>" + Convert.ToDateTime(sdr["DateOfEvent"]).ToString("dd-MM-yyyy") + "<br/><strong style='font-size:smaller'><u>Time of Event</u> :</strong>" + Convert.ToString(sdr["TimeOfEvent"]),
                            TypeOfPhotography = Convert.ToString(sdr["TypeOfPhotography"]),
                            PhotographerType = Convert.ToString(sdr["PhotographerType"]),
                            Message= Convert.ToString(sdr["Message"]),    
                            selectedphotographers = detailarr
                        });
                           

                        }
                    }

                    objSqlConnection.Close();
                    

                }
            }
            catch (Exception ex)
            {
                string Exception = ex.Message;
            }

            return Json(new { data = hiredPhotographersList }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult DeleteHiredPhotographer(string id)
        {
            Utility.Utility u = new Utility.Utility();
            bool status = false;
            //string email = "";
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();

            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM HirePhotographer WHERE HireID='" + id + "' ", con))
                        {

                            cmd.ExecuteNonQuery();
                            status = true;

                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    status = false;
                    string Exception = ex.Message;
                }
            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region PhotoBook Details

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult PhotoBookTable()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult GetPhotoBook()
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoBookModel> photobookDetails = new List<PhotoBookModel>();
            Utility.Utility u = new Utility.Utility();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Select SecurityCode,Title,OrderNo,pp.Name as PhotograherName,pp.StudioName,pp.Email,pc.Name as Category,TypeofOrientation,s.AllowDelete FROM tbl_securitycode s join tbl_photographerprofile pp on s.PhotographerID=pp.PhotographerID join Tbl_PhotographyCategories pc on s.PhotographyCategoriesID=pc.PhotographyCategoriesID join tbl_orientation o on o.OrientationID=s.sizeID", con))
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
                                    OrderNo = Convert.ToString(rd["OrderNo"]),
                                    PhotographerName = Convert.ToString(rd["PhotograherName"]),
                                    StudioName = Convert.ToString(rd["StudioName"]),
                                    Email = Convert.ToString(rd["Email"]),
                                    PhotographerCategory = Convert.ToString(rd["Category"]),
                                    Size = Convert.ToString(rd["TypeofOrientation"]),
                                    AllowDelete=Convert.ToInt32(rd["AllowDelete"])
                                });


                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Exception = ex.Message;
            }

            return Json(new { data = photobookDetails }, JsonRequestBehavior.AllowGet);

        }
        
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult DeletePhotoBook(string id)
        {
            Utility.Utility u = new Utility.Utility();
            bool status = false;
            //string email = "";
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();
            SqlTransaction tran = null;

            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();
                        tran = con.BeginTransaction();
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Tbl_Photos WHERE SecurityCode='" + id + "' ", con))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                            status = false;

                        }
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Tbl_SecurityCode WHERE SecurityCode='" + id + "' ", con))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                            status = true;

                        }

                        tran.Commit();

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    if (tran != null)
                        tran.Rollback();
                    status = false;
                    string Exception = ex.Message;
                }
            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);

        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult RequestDeletePhotoBook(string id)
        {
            Utility.Utility u = new Utility.Utility();
            bool status = false;
            //string email = "";
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();

            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("Update Tbl_SecurityCode set AllowDelete=1 WHERE SecurityCode='" + id + "' ", con))
                        {

                            cmd.ExecuteNonQuery();
                            status = true;

                        }
                        
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    status = false;
                    string Exception = ex.Message;
                }
            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);

        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult RequestDeletePhotographer(string id)
        {
            Utility.Utility u = new Utility.Utility();
            bool status = false;


            if (!string.IsNullOrEmpty(id))
            {
                int RegistrationID = 0;
                string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
                PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();
                int photographerId = Convert.ToInt32(Utility.Utility.Decode(id));
                SqlTransaction tran = null;
                try
                {
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();
                        tran = con.BeginTransaction();
                        using (SqlCommand cmd = new SqlCommand("Select RegistrationID FROM Tbl_PhotographerProfile WHERE PhotographerID='" + photographerId + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save1");
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                }
                            }
                            status = false;
                        }
                        using (SqlCommand cmd = new SqlCommand("Update Tbl_PhotographerProfile SET AllowDelete=1 WHERE RegistrationID='" + RegistrationID + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save2");
                            cmd.ExecuteNonQuery();
                            status = true;

                        }

                       
                        tran.Commit();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    tran.Rollback();
                    status = false;
                    string Exception = ex.Message;
                }

            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);

        }


        #endregion

        #region Cancel Request

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult CancelRequest(string PhotographerID,string SecurityCode)
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            bool status = false;
            Utility.Utility u = new Utility.Utility();
            SqlTransaction tran = null;
            int RegistrationID = 0;
            

            if (!string.IsNullOrEmpty(PhotographerID))
            {
                int photographerId = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
                try
                {
                    SqlConnection con = new SqlConnection(st);
                    con.Open();
                    tran = con.BeginTransaction();
                    using (SqlCommand cmd = new SqlCommand("Select RegistrationID FROM Tbl_PhotographerProfile WHERE PhotographerID='" + photographerId + "' ", con))
                    {
                        cmd.Transaction = tran;
                        tran.Save("save1");
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                            }
                        }
                        status = false;
                    }
                    using (SqlCommand cmd = new SqlCommand("Update Tbl_PhotographerProfile SET AllowDelete=0 WHERE RegistrationID='" + RegistrationID + "' ", con))
                    {
                        cmd.Transaction = tran;
                        tran.Save("save2");
                        cmd.ExecuteNonQuery();
                        status = true;
                    }
                    
                    tran.Commit();
                    con.Close();
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    string Exception = ex.Message;
                }
            }
            if(!string.IsNullOrEmpty(SecurityCode))
            {
                try
                {
                
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();

                        SqlCommand cmd = new SqlCommand();
                        using (cmd = new SqlCommand("Update Tbl_SecurityCode set AllowDelete = 0 WHERE SecurityCode = '" + SecurityCode + "' ", con))
                        {
                            cmd.ExecuteNonQuery();
                            status = true;
                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    string Exception = ex.Message;
                }
                
            }

            return new JsonResult { Data = new { status = status } };
        }
        #endregion

        #region Create Admin
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult CreateAdmin(LoginModel obj)
        {
            SqlTransaction tran = null;
            //Check if email exists
            if (IsAlreadySignedUp(obj.Email) != false)
            {
                return Json(new { result = "ALready a Member" });
            }

            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    tran = objSqlConnection.BeginTransaction();

                    #region Insert LoginDetails Table
                    SqlCommand objSqlCommand = new SqlCommand("InsertLogin", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Transaction = tran;

                    objSqlCommand.Parameters.AddWithValue("@Email", obj.Email);
                    objSqlCommand.Parameters.AddWithValue("@Contact", Convert.ToString(obj.Contact));
                    objSqlCommand.Parameters.AddWithValue("@Name", obj.Name);
                    objSqlCommand.Parameters.AddWithValue("@Password", Crypto.Hash(obj.Password));
                    objSqlCommand.Parameters.AddWithValue("@ConfirmPassword", Crypto.Hash(obj.Password));
                    objSqlCommand.Parameters.AddWithValue("@IsProfileCompleted", 1);

                    SqlParameter @P_RegistrationID = new SqlParameter("@P_RegistrationID", SqlDbType.Int, 10);
                    @P_RegistrationID.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_RegistrationID);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    tran.Save("save1");
                    //adapt.Fill(dt);
                    int i = objSqlCommand.ExecuteNonQuery();
                    string SuccessFlag = Convert.ToString(objSqlCommand.Parameters["@P_SUCCESSFLAG"].Value);
                    int RegistrationID = Convert.ToInt32(objSqlCommand.Parameters["@P_RegistrationID"].Value);
                    #endregion

                    #region Insert  UserRoleMapping

                    SqlCommand objSqlCmd = new SqlCommand("Insert_UserRoleMapping", objSqlConnection);
                    SqlDataAdapter ad = new SqlDataAdapter(objSqlCmd);
                    DataTable dt2 = new DataTable();
                    objSqlCmd.CommandType = CommandType.StoredProcedure;
                    objSqlCmd.Transaction = tran;

                    objSqlCmd.Parameters.AddWithValue("@P_RegistrationID", RegistrationID);
                    objSqlCmd.Parameters.AddWithValue("@P_RoleID", 1);

                    SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag2.Direction = ParameterDirection.Output;
                    objSqlCmd.Parameters.Add(@P_SuccessFlag2);

                    SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                    @P_SuccessMessage2.Direction = ParameterDirection.Output;
                    objSqlCmd.Parameters.Add(@P_SuccessMessage2);
                    int j = objSqlCmd.ExecuteNonQuery();
                    string SuccessFlag2 = Convert.ToString(objSqlCmd.Parameters["@P_SUCCESSFLAG"].Value);

                    #endregion

                    #region Generate Activation Code 
                    var ActivationCode = Guid.NewGuid();
                    #endregion

                    //Send Email to User
                    
                    SendVerificationLinkEmail(obj.Email.Trim(), Convert.ToString(ActivationCode));
                    string message = "Registration successfully done. Account activation link " +
                        " has been sent to your email id:" + obj.Email.Trim();

                    //update in logindetails isemailverifies 0 and activation code

                    using (SqlCommand cmd = new SqlCommand("Update logindetails SET IsEmailVerified=0 , ActivationCode='" + ActivationCode.ToString() + "' WHERE RegistrationID='" + RegistrationID + "' ", objSqlConnection))
                    {
                        cmd.Transaction = tran;
                        tran.Save("save2");
                        cmd.ExecuteNonQuery();
                        //status = true;
                    }

                    if (SuccessFlag == "Y" && SuccessFlag2 == "Y")
                    {
                        tran.Commit();
                        return Json(new { result = "Redirect", url = Url.Action("AdminPage", "Admin") });
                    }
                    else
                    {
                        tran.Rollback();
                        return Json(new { result = "Invalid", msg = "Failed To Register Admin" });
                    }
                }

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                if (tran != null)
                    tran.Rollback();
                string Message = ex.Message;
                return Json(new { result = "Invalid", msg = "Failed To Register Admin" });
            }
        }
        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {

            var verifyUrl = "";
            if (emailFor != "VerifyAccount")
            {
                verifyUrl = "/ForgotPassword/" + emailFor + "/" + activationCode;
            }
            else
            {
                verifyUrl = "/Login/" + emailFor + "/" + activationCode;
            }

            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"].ToString();
            var fromEmail = new MailAddress(EmailFrom, "Pixthon");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = ConfigurationManager.AppSettings["Password"].ToString();  // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your Pixthon account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                //body = "Hi,<br/><br/>We got request for reset your account password. Please click on the below link to reset your password" +
                //    "<br/><br/><a href=" + link + ">Reset Password link</a>";
                body = "Hi,<br/><br/>You recently requested to reset your password for your Pixthon account.<br/><br/>Please click on the below link to reset your password" +
                         "<br/><br/><a href=" + link + ">Reset Password link</a>" +
                       "<br/><br/>Using this you can login and then reset your password from the account itself." +
                       "<br/><br/>Further, if you did not request a password reset, ignore this email." +
                        "<br/><br/>Thanks," + "<br/>Pixthon Digital Solutions";
            }

            //client.Host = "smtpout.asia.secureserver.net";//hosting.secureserver.net"smtp.gmail.com";
            //client.EnableSsl = true;
            //NetworkCredential basicCredential1 = new System.Net.NetworkCredential(From, Password);
            //client.UseDefaultCredentials = false;
            //client.Port = 25;
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.Credentials = basicCredential1;

            var smtp = new SmtpClient
            {
                Host = "smtpout.asia.secureserver.net",
                Port = 25,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        //Checking If Admin Already Exists

        public bool IsAlreadySignedUp(string EmailId)
        {
            bool result = false;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("IsAlreadySignedUpPhotographer", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_EmailID", EmailId);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    adapt.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
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

            return result;
        }

        //
        #endregion

        #region Photographer Details
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult GetData()
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            Utility.Utility u = new Utility.Utility();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Select p.PhotographerID,p.Name,p.PhoneNo,p.PhotographerAddress,p.Email,p.About,p.CurrentLocation,p.RegistrationID,AllowDelete,l.IsProfileCompleted,p.imageportfolio,l.IsEmailVerified FROM Tbl_PhotographerProfile p join logindetails l on p.RegistrationID = l.RegistrationID", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                int emailactivation = (rd["IsEmailVerified"]) == DBNull.Value ? 0 : Convert.ToInt32(rd["IsEmailVerified"]);
                                photoGrapherDetails.Add(new PhotoGrapherDetail
                                {
                                    PhotographerID = Convert.ToInt32(rd["PhotographerID"]),
                                    EncodedPhotographerID = u.Encode(Convert.ToString(rd["PhotographerID"])),
                                    PhotographerName = Convert.ToString(rd["Name"]),
                                    PhoneNo = (rd["PhoneNo"]) == DBNull.Value? "": Convert.ToString(rd["PhoneNo"]),
                                    PhotographerAddress = Convert.ToString(rd["PhotographerAddress"]),
                                    PhotographerEmail = Convert.ToString(rd["Email"]),
                                    About = Convert.ToString(rd["About"]),
                                    CurrentLocation = Convert.ToString(rd["CurrentLocation"]),
                                    RegistrationID = (rd["RegistrationID"]) == DBNull.Value ? 0 : Convert.ToInt32(rd["RegistrationID"]),
                                    AllowDelete= (rd["AllowDelete"]) == DBNull.Value ? 0 : Convert.ToInt32(rd["AllowDelete"]),
                                    IsProfileCompleted= (rd["IsProfileCompleted"]) == DBNull.Value ? 0 : Convert.ToInt32(rd["IsProfileCompleted"]),
                                    imageportfolio = (rd["imageportfolio"]) == DBNull.Value ? 0 : Convert.ToInt32(rd["imageportfolio"]),
                                    EmailActivation= emailactivation
                                });


                            }
                        }

                    }

                    for(int i=0;i<photoGrapherDetails.Count;i++)
                    {
                        
                        if(CheckIfVerified(photoGrapherDetails[i].PhotographerID) == true)
                        {
                            photoGrapherDetails[i].IsEmailVerified = 1;
                        }
                        else
                        {
                            photoGrapherDetails[i].IsEmailVerified = 0;
                        }



                    }

                    

                    //using (SqlCommand cmd = new SqlCommand("Select IsEmailVerified FROM logindetails", con))
                    //{
                    //    //cmd.Parameters.AddWithValue("@Id", Id);
                    //    using (SqlDataReader rd = cmd.ExecuteReader())
                    //    {
                    //        //int i = 0;
                    //        while (rd.Read())
                    //        {

                    //            photoGrapherDetails[i].IsEmailVerified = Convert.ToInt32(rd["IsEmailVerified"]);
                    //            i++;
                    //        }
                    //    }

                    //}
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Exception = ex.Message;
            }

            return Json(new { data = photoGrapherDetails }, JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(string id)
        {
            Utility.Utility u = new Utility.Utility();
            bool status = false;


            if (!string.IsNullOrEmpty(id))
            {
                int RegistrationID = 0;
                string SecurityCode = string.Empty;
                string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
                List<string> SecurityCodeList = new List<string>();
                PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();
                int photographerId = Convert.ToInt32(Utility.Utility.Decode(id));
                SqlTransaction tran = null;
                SqlCommand cmd = new SqlCommand();
                try
                {
                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();
                        tran = con.BeginTransaction();
                        using (cmd = new SqlCommand("Select RegistrationID FROM Tbl_PhotographerProfile WHERE PhotographerID='" + photographerId + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save1");
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                }
                                
                            }
                            status = false;
                        }

                        #region Getting Security Code From PhotographerId

                        using (SqlCommand cmd1 = new SqlCommand("SELECT SecurityCode FROM Tbl_SecurityCode WHERE PhotographerID='" + photographerId + "' ", con))
                        {
                            cmd1.Transaction = tran;
                            using (SqlDataReader rd = cmd1.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    SecurityCode = Convert.ToString(rd["SecurityCode"]);

                                    SecurityCodeList.Add(SecurityCode);
                                    

                                }
                            }
                            status = false;

                        }
                        
                        #endregion

                        for (int i = 0; i < SecurityCodeList.Count; i++)
                        {
                            using (cmd = new SqlCommand("DELETE FROM Tbl_Photos WHERE SecurityCode='" + SecurityCode + "'  ", con))
                            {
                                cmd.Transaction = tran;
                                tran.Save("save1");
                                cmd.ExecuteNonQuery();
                                status = false;

                            }
                        }

                        #region  New Change For Solving Error Of Foreign Key 

                        using (cmd = new SqlCommand("DELETE FROM Tbl_ImagePortFolio WHERE PhotographerID='" + photographerId + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save2");
                            cmd.ExecuteNonQuery();
                            status = false;
                        }

                        using (cmd = new SqlCommand("DELETE FROM Tbl_VideoPortFolio WHERE PhotographerID='" + photographerId + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save2");
                            cmd.ExecuteNonQuery();
                            status = false;
                        }

                        using (cmd = new SqlCommand("DELETE FROM Tbl_Ratings WHERE PhotographerID='" + photographerId + "'  ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save3");
                            cmd.ExecuteNonQuery();
                            status = false;

                        }

                        using (cmd = new SqlCommand("DELETE FROM Tbl_SecurityCode WHERE PhotographerID='" + photographerId + "'  ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save4");
                            cmd.ExecuteNonQuery();
                            status = false;

                        }
                            
                        using (cmd = new SqlCommand("DELETE FROM Tbl_PhotographerProfile WHERE RegistrationID='" + RegistrationID + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save5");
                            cmd.ExecuteNonQuery();
                            status = false;

                        }

                        using (cmd = new SqlCommand("DELETE FROM UserRolesMapping WHERE userID='" + RegistrationID + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save6");
                            cmd.ExecuteNonQuery();
                            status = false;

                        }

                        using (cmd = new SqlCommand("DELETE FROM logindetails WHERE RegistrationID='" + RegistrationID + "' ", con))
                        {
                            cmd.Transaction = tran;
                            tran.Save("save7");
                            cmd.ExecuteNonQuery();
                            status = true;

                        }
                        tran.Commit();
                        con.Close();
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    tran.Rollback();
                    status = false;
                    string Exception = ex.Message;
                }

            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult UpdatePhotographer_VerificationStatus(string Email)
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            bool status = false;
            Utility.Utility u = new Utility.Utility();
            if (!string.IsNullOrEmpty(Email))
            {

                try
                {
                    //int Id = Convert.ToInt32(Utility.Utility.Decode(EncodedId));
                    int RegistrationID = 0;
                    int RoleID = 0;

                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();

                        SqlCommand cmd1 = new SqlCommand();
                        using (cmd1 = new SqlCommand("SELECT RegistrationID FROM logindetails WHERE Email = '" + Email + "' ", con))
                        {
                            using (SqlDataReader rd = cmd1.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                }
                            }



                        }

                        SqlCommand cmd = new SqlCommand();
                        using (cmd = new SqlCommand("SELECT RoleID FROM UserRolesMapping WHERE userID = '" + RegistrationID + "' ", con))
                        {
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    RoleID = Convert.ToInt32(rd["RoleID"]);
                                }
                            }


                            if (RoleID == 2)
                            {
                                status = false;
                            }
                            else
                            {
                                
                                using (cmd = new SqlCommand("UPDATE UserRolesMapping SET RoleID = 2 WHERE userID = '" + RegistrationID + "' ", con))
                                {
                                    cmd.ExecuteNonQuery();
                                    //send mail of verification
                                    #region For Sending Email to Customer for verification
                                    string CustomerEmailmsg = "";
                                    string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
                                    string CustomerMail = Email;
                                    Utility.Utility utility = new Utility.Utility();
                                    //string mailbodyCustomer = "Thank you for enquiring with " + obj.StudioName + ". We will get in touch with you shortly.<br/>Thanks for choosing us<br/><br/>Thanks,<br/>" + obj.PhotographerName + "<br/>" + obj.StudioName;
                                    string mailbodyCustomer = "Hi " + Email + ",<br/>";
                                    mailbodyCustomer += "Thank you for creating profile at Pixthon.<br/>";
                                    mailbodyCustomer += "We have received your details and your profile has been verified. Click Here To <a href='http://profile.pixthon.com/'>Login<a/><br/>";
                                    mailbodyCustomer += "<br/>Cheers,<br/>";
                                    mailbodyCustomer += "Pixthon Digital Solutions Pvt Ltd<br/>";

                                    string SubjectCustomer = "Pixthon | Profile Verified";
                                    CustomerEmailmsg = utility.SendEmail(CustomerMail, EmailFrom, mailbodyCustomer, SubjectCustomer);
                                    #endregion
                                    status = true;
                                }
                            }

                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    string Exception = ex.Message;
                }
            }

            return new JsonResult { Data = new { status = status } };
        }

        public ActionResult UpdatePhotographer_VerificationStatusEmail(string Email)
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            bool status = false;
            Utility.Utility u = new Utility.Utility();
            if (!string.IsNullOrEmpty(Email))
            {

                try
                {
                    //int Id = Convert.ToInt32(Utility.Utility.Decode(EncodedId));
                    int RegistrationID = 0;
                    

                    using (SqlConnection con = new SqlConnection(st))
                    {
                        con.Open();

                        SqlCommand cmd1 = new SqlCommand();
                        using (cmd1 = new SqlCommand("SELECT RegistrationID FROM logindetails WHERE Email = '" + Email + "' ", con))
                        {
                            using (SqlDataReader rd = cmd1.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                }
                            }



                        }

                        SqlCommand cmd = new SqlCommand();
                       using (cmd = new SqlCommand("UPDATE logindetails SET IsEmailVerified = 1 WHERE RegistrationID = '" + RegistrationID + "' ", con))
                                {
                                    cmd.ExecuteNonQuery();
                                    //send mail of verification
                                    #region For Sending Email to Customer for verification
                                    string CustomerEmailmsg = "";
                                    string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
                                    string CustomerMail = Email;
                                    Utility.Utility utility = new Utility.Utility();
                                    //string mailbodyCustomer = "Thank you for enquiring with " + obj.StudioName + ". We will get in touch with you shortly.<br/>Thanks for choosing us<br/><br/>Thanks,<br/>" + obj.PhotographerName + "<br/>" + obj.StudioName;
                                    string mailbodyCustomer = "Hi " + Email + ",<br/>";
                                    mailbodyCustomer += "Thank you for registering profile at Pixthon.<br/>";
                                    mailbodyCustomer += "Your email has been verified. Click Here To <a href='http://profile.pixthon.com/'>Login<a/><br/>";
                                    mailbodyCustomer += "<br/>Cheers,<br/>";
                                    mailbodyCustomer += "Pixthon Digital Solutions Pvt Ltd<br/>";

                                    string SubjectCustomer = "Pixthon | Verified";
                                    CustomerEmailmsg = utility.SendEmail(CustomerMail, EmailFrom, mailbodyCustomer, SubjectCustomer);
                                    #endregion
                                    status = true;
                                }
                        

                        
                       
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Exception = ex.Message;
                }
            }

            return new JsonResult { Data = new { status = status } };
        }
        #endregion

        #region Not Used

        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public ActionResult Save(int id)
        //{
        //    string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        //    List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
        //    PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();


        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(st))
        //        {
        //            con.Open();
        //            using (SqlCommand cmd = new SqlCommand("Select PhotographerID,Name,PhoneNo,PhotographerAddress,Email,About,CurrentLocation,AlsoShootIn,ProductAndServices FROM Tbl_PhotographerProfile where PhotographerID='" + id + "' ", con)) // 
        //            {
        //                //cmd.Parameters.AddWithValue("@Id", Id);
        //                using (SqlDataReader rd = cmd.ExecuteReader())
        //                {
        //                    while (rd.Read())
        //                    {
        //                        photoGrapherDetails.Add(new PhotoGrapherDetail
        //                        {
        //                            PhotographerID = Convert.ToInt32(rd["PhotographerID"]),
        //                            PhotographerName = rd["Name"].ToString(),
        //                            PhoneNo = rd["PhoneNo"].ToString(),
        //                            PhotographerAddress = rd["PhotographerAddress"].ToString(),
        //                            PhotographerEmail = Convert.ToString(rd["Email"]),
        //                            About = Convert.ToString(rd["About"]),
        //                            CurrentLocation = Convert.ToString(rd["CurrentLocation"]),
        //                            AlsoShootIn = Convert.ToString(rd["AlsoShootIn"]),


        //                        });

        //                        if (photoGrapherDetails == null)
        //                        {
        //                            photoGrapher.photoGrapherDetails[0].PhotographerID = 0;
        //                        }
        //                        else
        //                        {
        //                            photoGrapher.photoGrapherDetails = photoGrapherDetails;
        //                        }

        //                    }
        //                }
        //            }
        //            con.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string Exception = ex.Message;
        //    }

        //    return View("Save", photoGrapher);
        //}


        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult SavePhotographer(PhotoGrapherDetail photoGrapherDetail)
        //{
        //    string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        //    List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
        //    bool status = false;

        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(st))
        //        {
        //            con.Open();
        //            if (photoGrapherDetail.photoGrapherDetails[0].PhotographerID > 0)
        //            {
        //                //insert into Tbl_PhotographerProfile(Name, PhoneNo, PhotographerAddress, Email, About, CurrentLocation, AlsoShootIn, ProductAndServices) values('" + photoGrapherDetail.photoGrapherDetails[0].Name + "', '" + photoGrapherDetail.photoGrapherDetails[0].PhoneNo + "', '" + photoGrapherDetail.photoGrapherDetails[0].PhotographerAddress + "', '" + photoGrapherDetail.photoGrapherDetails[0].Email + "', '" + photoGrapherDetail.photoGrapherDetails[0].About + "', '" + photoGrapherDetail.photoGrapherDetails[0].CurrentLocation + "', '" + photoGrapherDetail.photoGrapherDetails[0].AlsoShootIn + "', '" + photoGrapherDetail.photoGrapherDetails[0].ProductAndServices + "') WHERE PhotographerID = '" + photoGrapherDetail.photoGrapherDetails[0].PhotographerID + "' "
        //                using (SqlCommand cmd = new SqlCommand("UPDATE Tbl_PhotographerProfile SET Name='" + photoGrapherDetail.photoGrapherDetails[0].PhotographerName + "',PhoneNo='" + photoGrapherDetail.photoGrapherDetails[0].PhoneNo + "',PhotographerAddress='" + photoGrapherDetail.photoGrapherDetails[0].PhotographerAddress + "',Email='" + photoGrapherDetail.photoGrapherDetails[0].PhotographerEmail + "',About='" + photoGrapherDetail.photoGrapherDetails[0].About + "',CurrentLocation='" + photoGrapherDetail.photoGrapherDetails[0].CurrentLocation + "',AlsoShootIn='" + photoGrapherDetail.photoGrapherDetails[0].AlsoShootIn + "',ProductAndServices='" + photoGrapherDetail.photoGrapherDetails[0].ProductAndServices + "' WHERE PhotographerID = '" + photoGrapherDetail.photoGrapherDetails[0].PhotographerID + "' ", con))
        //                {
        //                    cmd.ExecuteNonQuery();
        //                    status = true;
        //                }
        //            }
        //            else
        //            {
        //                using (SqlCommand cmd = new SqlCommand("insert into Tbl_PhotographerProfile (Name,PhoneNo,PhotographerAddress,Email,About,CurrentLocation,AlsoShootIn,ProductAndServices) values ('" + photoGrapherDetail.photoGrapherDetails[0].PhotographerName + "','" + photoGrapherDetail.photoGrapherDetails[0].PhoneNo + "','" + photoGrapherDetail.photoGrapherDetails[0].PhotographerAddress + "','" + photoGrapherDetail.photoGrapherDetails[0].PhotographerEmail + "','" + photoGrapherDetail.photoGrapherDetails[0].About + "','" + photoGrapherDetail.photoGrapherDetails[0].CurrentLocation + "','" + photoGrapherDetail.photoGrapherDetails[0].AlsoShootIn + "','" + photoGrapherDetail.photoGrapherDetails[0].ProductAndServices + "') ", con))
        //                {
        //                    cmd.ExecuteNonQuery();
        //                    status = true;
        //                }
        //            }

        //            con.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string Exception = ex.Message;
        //    }

        //    return new JsonResult { Data = new { status = status } };
        //}

        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public ActionResult DeletePhotographer(string id)
        //{
        //    Utility.Utility u = new Utility.Utility();
        //    bool status = false;
        //    if (!string.IsNullOrEmpty(id))
        //    {
        //        string email = "";
        //        string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        //        List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
        //        PhotoGrapherDetail photoGrapher = new PhotoGrapherDetail();
        //        int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
        //        try
        //        {
        //            using (SqlConnection con = new SqlConnection(st))
        //            {
        //                con.Open();
        //                using (SqlCommand cmd = new SqlCommand("DELETE FROM Tbl_PhotographerProfile WHERE PhotographerID='" + id + "' ", con))
        //                {

        //                    cmd.ExecuteNonQuery();
        //                    status = false;

        //                }
        //                using (SqlCommand cmd = new SqlCommand("Select Email FROM Tbl_PhotographerProfile WHERE PhotographerID='" + id + "' ", con))
        //                {

        //                    using (SqlDataReader rd = cmd.ExecuteReader())
        //                    {
        //                        while (rd.Read())
        //                        {
        //                            email = Convert.ToString(rd["Email"]);
        //                        }
        //                    }
        //                    status = false;
        //                }

        //                using (SqlCommand cmd = new SqlCommand("DELETE FROM logindetails WHERE email='" + email + "' ", con))
        //                {

        //                    cmd.ExecuteNonQuery();
        //                    status = true;

        //                }
        //                con.Close();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            status = false;
        //            string Exception = ex.Message;
        //        }
        //    }

        //    return Json(new { status = status });

        //}
        #endregion
    }
}