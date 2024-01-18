using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using UploadMusic.Models;
using UploadMusic.Utility;


namespace UploadMusic.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        #region Declaration

        string FinalFileName = string.Empty;

        #endregion
        [Authorize(Roles = "Admin,VerifiedPhotographer,NonVerifiedPhotographer,SuperAdmin")]
        public ActionResult Registration(string id)
        {
            //int RegistrationID = 0;
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }
            Utility.Utility u = new Utility.Utility();
            RegistrationModel obj = new RegistrationModel();
            if (id == "0")
            {
                obj.PhotographerID = Convert.ToInt32(id);
                id = u.Encode(id);
                obj.EncodedPhotographerID = Convert.ToString(id);
            }

            int id2 = Convert.ToInt32(Utility.Utility.Decode(id));


            if (id2 != 0)
            {
                try
                {
                    string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {
                        objSqlConnection.Open();
                        SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", objSqlConnection);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataSet ds = new DataSet();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                        SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessFlag);

                        SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessMessage);
                        adapt.Fill(ds);

                        DataTable dtprofile = ds.Tables[0];
                        DataTable dtteam = ds.Tables[1];
                        DataTable dtservice = ds.Tables[2];
                        DataTable dtwork = ds.Tables[3];



                        if (dtprofile.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtprofile.Rows.Count; i++)
                            {
                                obj.PhotographerID = Convert.ToInt32(dtprofile.Rows[i]["PhotographerID"]);
                                obj.EncodedPhotographerID = u.Encode(Convert.ToString(obj.PhotographerID));
                                obj.PhotographerName = Convert.ToString(dtprofile.Rows[i]["Name"]);
                                obj.Contact = Convert.ToString(dtprofile.Rows[i]["PhoneNo"]);
                                obj.Contact2 = Convert.ToString(dtprofile.Rows[i]["Phone2"]);
                                obj.Address = Convert.ToString(dtprofile.Rows[i]["PhotographerAddress"]);
                                obj.Email = Convert.ToString(dtprofile.Rows[i]["Email"]);
                                obj.AboutMe = Convert.ToString(dtprofile.Rows[i]["About"]);
                                obj.CurrentState = Convert.ToString(dtprofile.Rows[i]["CurrentState"]);
                                obj.CurrentLocation = Convert.ToString(dtprofile.Rows[i]["CurrentLocation"]);
                                obj.PreferedLocation = Convert.ToString(dtprofile.Rows[i]["AlsoShootIn"]);
                                obj.Equipments = Convert.ToString(dtprofile.Rows[i]["Equipments"]);
                                obj.StudioName = Convert.ToString(dtprofile.Rows[i]["StudioName"]);
                                obj.Website = Convert.ToString(dtprofile.Rows[i]["Website"]);
                                obj.Products = Convert.ToString(dtprofile.Rows[i]["Product"]);
                                if (Convert.ToString(dtprofile.Rows[i]["YearOfExperience"]) == "")
                                {
                                    obj.YearOfExperience = 0;
                                }
                                else
                                {
                                    obj.YearOfExperience = Convert.ToInt32(dtprofile.Rows[i]["YearOfExperience"]);
                                }

                                obj.Achievements = Convert.ToString(dtprofile.Rows[i]["Achievement"]);
                                obj.FacebookLink = Convert.ToString(dtprofile.Rows[i]["FacebookLink"]);
                                obj.YoutubeLink = Convert.ToString(dtprofile.Rows[i]["YoutubeLink"]);
                                obj.InstaLink = Convert.ToString(dtprofile.Rows[i]["InstagramLink"]);
                                obj.GoogleMap = Convert.ToString(dtprofile.Rows[i]["GoogleMap"]);
                                obj.LanguageKnown = Convert.ToString(dtprofile.Rows[i]["LanguageKnown"]);
                                obj.Logo = Convert.ToString(dtprofile.Rows[i]["Logo"]);
                                obj.CoverImage = Convert.ToString(dtprofile.Rows[i]["CoverImage"]);
                                obj.OwnerPhoto = Convert.ToString(dtprofile.Rows[i]["OwnerPhotograph"]);
                            }




                        }
                        else
                        {

                        }

                        if (dtservice.Rows.Count > 0)
                        {
                            obj.Services = GetEventTypes(dtservice);
                        }
                        else
                        {
                            obj.Services = GetEventTypes(null);
                        }

                        if (dtwork.Rows.Count > 0)
                        {
                            obj.Work = GetWorkTypes(dtwork);
                        }
                        else
                        {
                            obj.Work = GetWorkTypes(null);
                        }

                        if (dtteam.Rows.Count > 0)
                        {
                            obj.TeamSize = GetTeamSizes(dtteam);

                        }
                        else
                        {
                            obj.TeamSize = GetTeamSizes(null);
                        }

                        objSqlConnection.Close();

                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Message = ex.Message;
                }


            }
            else
            {
                obj.Work = GetWorkTypes(null);
                obj.Services = GetEventTypes(null);
                obj.TeamSize = GetTeamSizes(null);
            }


            return View(obj);
        }

        [Authorize(Roles = "Admin,VerifiedPhotographer,NonVerifiedPhotographer,SuperAdmin")]
        [HttpPost]
        public ActionResult Submit(RegistrationModel obj)
        {
            SqlTransaction tran = null;
            int RegistrationID = 0;
            if (obj == null)
            {
                return Json(new { result = "Error2" });
            }
            int newPhotograpgerID = 0;
            int newid = 0;
            string actiontype = "";
            bool status = false;
            Utility.Utility u = new Utility.Utility();
            string SuccessFlag1 = "F";
            string SuccessFlag2 = "F";
            string SuccessFlag3 = "F";
            string SuccessFlag4 = "F";
            string SuccessFlag9 = "F";

            //Decide whether Photographer is new or already a Photographer


            string ConnectionString1 = string.Empty;
            try
            {
                ConnectionString1 = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString1))
                {
                    objSqlConnection.Open();
                    tran = objSqlConnection.BeginTransaction();
                    if (obj.EncodedPhotographerID == null)
                    {
                        obj.EncodedPhotographerID = u.Encode(Convert.ToString(obj.PhotographerID));
                    }
                    #region New Photographer By Admin

                    if (Convert.ToInt32(Utility.Utility.Decode(obj.EncodedPhotographerID)) == 0)
                    {
                        actiontype = "Insert";
                        newid = GetPhotographerID();
                        if (newid == 0)
                        {
                            return Json(new { result = "Error1" });
                        }
                        if (IsAlreadySignedUp(obj.Email.Trim()) != false)
                        {
                            return Json(new { result = "ALready a Member" });
                        }

                        string Password = GetPassword();


                        #region Insert LoginDetails Table
                        SqlCommand objSqlCommand5 = new SqlCommand("InsertLogin", objSqlConnection);
                        SqlDataAdapter adapt5 = new SqlDataAdapter(objSqlCommand5);
                        DataTable dt5 = new DataTable();
                        objSqlCommand5.CommandType = CommandType.StoredProcedure;
                        objSqlCommand5.Transaction = tran;

                        objSqlCommand5.Parameters.AddWithValue("@Email", obj.Email.Trim());
                        objSqlCommand5.Parameters.AddWithValue("@Contact", Convert.ToString(obj.Contact));
                        objSqlCommand5.Parameters.AddWithValue("@Name", obj.PhotographerName);
                        objSqlCommand5.Parameters.AddWithValue("@Password", Crypto.Hash(Password));
                        objSqlCommand5.Parameters.AddWithValue("@ConfirmPassword", Crypto.Hash(Password));
                        objSqlCommand5.Parameters.AddWithValue("@IsProfileCompleted", 1);

                        SqlParameter @P_RegistrationID = new SqlParameter("@P_RegistrationID", SqlDbType.Int, 10);
                        @P_RegistrationID.Direction = ParameterDirection.Output;
                        objSqlCommand5.Parameters.Add(@P_RegistrationID);

                        SqlParameter @P_SuccessFlag5 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag5.Direction = ParameterDirection.Output;
                        objSqlCommand5.Parameters.Add(@P_SuccessFlag5);

                        SqlParameter @P_SuccessMessage5 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                        @P_SuccessMessage5.Direction = ParameterDirection.Output;
                        objSqlCommand5.Parameters.Add(@P_SuccessMessage5);

                        tran.Save("save1");
                        //adapt.Fill(dt);
                        int i = objSqlCommand5.ExecuteNonQuery();
                        string SuccessFlag5 = Convert.ToString(objSqlCommand5.Parameters["@P_SUCCESSFLAG"].Value);
                        RegistrationID = Convert.ToInt32(objSqlCommand5.Parameters["@P_RegistrationID"].Value);
                        #endregion

                        #region Insert  UserRoleMapping

                        SqlCommand objSqlCmd = new SqlCommand("Insert_UserRoleMapping", objSqlConnection);
                        SqlDataAdapter ad = new SqlDataAdapter(objSqlCmd);
                        DataTable dt2 = new DataTable();
                        objSqlCmd.CommandType = CommandType.StoredProcedure;
                        objSqlCmd.Transaction = tran;

                        objSqlCmd.Parameters.AddWithValue("@P_RegistrationID", RegistrationID);
                        objSqlCmd.Parameters.AddWithValue("@P_RoleID", 2);

                        SqlParameter @P_SuccessFlag6 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag6.Direction = ParameterDirection.Output;
                        objSqlCmd.Parameters.Add(@P_SuccessFlag6);

                        SqlParameter @P_SuccessMessage6 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                        @P_SuccessMessage6.Direction = ParameterDirection.Output;
                        objSqlCmd.Parameters.Add(@P_SuccessMessage6);
                        int j = objSqlCmd.ExecuteNonQuery();
                        string SuccessFlag6 = Convert.ToString(objSqlCmd.Parameters["@P_SUCCESSFLAG"].Value);

                        #endregion

                        if (SuccessFlag5 == "Y" && SuccessFlag6 == "Y")
                        {
                            //tran.Commit();
                            //return Json(new { result = "Redirect", url = Url.Action("Index", "Login") });
                        }
                        else
                        {
                            tran.Rollback();
                            return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
                        }

                        #region SendEmailAsPassword
                        status = SendPasswordEmail(obj.Email, Password);
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
                        // Status = true;

                    }
                    #endregion
                    #region Already a Photographer
                    else
                    {

                        actiontype = "Update";
                        newid = Convert.ToInt32(Utility.Utility.Decode(obj.EncodedPhotographerID));
                    }
                    #endregion


                    #region CheckBoxes of Team and Services and GoogleMap
                    string teamname = "";

                    int count = 1;
                    var model = JsonConvert.DeserializeObject<List<TeamSize>>(obj.TeamSizestring);
                    //string iframegooglemap = HttpUtility.HtmlDecode(obj.GoogleMap);
                    string iframegooglemap = HttpUtility.UrlDecode(obj.GoogleMap);
                    obj.GoogleMap = iframegooglemap;
                    foreach (TeamSize team in model)
                    {
                        teamname += team.Name;

                        if (count != model.Count)
                        {
                            teamname += ",";
                        }
                    }

                    string serviceProvided = "";
                    int count1 = 1;
                    var model1 = JsonConvert.DeserializeObject<List<Work>>(obj.WorkString);

                    foreach (Work service in model1)
                    {
                        serviceProvided += service.Name;

                        if (count1 != model1.Count)
                        {
                            serviceProvided += ",";
                        }
                        else
                        {
                            break;
                        }
                        count1++;
                    }
                    #endregion
                    #region Categories and SubCategories
                    string servicename = obj.PhotographyCategoriesstring;
                    string subcategory = string.Empty;
                    

                    var array = new[] { obj.SubBirthday, obj.SubCommercial, obj.SubCorporate, obj.SubFashion, obj.SubKid, obj.SubOther, obj.SubWedding, obj.SubWildlife };
                    subcategory = string.Join(",", array.Where(s => !string.IsNullOrEmpty(s)));
                    // subcategory = obj.SubBirthday + "," + obj.SubCommercial + "," + obj.SubCorporate + "," + obj.SubFashion + "," + obj.SubKid + "," + obj.SubOther + "," + obj.SubWedding + "," + obj.SubWildlife;
                    
                    #endregion

                    #region Insert  Photographer Details Tbl_PhotographerProfile
                    Log.Info("Start Insert  Photographer Details Tbl_PhotographerProfile ");
                    Log.Info("PhotographerID   " + newid);
                    Log.Info("RegistrationID " + RegistrationID);
                    Log.Info("@PhotographerID  " + newid);
                    Log.Info("@Name  " + obj.PhotographerName);
                    Log.Info("@PhoneNo  " + Convert.ToString(obj.Contact));
                    Log.Info("@PhoneNo2  " + Convert.ToString(obj.Contact2));
                    Log.Info("@PhotographerAddress  " + Convert.ToString(obj.Address));
                    Log.Info("@Email  " + Convert.ToString(obj.Email));
                    Log.Info("@About  " + Convert.ToString(obj.AboutMe));
                    Log.Info("@CurrentLocation  " + Convert.ToString(obj.CurrentLocation));//Convert.ToString(obj.CurrentLocation));
                    Log.Info("@AlsoShootIn  " + Convert.ToString(obj.PreferedLocation));//1
                    Log.Info("@PaymentOption  " + "");//Convert.ToString(obj.PaymentOption));

                    Log.Info("@Equipments  " + Convert.ToString(obj.Equipments));

                    Log.Info("@ServiceDescription  " + "");// Convert.ToString(obj.Services));
                    Log.Info("@StudioName  " + Convert.ToString(obj.StudioName));

                    Log.Info("@TeamSize  " + Convert.ToString(teamname));//Convert.ToString(obj.TeamSize));
                    Log.Info("@Website  " + Convert.ToString(obj.Website));
                    Log.Info("@Product  " + Convert.ToString(obj.Products));//Convert.ToString(obj.Products));
                    Log.Info("@ServiceOffered  " + servicename);// Convert.ToString(obj.Services));//1servicename
                    Log.Info("@ServiceProvided  " + serviceProvided);
                    Log.Info("@SubCategories  " + subcategory);
                    Log.Info("@LanguageKnown  " + Convert.ToString(obj.LanguageKnown));//Convert.ToString(obj.LanguageKnown));
                    Log.Info("@YearOfExperience  " + Convert.ToString(obj.YearOfExperience));
                    Log.Info("@Achievement  " + Convert.ToString(obj.Achievements));
                    Log.Info("@YoutubeLink  " + Convert.ToString(obj.YoutubeLink));
                    Log.Info("@FacebookLink  " + Convert.ToString(obj.FacebookLink));
                    Log.Info("@InstagramLink  " + Convert.ToString(obj.InstaLink));
                    Log.Info("@GoogleMap  " + Convert.ToString(obj.GoogleMap));//1
                    Log.Info("@RegistrationID  " + (RegistrationID));//1
                    Log.Info("@actionType  " + actiontype);

                    #region null handle

                    if (obj.OwnerPhoto == null)
                    {
                        obj.OwnerPhoto = "";
                    }
                    if (obj.StudioName == null)
                    {
                        obj.StudioName = "";
                    }
                    if (obj.Contact2 == null)
                    {
                        obj.Contact2 = "";
                    }

                    if (obj.CurrentLocation == null)
                    {
                        obj.CurrentLocation = "";
                    }
                    if (obj.PreferedLocation == null)
                    {
                        obj.PreferedLocation = "";
                    }
                    if (obj.PaymentOption == null)
                    {
                        obj.PaymentOption = new string[0];
                    }
                    if (obj.Equipments == null)
                    {
                        obj.Equipments = "";
                    }
                    if (obj.CoverImage == null)
                    {
                        obj.CoverImage = "";
                    }
                    if (obj.Logo == null)
                    {
                        obj.Logo = "";
                    }

                    if (obj.Services == null)
                    {

                        obj.Services = new List<PhotographyCategories> { };
                    }
                    if (obj.FacebookLink == null)
                    {
                        obj.FacebookLink = "";
                    }
                    if (obj.YoutubeLink == null)
                    {
                        obj.YoutubeLink = "";
                    }
                    if (obj.InstaLink == null)
                    {
                        obj.InstaLink = "";
                    }
                    if (obj.Website == null)
                    {
                        obj.Website = "";
                    }
                    if (obj.Achievements == null)
                    {
                        obj.Achievements = "";
                    }
                    if (obj.LanguageKnown == null)
                    {
                        obj.LanguageKnown = "";
                    }
                    if (obj.Products == null)
                    {
                        obj.Products = "";
                    }
                    if (obj.GoogleMap == null)
                    {
                        obj.GoogleMap = "";
                    }
                    #endregion null handle

                    try
                    {
                        SqlCommand objSqlCommand = new SqlCommand("Insert_Registration", objSqlConnection);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataTable dt = new DataTable();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Transaction = tran;
                        Log.Info("Start Passing Parameters To Insert_Registration");
                        objSqlCommand.Parameters.AddWithValue("@PhotographerID", newid);
                        objSqlCommand.Parameters.AddWithValue("@Name", obj.PhotographerName);
                        objSqlCommand.Parameters.AddWithValue("@PhoneNo", Convert.ToString(obj.Contact));
                        objSqlCommand.Parameters.AddWithValue("@PhoneNo2", Convert.ToString(obj.Contact2));
                        objSqlCommand.Parameters.AddWithValue("@PhotographerAddress", Convert.ToString(obj.Address));
                        objSqlCommand.Parameters.AddWithValue("@Email", Convert.ToString(obj.Email));
                        objSqlCommand.Parameters.AddWithValue("@About", Convert.ToString(obj.AboutMe));
                        objSqlCommand.Parameters.AddWithValue("@CurrentState", Convert.ToString(obj.CurrentState));
                        objSqlCommand.Parameters.AddWithValue("@CurrentLocation", Convert.ToString(obj.CurrentLocation));//Convert.ToString(obj.CurrentLocation));
                        objSqlCommand.Parameters.AddWithValue("@AlsoShootIn", Convert.ToString(obj.PreferedLocation));//1
                        objSqlCommand.Parameters.AddWithValue("@PaymentOption", "");//Convert.ToString(obj.PaymentOption));
                        objSqlCommand.Parameters.AddWithValue("@Serviceprovide", serviceProvided);
                        //objSqlCommand.Parameters.AddWithValue("@ModifiedOn", string.Format("{0:D}", DateTime.Now));
                        objSqlCommand.Parameters.AddWithValue("@Equipments", Convert.ToString(obj.Equipments));

                        objSqlCommand.Parameters.AddWithValue("@ServiceDescription", "");// Convert.ToString(obj.Services));
                        objSqlCommand.Parameters.AddWithValue("@StudioName", Convert.ToString(obj.StudioName));

                        objSqlCommand.Parameters.AddWithValue("@TeamSize", Convert.ToString(teamname));//Convert.ToString(obj.TeamSize));
                        objSqlCommand.Parameters.AddWithValue("@Website", Convert.ToString(obj.Website));
                        objSqlCommand.Parameters.AddWithValue("@Product", Convert.ToString(obj.Products));//Convert.ToString(obj.Products));
                        objSqlCommand.Parameters.AddWithValue("@ServiceOffered", servicename);// Convert.ToString(obj.Services));//1
                        objSqlCommand.Parameters.AddWithValue("@SubCategories", subcategory); 
                        objSqlCommand.Parameters.AddWithValue("@LanguageKnown", Convert.ToString(obj.LanguageKnown));//Convert.ToString(obj.LanguageKnown));
                        objSqlCommand.Parameters.AddWithValue("@YearOfExperience", Convert.ToString(obj.YearOfExperience));
                        objSqlCommand.Parameters.AddWithValue("@Achievement", Convert.ToString(obj.Achievements));
                        objSqlCommand.Parameters.AddWithValue("@YoutubeLink", Convert.ToString(obj.YoutubeLink));
                        objSqlCommand.Parameters.AddWithValue("@FacebookLink", Convert.ToString(obj.FacebookLink));
                        objSqlCommand.Parameters.AddWithValue("@InstagramLink", Convert.ToString(obj.InstaLink));
                        objSqlCommand.Parameters.AddWithValue("@GoogleMap", Convert.ToString(obj.GoogleMap));//1
                        objSqlCommand.Parameters.AddWithValue("@RegistrationID", (RegistrationID));//1
                        
                        objSqlCommand.Parameters.AddWithValue("@actionType", actiontype);
                        SqlParameter @P_PhotographerID = new SqlParameter("@P_PhotographerID", SqlDbType.BigInt, 10);
                        @P_PhotographerID.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_PhotographerID);


                        SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessFlag);

                        SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                        @P_SuccessMessage.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessMessage);
                        Log.Info("Before Saving Photographer Details In Tbl_PhotographerProfile ");
                        objSqlCommand.ExecuteNonQuery();
                        // adapt.Fill(dt); 
                        SuccessFlag1 = Convert.ToString(objSqlCommand.Parameters["@P_SUCCESSFLAG"].Value);
                        newPhotograpgerID = Convert.ToInt32(objSqlCommand.Parameters["@P_PhotographerID"].Value);
                        Log.Info("End Insert  Photographer Details Tbl_PhotographerProfile ");
                    } catch (Exception ex)
                    {
                        string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    }
                    #endregion

                    #region Insert CoverImage in Photographer Profile
                   
                    HttpPostedFileBase coverfile = Request.Files["coverfile"];
                    Log.Info("Start: Insert CoverImage in Photographer Profile ");
                    if (coverfile != null)
                    {
                        string filename = Path.GetFileName(coverfile.FileName);
                        string contentType = coverfile.ContentType;
                        string extension = Path.GetExtension(coverfile.FileName);

                        var path = Server.MapPath("~/RegistrationImages/Coverimage/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/");
                        string srcpathcover = Server.MapPath("~/RegistrationImages/Temp/Coverimage/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/");
                        if (!Directory.Exists(srcpathcover))
                        {
                            //If Directory (Folder) does not exists Create it.
                            Directory.CreateDirectory(srcpathcover);
                        }
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        else
                        {
                            DirectoryInfo di = new DirectoryInfo(path);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }

                        }

                        FinalFileName = filename;
                        string FileDataImages = "\\RegistrationImages\\Coverimage\\" + u.Encode(Convert.ToString(newPhotograpgerID)) + "\\" + FinalFileName;
                        coverfile.SaveAs(srcpathcover + "\\" + FinalFileName);
                        

                        string target = (path + FinalFileName);
                        string source = (srcpathcover + FinalFileName);
                        Utility.Utility.CompressImage_Imageportfolio(source, target, 500, 1350);

                        //string root = AppDomain.CurrentDomain.BaseDirectory; //Will Dynamically Take The Root Path
                        //if (System.IO.File.Exists(Path.Combine(root, path)))
                        if (System.IO.File.Exists(srcpathcover + "\\" + FinalFileName))
                        {
                            // If file found, delete it    
                            System.IO.File.Delete(srcpathcover + "\\" + FinalFileName); // Path.Combine(root, path)


                        }


                        //coverfile.SaveAs(Path.Combine(path, filename));
                        obj.CoverImage = "\\RegistrationImages\\Coverimage\\" + u.Encode(Convert.ToString(newPhotograpgerID)) + "\\" + filename;

                        #region Cover Image in Database
                        byte[] FileDetcoverfile;
                        FileDetcoverfile = System.IO.File.ReadAllBytes(System.Web.HttpContext.Current.Server.MapPath("~/RegistrationImages/Coverimage/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/"+ FinalFileName));
                        //Stream fscoverfile = coverfile.InputStream;
                        //BinaryReader Br = new BinaryReader(fscoverfile);
                        //FileDetcoverfile = Br.ReadBytes((Int32)fscoverfile.Length);

                        Log.Info("Start: Passing Paramters To UpdateCoverImage ");

                        SqlCommand objSqlCommand1 = new SqlCommand("UpdateCoverImage", objSqlConnection);
                        SqlDataAdapter adapt1 = new SqlDataAdapter(objSqlCommand1);
                        DataTable dt1 = new DataTable();
                        objSqlCommand1.CommandType = CommandType.StoredProcedure;
                        objSqlCommand1.Transaction = tran;
                        objSqlCommand1.Parameters.AddWithValue("@PhotographerID", newPhotograpgerID);
                        objSqlCommand1.Parameters.Add("@CoverImage", SqlDbType.NVarChar, -1).Value = obj.CoverImage;
                        //objSqlCommand1.Parameters.AddWithValue("@CoverImage", obj.CoverImage);
                        objSqlCommand1.Parameters.Add("@CoverByte", SqlDbType.VarBinary, -1).Value = FileDetcoverfile;


                        SqlParameter @P_SuccessFlag1 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessFlag1);

                        SqlParameter @P_SuccessMessage1 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessMessage1);
                        //adapt1.Fill(dt1);
                        Log.Info("Start: Before Saving Paramters To UpdateCoverImage ");
                        objSqlCommand1.ExecuteNonQuery();
                        SuccessFlag2 = Convert.ToString(objSqlCommand1.Parameters["@P_SUCCESSFLAG"].Value);
                        Log.Info("SuccessFlag2 : " + SuccessFlag2);
                        Log.Info("End:Insert CoverImage in Photographer Profile ");
                        #endregion
                    }
                    #endregion

                    #region Insert Logo Image in Photographer Profile
                    Log.Info("Start:Insert Logo Image in Photographer Profile ");
                    HttpPostedFileBase logofile = Request.Files["logofile"];
                    if (logofile != null)
                    {
                        string filename = Path.GetFileName(logofile.FileName);
                        string contentType = logofile.ContentType;
                        string extension = Path.GetExtension(logofile.FileName);
                        var path = Server.MapPath("~/RegistrationImages/Logo/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/");
                        string srcpathlogo = Server.MapPath("~/RegistrationImages/Temp/Logo/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/");
                        if (!Directory.Exists(srcpathlogo))
                        {
                            //If Directory (Folder) does not exists Create it.
                            Directory.CreateDirectory(srcpathlogo);
                        }
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        else
                        {
                            DirectoryInfo di = new DirectoryInfo(path);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }

                        }
                        FinalFileName = filename;
                        string FileDataImages = "\\RegistrationImages\\Logo\\" + u.Encode(Convert.ToString(newPhotograpgerID)) + "\\" + FinalFileName;
                        logofile.SaveAs(srcpathlogo + "\\" + FinalFileName);


                        string target = (path + FinalFileName);
                        string source = (srcpathlogo + FinalFileName);
                        Utility.Utility.CompressImage_Imageportfolio(source, target, 300, 300);

                        //string root = AppDomain.CurrentDomain.BaseDirectory; //Will Dynamically Take The Root Path
                        //if (System.IO.File.Exists(Path.Combine(root, path)))
                        if (System.IO.File.Exists(srcpathlogo + "\\" + FinalFileName))
                        {
                            // If file found, delete it    
                            System.IO.File.Delete(srcpathlogo + "\\" + FinalFileName); // Path.Combine(root, path)


                        }

                        //logofile.SaveAs(Path.Combine(path, filename));
                        obj.Logo = "\\RegistrationImages\\Logo\\" + u.Encode(Convert.ToString(newPhotograpgerID)) + "\\" + filename;
                        //byte[] FileDetlogofile;
                        //Stream fslogofile = logofile.InputStream;
                        //BinaryReader Br = new BinaryReader(fslogofile);
                        //FileDetlogofile = Br.ReadBytes((Int32)fslogofile.Length);
                        //obj.Logo = FileDetlogofile;
                        Log.Info("Start:Passing Paramters To UpdateLogo ");
                        SqlCommand objSqlCommand1 = new SqlCommand("UpdateLogo", objSqlConnection);
                        SqlDataAdapter adapt1 = new SqlDataAdapter(objSqlCommand1);
                        DataTable dt1 = new DataTable();
                        objSqlCommand1.CommandType = CommandType.StoredProcedure;
                        objSqlCommand1.Transaction = tran;
                        objSqlCommand1.Parameters.AddWithValue("@PhotographerID", newPhotograpgerID);
                        objSqlCommand1.Parameters.Add("@Logo", SqlDbType.NVarChar, -1).Value = obj.Logo;
                        //objSqlCommand1.Parameters.AddWithValue("@Logo", obj.Logo);


                        SqlParameter @P_SuccessFlag1 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessFlag1);

                        SqlParameter @P_SuccessMessage1 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessMessage1);
                        //adapt1.Fill(dt1);
                        Log.Info("Before:Saving Paramters To UpdateLogo ");
                        objSqlCommand1.ExecuteNonQuery();
                        SuccessFlag3 = Convert.ToString(objSqlCommand1.Parameters["@P_SUCCESSFLAG"].Value);
                        Log.Info("SuccessFlag3: " + SuccessFlag3);
                        Log.Info("End:Insert Logo Image in Photographer Profile ");
                    }
                    #endregion

                    #region Insert Owner Photo in Photographer Profile
                    Log.Info("Start:Insert Owner Photo in Photographer Profile ");
                    HttpPostedFileBase ownerphotofile = Request.Files["ownerphotofile"];
                    if (ownerphotofile != null)
                    {
                        string filenameownerphotofile = Path.GetFileName(ownerphotofile.FileName);
                        string contentTypeownerphotofile = ownerphotofile.ContentType;
                        string extension = Path.GetExtension(ownerphotofile.FileName);
                        var path = Server.MapPath("~/RegistrationImages/OwnerPhoto/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/");
                        string srcpathowner = Server.MapPath("~/RegistrationImages/Temp/OwnerPhoto/" + u.Encode(Convert.ToString(newPhotograpgerID)) + "/");
                        if (!Directory.Exists(srcpathowner))
                        {
                            //If Directory (Folder) does not exists Create it.
                            Directory.CreateDirectory(srcpathowner);
                        }
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        else
                        {
                            DirectoryInfo di = new DirectoryInfo(path);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }

                        }

                        FinalFileName = filenameownerphotofile;
                        string FileDataImages = "\\RegistrationImages\\OwnerPhoto\\" + u.Encode(Convert.ToString(newPhotograpgerID)) + "\\" + FinalFileName;
                        ownerphotofile.SaveAs(srcpathowner + "\\" + FinalFileName);


                        string target = (path + FinalFileName);
                        string source = (srcpathowner + FinalFileName);
                        Utility.Utility.CompressImage_Imageportfolio(source, target, 300, 300);

                        //string root = AppDomain.CurrentDomain.BaseDirectory; //Will Dynamically Take The Root Path
                        //if (System.IO.File.Exists(Path.Combine(root, path)))
                        if (System.IO.File.Exists(srcpathowner + "\\" + FinalFileName))
                        {
                            // If file found, delete it    
                            System.IO.File.Delete(srcpathowner + "\\" + FinalFileName); // Path.Combine(root, path)


                        }
                        //ownerphotofile.SaveAs(Path.Combine(path, filenameownerphotofile));
                        obj.OwnerPhoto =  "\\RegistrationImages\\OwnerPhoto\\" + u.Encode(Convert.ToString(newPhotograpgerID)) + "\\" + filenameownerphotofile;
                        //byte[] FileDetownerphotofile;
                        //Stream fsownerphotofile = ownerphotofile.InputStream;
                        //BinaryReader Brownerphotofile = new BinaryReader(fsownerphotofile);
                        //FileDetownerphotofile = Brownerphotofile.ReadBytes((Int32)fsownerphotofile.Length);
                        //obj.OwnerPhoto = FileDetownerphotofile;
                        Log.Info("Start:Passing Parameters To UpdateOwnerPhotograph  ");
                        SqlCommand objSqlCommand1 = new SqlCommand("UpdateOwnerPhotograph", objSqlConnection);
                        SqlDataAdapter adapt1 = new SqlDataAdapter(objSqlCommand1);
                        DataTable dt1 = new DataTable();
                        objSqlCommand1.CommandType = CommandType.StoredProcedure;
                        objSqlCommand1.Transaction = tran;
                        objSqlCommand1.Parameters.AddWithValue("@PhotographerID", newPhotograpgerID);
                        //objSqlCommand1.Parameters.AddWithValue("@OwnerPhotograph", obj.OwnerPhoto);
                        objSqlCommand1.Parameters.Add("@OwnerPhotograph", SqlDbType.NVarChar, -1).Value = obj.OwnerPhoto;

                        SqlParameter @P_SuccessFlag1 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessFlag1);

                        SqlParameter @P_SuccessMessage1 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessMessage1);
                        Log.Info("Before:Saving Parameters To UpdateOwnerPhotograph  ");
                        objSqlCommand1.ExecuteNonQuery();
                        //adapt1.Fill(dt1);
                        SuccessFlag4 = Convert.ToString(objSqlCommand1.Parameters["@P_SUCCESSFLAG"].Value);
                        Log.Info("SuccessFlag4:  " + SuccessFlag4);
                        Log.Info("End:Insert Owner Photo in Photographer Profile ");
                    }
                    #endregion



                    #region Update Login details of complete profile
                    Log.Info("Start:Update Login details of complete profile ");
                    SqlCommand objSqlCommand9 = new SqlCommand("UpdateCompleteProfile", objSqlConnection);
                    SqlDataAdapter adapt9 = new SqlDataAdapter(objSqlCommand9);
                    DataTable dt9 = new DataTable();
                    objSqlCommand9.CommandType = CommandType.StoredProcedure;
                    objSqlCommand9.Transaction = tran;
                    objSqlCommand9.Parameters.AddWithValue("@RegistrationID", RegistrationID);


                    SqlParameter @P_SuccessFlag9 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag9.Direction = ParameterDirection.Output;
                    objSqlCommand9.Parameters.Add(@P_SuccessFlag9);

                    SqlParameter @P_SuccessMessage9 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage9.Direction = ParameterDirection.Output;
                    objSqlCommand9.Parameters.Add(@P_SuccessMessage9);
                    Log.Info("Before:Saving Parameters To UpdateCompleteProfile ");
                    objSqlCommand9.ExecuteNonQuery();
                    //adapt1.Fill(dt1);
                    SuccessFlag9 = Convert.ToString(objSqlCommand9.Parameters["@P_SUCCESSFLAG"].Value);
                    Log.Info("End:Update Login details of complete profile ");
                    Log.Info("SuccessFlag1" + SuccessFlag1);
                    Log.Info("SuccessFlag1" + SuccessFlag1);
                    Log.Info("SuccessFlag1" + SuccessFlag1);
                    Log.Info("SuccessFlag1" + SuccessFlag1);
                    #endregion

                    if (SuccessFlag1 == "Y" && SuccessFlag9 == "Y")
                    {
                        if (coverfile != null)
                        {
                            if (SuccessFlag2 == "F")
                            {
                                Log.Info("SuccessFlag2" + SuccessFlag2);

                                if (tran != null)
                                    tran.Rollback();
                                return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
                            }
                        }
                        if (logofile != null)
                        {
                            if (SuccessFlag3 == "F")
                            {
                                Log.Info("SuccessFlag3" + SuccessFlag3);
                                if (tran != null)
                                    tran.Rollback();
                                return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
                            }
                        }
                        if (ownerphotofile != null)
                        {
                            if (SuccessFlag4 == "F")
                            {
                                Log.Info("SuccessFlag4" + SuccessFlag4);
                                if (tran != null)
                                    tran.Rollback();
                                return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
                            }
                        }
                        tran.Commit();
                        return Json(new { result = u.Encode(Convert.ToString(newPhotograpgerID)), url = Url.Action("Index", "Login") });
                    }
                    else
                    {
                        Log.Info("SuccessFlag1" + SuccessFlag2 + "-----" + "SuccessFlag9" + SuccessFlag9);
                        if (tran != null)
                            tran.Rollback();
                        return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
                    }

                }



            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                //Utility.Utility.LogError(tran.);
                if (tran == null)
                    tran.Rollback();
                string Message = ex.Message;
                return Json(new { result = "Error2" });
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

        [Authorize(Roles = "Admin,VerifiedPhotographer,NonVerifiedPhotographer,SuperAdmin")]
        [HttpPost]
        public ActionResult Finish(string PhotographerID, string VideoIframe)
        {
            Utility.Utility u = new Utility.Utility();
            string ConnectionString = string.Empty;
            int RegistrationID = 0;
            ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            if (!string.IsNullOrWhiteSpace(PhotographerID) && !string.IsNullOrEmpty(PhotographerID))
            {
                int photographerid = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
                if (photographerid != 0)
                {
                    #region Check To see Database Has Values
                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("select Name,PhoneNo,PhotographerAddress,Email,About,CurrentLocation,AlsoShootIn,CoverImage,TeamSize,ServiceOffered,YearOfExperience,CoverByte,RegistrationID from tbl_photographerprofile where PhotographerID='" + photographerid + "'", con))
                        {

                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {

                                while (rd.Read())
                                {
                                    RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                    if (string.IsNullOrEmpty(Convert.ToString(rd["Name"])) || string.IsNullOrEmpty(Convert.ToString(rd["PhoneNo"]))
                                        || string.IsNullOrEmpty(Convert.ToString(rd["PhotographerAddress"])) || string.IsNullOrEmpty(Convert.ToString(rd["Email"]))
                                        || string.IsNullOrEmpty(Convert.ToString(rd["About"])) || string.IsNullOrEmpty(Convert.ToString(rd["CurrentLocation"]))
                                        || string.IsNullOrEmpty(Convert.ToString(rd["AlsoShootIn"])) || string.IsNullOrEmpty(Convert.ToString(rd["CoverImage"]))
                                        || string.IsNullOrEmpty(Convert.ToString(rd["TeamSize"]))
                                        || string.IsNullOrEmpty(Convert.ToString(rd["ServiceOffered"])) || string.IsNullOrEmpty(Convert.ToString(rd["YearOfExperience"])) || (rd["CoverByte"]) == null)
                                    {
                                        return Json(new { result = "Error1" });
                                    }
                                }
                            }
                        }
                        using (SqlCommand cmd = new SqlCommand("select ImagePath from Tbl_ImagePortFolio where PhotographerID='" + photographerid + "'", con))
                        {

                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {

                                if (rd.HasRows)
                                {

                                }
                                else
                                {
                                    return Json(new { result = "Error2" });
                                }
                            }
                        }
                        try { 
                        var VideoIframes = JsonConvert.DeserializeObject<List<Tbl_VideoPortFolio>>(VideoIframe);

                        foreach (var item in VideoIframes)
                        {
                                if (!string.IsNullOrEmpty(item.VideoBytes))
                                {


                                    string iframevideo = HttpUtility.HtmlDecode(item.VideoBytes);
                                    HtmlDocument document = new HtmlDocument();
                                    document.LoadHtml(iframevideo);

                                    var image = document.DocumentNode.FirstChild;
                                    
                                        if (image.Attributes["width"] != null)
                                        {
                                            image.Attributes["width"].Remove();
                                        }
                                        if (image.Attributes["height"] != null)
                                        {
                                            image.Attributes["height"].Remove();
                                        }
                                    
                                    item.VideoBytes = image.OuterHtml;
                                    #region Video Portfolio

                                    SqlCommand cmd2 = new SqlCommand("Insert_VideoPortfolios", con);
                                    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                    DataTable dtable2 = new DataTable();
                                    cmd2.CommandType = CommandType.StoredProcedure;

                                    cmd2.Parameters.Add("@VideoPath", SqlDbType.NVarChar, -1).Value = item.VideoBytes;

                                    cmd2.Parameters.AddWithValue("@PhotographerID", Utility.Utility.Decode(PhotographerID));


                                    SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                                    @P_SuccessFlag2.Direction = ParameterDirection.Output;
                                    cmd2.Parameters.Add(@P_SuccessFlag2);

                                    SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                                    @P_SuccessMessage2.Direction = ParameterDirection.Output;
                                    cmd2.Parameters.Add(@P_SuccessMessage2);
                                    da2.Fill(dtable2);

                                    #endregion
                                }


                        }

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    #endregion
                    #endregion
                    try
                    {

                        #region Passing Parameters To Stored Procedure
                        string SuccessFlag9 = "";
                        using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                        {

                            objSqlConnection.Open();

                            #region Update Login details of complete profile
                            Log.Info("Start:Update Login details of complete profile ");
                            SqlCommand objSqlCommand9 = new SqlCommand("UpdateCompleteProfile", objSqlConnection);
                            SqlDataAdapter adapt9 = new SqlDataAdapter(objSqlCommand9);
                            DataTable dt9 = new DataTable();
                            objSqlCommand9.CommandType = CommandType.StoredProcedure;
                            
                            objSqlCommand9.Parameters.AddWithValue("@RegistrationID", RegistrationID);
                            


                            SqlParameter @P_SuccessFlag9 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                            @P_SuccessFlag9.Direction = ParameterDirection.Output;
                            objSqlCommand9.Parameters.Add(@P_SuccessFlag9);

                            SqlParameter @P_SuccessMessage9 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                            @P_SuccessMessage9.Direction = ParameterDirection.Output;
                            objSqlCommand9.Parameters.Add(@P_SuccessMessage9);
                            Log.Info("Before:Saving Parameters To UpdateCompleteProfile ");
                            objSqlCommand9.ExecuteNonQuery();
                            //adapt1.Fill(dt1);
                            SuccessFlag9 = Convert.ToString(objSqlCommand9.Parameters["@P_SUCCESSFLAG"].Value);
                            Log.Info("End:Update Login details of complete profile ");
                          
                            #endregion



                            #region Confirming Portfolio is Inserted
                            SqlCommand objSqlCommand = new SqlCommand("UpdateRegistrationAfterPortFolio", objSqlConnection);
                            SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                            DataTable dt = new DataTable();
                            objSqlCommand.CommandType = CommandType.StoredProcedure;
                            objSqlCommand.Parameters.AddWithValue("@PhotographerID", photographerid);

                            SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                            @P_SuccessFlag.Direction = ParameterDirection.Output;
                            objSqlCommand.Parameters.Add(@P_SuccessFlag);

                            SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                            @P_SuccessMessage.Direction = ParameterDirection.Output;
                            objSqlCommand.Parameters.Add(@P_SuccessMessage);
                            adapt.Fill(dt);
                            string SuccessFlag = Convert.ToString(objSqlCommand.Parameters["@P_SUCCESSFLAG"].Value);
                            #endregion

                            if (SuccessFlag == "Y")
                            {
                                if (CheckIfVerified(photographerid) == true)
                                {
                                    return Json(new { result = "Success", url = Url.Action("PhotographerProfile", "PhotographerProfile", new { PhotographerID = u.Encode(Convert.ToString(photographerid)) }) });
                                }
                                else
                                {
                                    return Json(new { result = "NotVerified" });
                                }
                            }
                            else
                            {
                                return Json(new { result = "FailedForm" });
                            }




                        }

                        #endregion

                       

                    }
                    catch (Exception ex)
                    {
                        string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                        string Message = ex.Message;
                        return Json(new { result = "Error" });
                    }
                }
                else
                {
                    return Json(new { result = "Error1" });
                }
            }

            return Json(new { result = "Error" });

        }




        #region Get All Images on Edit Based on PhotographerID
        [HttpPost]
        public ActionResult GetImage(string PhotographerID)
        {
            string[] fileEntries = null;
            List<string> Entries = new List<string>();
            if (!string.IsNullOrEmpty(PhotographerID))
            {
                try
                {

                    string directoryPath = @"\RegistrationImages\ImagePortfolios\" + PhotographerID + "\\";
                    fileEntries = Directory.GetFiles(Server.MapPath(directoryPath));

                    foreach (string fileName in fileEntries)
                    {

                        Entries.Add(directoryPath + System.IO.Path.GetFileName(fileName));
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Exception = ex.Message;
                }
            }

            return Json(Entries);
        }

        [HttpPost]
        public ActionResult GetVideo(string PhotographerID)
        {
            //string[] fileEntries = null;
            List<string> Entries = new List<string>();
            List<string> VideoName = new List<string>();
            string ConnectionString = string.Empty;
            string DecodedPhotographerID = Utility.Utility.Decode(PhotographerID);
            ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            if (!string.IsNullOrWhiteSpace(PhotographerID) && !string.IsNullOrEmpty(PhotographerID))
            {
            //    if (!string.IsNullOrEmpty(PhotographerID))
            //{
                try
                {
                    Log.Info("Getting Videos With PhotographerID: " + PhotographerID);
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        con.Open();

                        using (SqlCommand cmd = new SqlCommand("SELECT VideoPath FROM Tbl_VideoPortFolio where PhotographerID='" + DecodedPhotographerID + "'", con))
                        {

                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    string iframestring = Convert.ToString(rd["VideoPath"]);
                                     iframestring = iframestring.Replace(" width=\"560\" height=\"315\"", " width=\"150\" height=\"100\"");
                                    
                                    Entries.Add(iframestring);
                                }
                                //if (rd.HasRows)
                                //{
                                    
                                //}
                                //else
                                //{
                                //    return Json(new { result = "Error2" });
                                //}
                            }
                        }
                        
                    }
                    //RegistrationImages / ImagePortfolios / "+ PhotographerID + " / "
                    //string directoryPath = @"\RegistrationImages\VideoPortfolios\" + PhotographerID + "\\";
                    //fileEntries = Directory.GetFiles(Server.MapPath(directoryPath));

                    //    foreach (string fileName in fileEntries)
                    //    {
                    //        VideoName.Add(System.IO.Path.GetFileName(fileName));
                    //        Entries.Add(directoryPath + System.IO.Path.GetFileName(fileName));
                    //    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Exception = ex.Message;
                }

            }
            return Json(new { result = Entries, fileName = VideoName });
        }

        [HttpPost]
        public ActionResult GetLogo(string PhotographerID)
        {
            string[] fileEntries = null;
            List<string> Entries = new List<string>();
            if (!string.IsNullOrEmpty(PhotographerID))
            {
                try
                {
                    //RegistrationImages / ImagePortfolios / "+ PhotographerID + " / "
                    string directoryPath = @"\RegistrationImages\Logo\" + PhotographerID + "\\";
                    fileEntries = Directory.GetFiles(Server.MapPath(directoryPath));


                    foreach (string fileName in fileEntries)
                    {
                        Entries.Add(directoryPath + System.IO.Path.GetFileName(fileName));
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Exception = ex.Message;
                }
            }

            return Json(Entries);
        }
        [HttpPost]
        public ActionResult GetOwner(string PhotographerID)
        {
            string[] fileEntries = null;
            List<string> Entries = new List<string>();
            if (!string.IsNullOrEmpty(PhotographerID))
            {
                try
                {
                    //RegistrationImages / ImagePortfolios / "+ PhotographerID + " / "
                    string directoryPath = @"\RegistrationImages\OwnerPhoto\" + PhotographerID + "\\";
                    fileEntries = Directory.GetFiles(Server.MapPath(directoryPath));

                    foreach (string fileName in fileEntries)
                    {
                        Entries.Add(directoryPath + System.IO.Path.GetFileName(fileName));
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Exception = ex.Message;
                }

            }
            return Json(Entries);
        }
        [HttpPost]
        public ActionResult GetCover(string PhotographerID)
        {
            string[] fileEntries = null;
            List<string> Entries = new List<string>();
            if (!string.IsNullOrEmpty(PhotographerID))
            {

                //int photographerid = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
                try
                {

                    string directoryPath = @"\RegistrationImages\Coverimage\" + PhotographerID + "\\";
                    fileEntries = Directory.GetFiles(Server.MapPath(directoryPath));

                    foreach (string fileName in fileEntries)
                    {
                        Entries.Add(directoryPath + System.IO.Path.GetFileName(fileName));
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    string Exception = ex.Message;
                }

            }


            return Json(Entries);
        }
        #endregion

        #region Upload Or Delete Image or Video Portfolio
        [HttpPost]
        public ActionResult UploadImage(string PhotographerID)
        {
            HttpFileCollectionBase files = Request.Files;
            List<string> pathlist = new List<string>();
            if (!string.IsNullOrEmpty(PhotographerID))
            {
                int photographerid = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
                if (photographerid != 0)
                {
                    try
                    {
                        string path = Server.MapPath("~/RegistrationImages/ImagePortfolios/" + PhotographerID + "/");
                        string srcpath1 = Server.MapPath("~/RegistrationImages/Temp/ImagePortfolios/" + PhotographerID + "/");
                        if (!Directory.Exists(path))
                        {
                            //If Directory (Folder) does not exists Create it.
                            Directory.CreateDirectory(path);
                        }
                        if (!Directory.Exists(srcpath1))
                        {
                            //If Directory (Folder) does not exists Create it.
                            Directory.CreateDirectory(srcpath1);
                        }
                        string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                        using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                        {

                            objSqlConnection.Open();
                            for (int i = 0; i < files.Count; i++)
                            {
                                HttpPostedFileBase file = files[i];

                                //Itech compress kar

                                FinalFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(file.FileName);
                                string FileDataImages = "\\RegistrationImages\\ImagePortfolios\\" + PhotographerID + "\\" + FinalFileName;
                                file.SaveAs(srcpath1 + "\\" + FinalFileName);
                                pathlist.Add(FileDataImages);

                                string target = (path + FinalFileName);
                                string source = (srcpath1  + FinalFileName);
                                Utility.Utility.CompressImage_Imageportfolio(source, target, 300, 300);

                                //string root = AppDomain.CurrentDomain.BaseDirectory; //Will Dynamically Take The Root Path
                                //if (System.IO.File.Exists(Path.Combine(root, path)))
                                if (System.IO.File.Exists(srcpath1 + "\\" + FinalFileName))
                                {
                                    // If file found, delete it    
                                    System.IO.File.Delete(srcpath1 + "\\" + FinalFileName); // Path.Combine(root, path)


                                }
                                


                                #region Image Portfolio

                                SqlCommand cmd2 = new SqlCommand("Insert_ImagePortfolios", objSqlConnection);
                                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                DataTable dtable2 = new DataTable();
                                cmd2.CommandType = CommandType.StoredProcedure;

                                cmd2.Parameters.Add("@ImagePath", SqlDbType.NVarChar, -1).Value = FileDataImages;
                                //cmd2.Parameters.AddWithValue("@ImagePath", FileDataImages);
                                cmd2.Parameters.AddWithValue("@PhotographerID", Utility.Utility.Decode(PhotographerID));


                                SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                                @P_SuccessFlag2.Direction = ParameterDirection.Output;
                                cmd2.Parameters.Add(@P_SuccessFlag2);

                                SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                                @P_SuccessMessage2.Direction = ParameterDirection.Output;
                                cmd2.Parameters.Add(@P_SuccessMessage2);
                                da2.Fill(dtable2);

                                #endregion

                            }
                            if (Directory.Exists(srcpath1))
                            {
                                Directory.Delete(srcpath1);
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                        string Exception = ex.Message;
                    }
                }
                else
                {
                    return Json(new { result = "Error" });

                }
            }

            return Json(pathlist);
        }
        [HttpPost]
        public ActionResult UploadVideo(string PhotographerID,string VideoIframe)
        {

            HttpFileCollectionBase files = Request.Files;
            List<string> pathlist = new List<string>();
            if (!string.IsNullOrEmpty(PhotographerID))
            {
                int photographerid = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
                if (photographerid != 0)
                {
                    try
                    {
                        string path = Server.MapPath("~/RegistrationImages/VideoPortfolios/" + PhotographerID + "/");
                        if (!Directory.Exists(path))
                        {
                            //If Directory (Folder) does not exists Create it.
                            Directory.CreateDirectory(path);
                        }
                        string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                        using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                        {

                            objSqlConnection.Open();

                            var VideoIframes= JsonConvert.DeserializeObject<List<Tbl_VideoPortFolio>>(VideoIframe);

                            foreach (var item in VideoIframes)
                            {
                                
                                   
                                    #region Video Portfolio

                                    SqlCommand cmd2 = new SqlCommand("Insert_VideoPortfolios", objSqlConnection);
                                    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                    DataTable dtable2 = new DataTable();
                                    cmd2.CommandType = CommandType.StoredProcedure;

                                    cmd2.Parameters.Add("@VideoPath", SqlDbType.NVarChar, -1).Value = item.VideoBytes;

                                    cmd2.Parameters.AddWithValue("@PhotographerID", Utility.Utility.Decode(PhotographerID));


                                    SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                                    @P_SuccessFlag2.Direction = ParameterDirection.Output;
                                    cmd2.Parameters.Add(@P_SuccessFlag2);

                                    SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                                    @P_SuccessMessage2.Direction = ParameterDirection.Output;
                                    cmd2.Parameters.Add(@P_SuccessMessage2);
                                    da2.Fill(dtable2);

                                    #endregion
                                

                            }

                            #region For Uploading Video Bytes Into Database (Previous)
                            //for (int i = 0; i < files.Count; i++)
                            //{
                            //    HttpPostedFileBase file = files[i];
                            //    FinalFileName = FinalFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(file.FileName);
                            //    file.SaveAs(path + "\\" + FinalFileName);
                            //    pathlist.Add(@"\RegistrationImages\VideoPortfolios\" + PhotographerID + "\\" + FinalFileName);
                            //    string FileDataVideos = "\\RegistrationImages\\VideoPortfolios\\" + PhotographerID + "\\" + FinalFileName;
                            //    #region Video Portfolio

                            //    SqlCommand cmd2 = new SqlCommand("Insert_VideoPortfolios", objSqlConnection);
                            //    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                            //    DataTable dtable2 = new DataTable();
                            //    cmd2.CommandType = CommandType.StoredProcedure;

                            //    cmd2.Parameters.Add("@VideoPath", SqlDbType.NVarChar, -1).Value = FileDataVideos;
                            //    //cmd2.Parameters.AddWithValue("@VideoPath", FileDataVideos);
                            //    cmd2.Parameters.AddWithValue("@PhotographerID", Utility.Utility.Decode(PhotographerID));


                            //    SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                            //    @P_SuccessFlag2.Direction = ParameterDirection.Output;
                            //    cmd2.Parameters.Add(@P_SuccessFlag2);

                            //    SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                            //    @P_SuccessMessage2.Direction = ParameterDirection.Output;
                            //    cmd2.Parameters.Add(@P_SuccessMessage2);
                            //    da2.Fill(dtable2);

                            //    #endregion

                            //}

                            #endregion For Uploading Video Bytes Into Database
                        }

                    }
                    catch (Exception ex)
                    {
                        string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    }
                }
                else
                {
                    return Json(new { result = "Error" });

                }
            }
            return Json(new { result = pathlist, fileName = FinalFileName });
            //return Json(pathlist);
        }
        [HttpPost]
        public ActionResult DeleteVideo(string path, string PhotographerID)
        {
            int result = 0;
            HttpFileCollectionBase files = Request.Files;
            if (!string.IsNullOrEmpty(PhotographerID) && !string.IsNullOrEmpty(path))
            {
                try
                {
                    path = HttpUtility.HtmlDecode(path);

                    //#region Physical path
                    ////string path1 = Server.MapPath("");
                    //string root = AppDomain.CurrentDomain.BaseDirectory; //Will Dynamically Take The Root Path
                    //                                                     //if (System.IO.File.Exists(Path.Combine(root, path)))
                    //if (System.IO.File.Exists(root + path))
                    //{
                    //    // If file found, delete it    
                    //    System.IO.File.Delete(root + path); // Path.Combine(root, path)
                    //    result = 1;

                    //}
                    //#endregion

                    #region Database path
                    string iframevideo = path;
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(iframevideo);

                    var image = document.DocumentNode.FirstChild;

                    if (image.Attributes["width"] != null)
                    {
                        image.Attributes["width"].Remove();
                        
                    }
                    
                    if (image.Attributes["height"] != null)
                    {
                        image.Attributes["height"].Remove();
                        
                    }
                   
                    string VideoPath = image.OuterHtml;
                    string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {

                        objSqlConnection.Open();


                        SqlCommand cmd2 = new SqlCommand("Delete_VideoPortfolios", objSqlConnection);
                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        DataTable dtable2 = new DataTable();
                        cmd2.CommandType = CommandType.StoredProcedure;

                        cmd2.Parameters.Add("@VideoPath", SqlDbType.NVarChar, -1).Value = VideoPath;
                        //cmd2.Parameters.AddWithValue("@VideoPath", VideoPath);
                        cmd2.Parameters.AddWithValue("@PhotographerID", Utility.Utility.Decode(PhotographerID));


                        SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag2.Direction = ParameterDirection.Output;
                        cmd2.Parameters.Add(@P_SuccessFlag2);

                        SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage2.Direction = ParameterDirection.Output;
                        cmd2.Parameters.Add(@P_SuccessMessage2);
                        da2.Fill(dtable2);
                        result = 1;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    result = 0;
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                }
            }

            return Json(result);
        }

        [HttpPost]
        public ActionResult DeleteImage(string path, string PhotographerID)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(PhotographerID) && !string.IsNullOrEmpty(path))
            {
                try
                {
                    #region Physical path
                    //string path1 = Server.MapPath("");
                    string root = AppDomain.CurrentDomain.BaseDirectory; //Will Dynamically Take The Root Path
                                                                         //if (System.IO.File.Exists(Path.Combine(root, path)))
                    if (System.IO.File.Exists(root + path))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(root + path); // Path.Combine(root, path)
                        result = 1;

                    }
                    #endregion

                    #region Database path

                    string ImagePath = path;
                    string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {

                        objSqlConnection.Open();


                        SqlCommand cmd2 = new SqlCommand("Delete_ImagePortfolios", objSqlConnection);
                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        DataTable dtable2 = new DataTable();
                        cmd2.CommandType = CommandType.StoredProcedure;

                        cmd2.Parameters.Add("@ImagePath", SqlDbType.NVarChar, -1).Value = ImagePath;
                        //cmd2.Parameters.AddWithValue("@ImagePath", ImagePath);
                        cmd2.Parameters.AddWithValue("@PhotographerID", Utility.Utility.Decode(PhotographerID));


                        SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag2.Direction = ParameterDirection.Output;
                        cmd2.Parameters.Add(@P_SuccessFlag2);

                        SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage2.Direction = ParameterDirection.Output;
                        cmd2.Parameters.Add(@P_SuccessMessage2);
                        da2.Fill(dtable2);

                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    result = 0;
                }
            }

            return Json(result);
        }

        #endregion

        #region Supporting Methods
        public int GetPhotographerID()
        {
            int id = 0;
            string ConnectionString = string.Empty;
            #region Passing Parameters To Stored Procedure
            ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
            {

                objSqlConnection.Open();
                SqlCommand objSqlCommand = new SqlCommand("GetPhotographerID", objSqlConnection);
                SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                DataTable dt = new DataTable();
                objSqlCommand.CommandType = CommandType.StoredProcedure;

                adapt.Fill(dt);



                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        id = Convert.ToInt32(dt.Rows[i]["ID"]);
                    }
                }
                else
                {

                }

                //--------Itna Code Apun Likhela Hai----------



                objSqlConnection.Close();

            }

            #endregion
            return id;
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
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Message = ex.Message;
            }

            return result;
        }

        public bool SendPasswordEmail(string Email, string Passsword)
        {
            bool status = false;
            string To = Email;
            string From = ConfigurationManager.AppSettings["EmailFrom"];
            string mailbody = "Welcome to Pixthon!<br/><br/>Hello, <br/>Thank you for joining us!We're glad to have you as our photographer partner, and we are excited for you to start exploring our products. <br/><br/>";
            mailbody += "To activate your account use this < b>Password : " + Passsword + " </b>.<br/><br/>Love,<br/><br/>Pixthon";
            //string mailbody = "Your PassWord is" + Passsword;

            string Subject = "Pixthon Login Password";
            Utility.Utility u = new Utility.Utility();
            string reply = u.SendEmail(To, From, mailbody, Subject);
            if (reply == "Email Sent Successfully")
                status = true;
            else if (reply == "Sending Email Failed")
                status = false;

            return status;
        }

        public string GetPassword()
        {
            string pass = "";
            string allowedChars = "";

            //allowedChars = "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,";

            //allowedChars += "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";

            allowedChars += "1,2,3,4,5,6,7,8,9,0";

            char[] sep = { ',' };

            string[] arr = allowedChars.Split(sep);

            string passwordString = "";

            string temp = "";
            int Passwordlength = Convert.ToInt32(ConfigurationManager.AppSettings["Passwordlength"]);

            Random rand = new Random();

            for (int i = 0; i < Passwordlength; i++)

            {

                temp = arr[rand.Next(0, arr.Length)];

                passwordString += temp;

            }

            pass = passwordString;
            return pass;
        }

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
                    SqlCommand objSqlCommand = new SqlCommand("IsAlreadySignedUp", objSqlConnection);
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
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Message = ex.Message;
            }

            return result;
        }

        public List<PhotographyCategories> GetEventTypes(DataTable dataTable)
        {
            List<PhotographyCategories> list = new List<PhotographyCategories>();

            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetServices", objSqlConnection);
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

                            list.Add(new PhotographyCategories
                            {
                                ID = Convert.ToInt32(rd["PhotographyCategoriesID"]),
                                Name = Convert.ToString(rd["Name"]),
                                IsChecked = IsCheckedOrNotService(dataTable, Convert.ToInt32(rd["PhotographyCategoriesID"]))

                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Message = ex.Message;
            }
            return list;
        }

        public bool IsCheckedOrNotService(DataTable dt, int ID)
        {
            bool status = false;
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (Convert.ToInt32(dt.Rows[i]["PhotographyCategoriesID"]) == ID)
                        {
                            status = true;
                        }
                    }
                }
            }


            return status;
        }

        public bool IsCheckedOrNotSubCategory(DataTable dt, int ID)
        {
            bool status = false;
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (Convert.ToInt32(dt.Rows[i]["ID"]) == ID)
                        {
                            status = true;
                        }
                    }
                }
            }


            return status;
        }

        public string IsCheckedOrNotTeam(DataTable dt, int ID)
        {
            string status = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (Convert.ToInt32(dt.Rows[i]["TeamSizeID"]) == ID)
                        {
                            status = "checked";
                        }
                    }
                }
            }


            return status;
        }

        public string IsCheckedOrNotWork(DataTable dt, int ID)
        {
            string status = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (Convert.ToInt32(dt.Rows[i]["ID"]) == ID)
                        {
                            status = "checked";
                        }
                    }
                }
            }


            return status;
        }

        public List<TeamSize> GetTeamSizes(DataTable dt)
        {
            List<TeamSize> list = new List<TeamSize>();

            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetTeamSize", objSqlConnection);
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

                            list.Add(new TeamSize
                            {
                                ID = Convert.ToInt32(rd["TeamSizeID"]),
                                Name = Convert.ToString(rd["NoOfTeamMembers"]),
                                IsChecked = IsCheckedOrNotTeam(dt, Convert.ToInt32(rd["TeamSizeID"]))

                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Message = ex.Message;
            }
            return list;
        }
        public List<Work> GetWorkTypes(DataTable dt)
        {
            List<Work> list = new List<Work>();

            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetWork", objSqlConnection);
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

                            list.Add(new Work
                            {
                                ID = Convert.ToInt32(rd["ID"]),
                                Name = Convert.ToString(rd["WorkType"]),
                                IsChecked = IsCheckedOrNotWork(dt, Convert.ToInt32(rd["ID"]))

                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                string Message = ex.Message;
            }
            return list;
        }
        #endregion

        #region AutoComplete For States And Cities
        [HttpPost]
        public JsonResult SearchState(string Prefix)
        {

            List<StateAuto> rate = new List<StateAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            //if (Prefix != null)
           // {
                try
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection conn = new SqlConnection())
                    {
                        conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                        using (SqlCommand cmd = new SqlCommand())
                        {
                        if(Prefix !=null && Prefix != "")
                        {
                            cmd.CommandText = "select state_name,state_code from all_states where state_name like '" + Prefix + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select state_name,state_code from all_states ";
                        }
                            
                            //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                            cmd.Connection = conn;
                            conn.Open();

                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {

                                    rate.Add(new StateAuto
                                    {

                                        StateID = Convert.ToInt32(sdr["state_code"]),
                                        StateName = Convert.ToString(sdr["state_name"]),

                                    });


                                }
                            }
                            conn.Close();

                        }
                    }
                //var modifiedData = rate.Select(x => new
                //{
                //    id = x.StateID,
                //    text = x.StateName
                //});
                //return Json(modifiedData, JsonRequestBehavior.AllowGet);
                var StateList = (from N in rate
                                 where N.StateName.ToUpper().StartsWith(Prefix.ToUpper())
                                 select new { N });
                return Json(rate, JsonRequestBehavior.AllowGet);

                #endregion

            }
            catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    string Message = ex.Message;
                }

            return Json(null, JsonRequestBehavior.AllowGet);


            //}
            //else
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
        }

        [HttpPost]
        public JsonResult SearchCity(string Prefix, string state_code)
{

            List<CityAuto> rate = new List<CityAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            if (Prefix != null)
            {
                try
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection conn = new SqlConnection())
                    {
                        conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "select city_name,state_code,city_code from all_cities where state_code='" + Convert.ToInt32(state_code) + "' and city_name like '" + Prefix + "' + '%'";
                            //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                            cmd.Connection = conn;
                            conn.Open();

                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {

                                    rate.Add(new CityAuto
                                    {

                                        CityID = Convert.ToInt32(sdr["city_code"]),
                                        CityName = Convert.ToString(sdr["city_name"]),

                                    });


                                }
                            }
                            conn.Close();

                        }
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    string Message = ex.Message;
                }

                var CityList = (from N in rate
                                 where N.CityName.ToUpper().StartsWith(Prefix.ToUpper())
                                 select new { N });


                return Json(rate, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        #region Multiselect
        //public JsonResult GetCountryList(string searchTerm)
        //{

        //    var CounrtyList = db.all_states.ToList();

        //    if (searchTerm != null)
        //    {
        //        CounrtyList = db.all_states.Where(x => x.state_name.Contains(searchTerm)).ToList();
        //    }

        //    var modifiedData = CounrtyList.Select(x => new
        //    {
        //        id = x.state_code,
        //        text = x.state_name
        //    });
        //    return Json(modifiedData, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetCityList(string searchTerm)
        //{

        //    var CounrtyList = db.all_cities.ToList();


        //    if (searchTerm != null)
        //    {
        //        CounrtyList = db.all_cities.Where(x => x.city_name.ToLower().Contains(searchTerm.ToLower())).ToList();
        //    }

        //    var modifiedData = CounrtyList.Select(x => new
        //    {
        //        id = x.city_code,
        //        text = x.city_name
        //    });
        //    return Json(modifiedData, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetStateList(string CountryIDs, string searchTerm)
        //{

        //    List<int> CountryIdList = new List<int>();
        //    var CounrtyList = db.all_cities.ToList();
        //    CountryIdList = CountryIDs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        //    List<all_cities> StateList = new List<all_cities>();
        //    foreach (int countryID in CountryIdList)
        //    {
        //        db.Configuration.ProxyCreationEnabled = false;
        //        var listDataByCountryID = db.all_cities.Where(x => x.state_code == countryID).ToList();
        //        foreach (var item in listDataByCountryID)
        //        {
        //            StateList.Add(item);

        //        }

        //    }

        //    if (searchTerm != null)
        //    {
        //        StateList = StateList.Where(x => x.city_name.ToLower().Contains(searchTerm.ToLower())).ToList();
        //    }
        //    var modifiedData = StateList.Select(x => new
        //    {
        //        id = x.city_code,
        //        text = x.city_name
        //    });

        //    return Json(modifiedData, JsonRequestBehavior.AllowGet);

        //}
        #endregion

        #region Categories Select2

        public JsonResult GetCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            if (string.IsNullOrEmpty(id))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            Utility.Utility u = new Utility.Utility();
            RegistrationModel obj = new RegistrationModel();
            if (id == "0" )
            {
                id = "0";
                obj.PhotographerID = Convert.ToInt32(id);
                id = u.Encode(id);
                obj.EncodedPhotographerID = Convert.ToString(id);
            }

            int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            List<PhotographyCategories> list = new List<PhotographyCategories>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            DataTable dataTable = null;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    #region Only Selected Photographer Categories
                    if (id2 != 0)
                    {


                        SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataSet ds = new DataSet();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                        SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessFlag);

                        SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessMessage);
                        adapt.Fill(ds);

                        dataTable = ds.Tables[2];
                    }
                    

                    #endregion
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                list.Add(new PhotographyCategories
                                {
                                    ID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),
                                    IsChecked = IsCheckedOrNotService(dataTable, Convert.ToInt32(sdr["PhotographyCategoriesID"]))

                                });

                                //rate.Add(new CategoriesAuto
                                //{

                                //    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                //    Name = Convert.ToString(sdr["Name"]),
                                //    IsChecked = IsCheckedOrNotService(dataTable, Convert.ToInt32(rd["PhotographyCategoriesID"]))

                                //});


                            }
                        }
                        
                    }
                    #endregion
                    
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    list = list.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = list.Select(x => new
                {
                    id = x.ID,
                    text = x.Name,
                    selected = x.IsChecked
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetWeddingCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            if (string.IsNullOrEmpty(id))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            Utility.Utility u = new Utility.Utility();
            RegistrationModel obj = new RegistrationModel();
            if (id == "0")
            {
                obj.PhotographerID = Convert.ToInt32(id);
                id = u.Encode(id);
                obj.EncodedPhotographerID = Convert.ToString(id);
            }

            int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            List<PhotographySubCategories> list = new List<PhotographySubCategories>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    #region Only Selected Photographer Categories
                    SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataSet ds = new DataSet();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    adapt.Fill(ds);

                    DataTable dataTable = ds.Tables[2];

                    #endregion
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                list.Add(new PhotographySubCategories
                                {
                                    ID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),
                                    IsChecked = IsCheckedOrNotService(dataTable, Convert.ToInt32(sdr["PhotographyCategoriesID"]))

                                });

                             
                            }
                        }

                    }
                    #endregion

                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    list = list.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = list.Select(x => new
                {
                    id = x.ID,
                    text = x.Name,
                    selected = x.IsChecked
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);



        }
        public JsonResult GetOtherCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetWildLifeCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetCommercialCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetFashionCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetBirthdayCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetKidCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetCorporateCategories(string id, string searchTerm)
        {
            //int RegistrationID = 0;
            //if (string.IsNullOrEmpty(id))
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //Utility.Utility u = new Utility.Utility();
            //RegistrationModel obj = new RegistrationModel();
            //if (id == "0")
            //{
            //    obj.PhotographerID = Convert.ToInt32(id);
            //    id = u.Encode(id);
            //    obj.EncodedPhotographerID = Convert.ToString(id);
            //}

            //int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    #region All PhotographerCategories
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }

                    }
                    #endregion
                    //#region Only Selected Photographer Categories
                    //SqlCommand objSqlCommand = new SqlCommand("GetPhotograherDetails", conn);
                    //SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    //DataSet ds = new DataSet();
                    //objSqlCommand.CommandType = CommandType.StoredProcedure;
                    //objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);

                    //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    //@P_SuccessFlag.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    //@P_SuccessMessage.Direction = ParameterDirection.Output;
                    //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    //adapt.Fill(ds);

                    //DataTable dtprofile = ds.Tables[0];

                    //#endregion
                    conn.Close();
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name,
                    selected = true
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }

        #endregion

        

        public JsonResult GetSubCategories(string id,string CountryIDs, string searchTerm)
        {
            //int RegistrationID = 0;
            if (string.IsNullOrEmpty(id))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            Utility.Utility u = new Utility.Utility();
            RegistrationModel obj = new RegistrationModel();
            if (id == "0")
            {
                obj.PhotographerID = Convert.ToInt32(id);
                id = u.Encode(id);
                obj.EncodedPhotographerID = Convert.ToString(id);
            }

            int id2 = Convert.ToInt32(Utility.Utility.Decode(id));
            List<int> CountryIdList = new List<int>();
            //var CounrtyList = db.all_cities.ToList();
            CountryIdList = CountryIDs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            List<PhotoTypeAuto> rate = new List<PhotoTypeAuto>();
            List<PhotographerType> StateList = new List<PhotographerType>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            
                try
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection conn = new SqlConnection())
                    {
                        conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                        foreach (int countryID in CountryIdList)
                        {
                        #region Only Selected Photographer Categories
                        SqlCommand objSqlCommand = new SqlCommand("GetSubCategory", conn);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataSet ds = new DataSet();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", id2);
                        objSqlCommand.Parameters.AddWithValue("@CategoryID", countryID);

                        SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessFlag);

                        SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessMessage);
                        adapt.Fill(ds);

                        DataTable dataTable = ds.Tables[0];

                        #endregion
                        using (SqlCommand cmd = new SqlCommand())
                            {
                            if (searchTerm != null)
                            {
                                cmd.CommandText = "select * from photographertype where CategoryID='" + countryID + "' and photographytype like '" + searchTerm + "' + '%'";
                            }
                            else
                            {
                                cmd.CommandText = "select * from photographertype where CategoryID='" + countryID + "'";
                            }
                                cmd.Connection = conn;
                                conn.Open();

                                using (SqlDataReader sdr = cmd.ExecuteReader())
                                {
                                    while (sdr.Read())
                                    {

                                        StateList.Add(new PhotographerType
                                        {

                                            ID = Convert.ToInt32(sdr["ID"]),
                                            PhotographyType = Convert.ToString(sdr["PhotographyType"]),
                                            IsChecked = IsCheckedOrNotSubCategory(dataTable, Convert.ToInt32(sdr["ID"]))

                                        });


                                    }
                                }
                                conn.Close();

                            }


                        }

                    }

                    #endregion
                    if (searchTerm != null)
                    {
                        StateList = StateList.Where(x => x.PhotographyType.ToLower().Contains(searchTerm.ToLower())).ToList();
                    }
                    var modifiedData = StateList.Select(x => new
                    {
                        id = x.ID,
                        text = x.PhotographyType,
                        selected = x.IsChecked
                    });

                    return Json(modifiedData, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    string Message = ex.Message;
                }




                return Json(null, JsonRequestBehavior.AllowGet);
            


        }

        public JsonResult GetCategoriesSelect(string searchTerm)
        {

            List<CategoriesAuto> rate = new List<CategoriesAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (searchTerm != null && searchTerm != "")
                        {
                            cmd.CommandText = "select * from tbl_photographycategories where name like '" + searchTerm + "' + '%'";
                        }
                        else
                        {
                            cmd.CommandText = "select * from tbl_photographycategories";
                        }

                        //cmd.Parameters.AddWithValue("@SearchText", prefixText);
                        cmd.Connection = conn;
                        conn.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {

                                rate.Add(new CategoriesAuto
                                {

                                    PhotographyCategoriesID = Convert.ToInt32(sdr["PhotographyCategoriesID"]),
                                    Name = Convert.ToString(sdr["Name"]),

                                });


                            }
                        }
                        conn.Close();

                    }
                }

                #endregion
                if (searchTerm != null)
                {
                    rate = rate.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                var modifiedData = rate.Select(x => new
                {
                    id = x.PhotographyCategoriesID,
                    text = x.Name
                });

                return Json(modifiedData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(null, JsonRequestBehavior.AllowGet);


        }
        #endregion

        #region Not Used
        //public void Hash(string Password)
        //{
        //    var PassBytes = new UTF8Encoding().GetBytes(Password);

        //    using (var algorithm = new System.Security.Cryptography.SHA512Managed())
        //    {
        //        HashPassBytes = algorithm.ComputeHash(PassBytes);
        //        PassHash = Convert.ToBase64String(HashPassBytes);
        //    }
        //}
        //[HttpPost]
        //public ActionResult UploadOwner()
        //{

        //    HttpPostedFileBase ownerphotofile = Request.Files["ownerphotofile"];
        //    if (ownerphotofile != null)
        //    {
        //        try
        //        {
        //            string path = Server.MapPath("~/RegistrationImages/OwnerPhoto/");
        //            if (!Directory.Exists(path))
        //            {
        //                //If Directory (Folder) does not exists Create it.
        //                Directory.CreateDirectory(path);

        //            }
        //            string filenameownerphotofile = Path.GetFileName(ownerphotofile.FileName);
        //            string contentTypeownerphotofile = ownerphotofile.ContentType;
        //            string extension = Path.GetExtension(ownerphotofile.FileName);

        //            ownerphotofile.SaveAs(Path.Combine(path, filenameownerphotofile + extension));



        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }


        //    return Json(" Files Uploaded!");
        //}

        //[HttpPost]
        //public ActionResult UploadLogo()
        //{
        //    HttpPostedFileBase ownerphotofile = Request.Files["ownerphotofile"];
        //    if (ownerphotofile != null)
        //    {
        //        try
        //        {
        //            string path = Server.MapPath("~/RegistrationImages/OwnerPhoto/");
        //            if (!Directory.Exists(path))
        //            {
        //                //If Directory (Folder) does not exists Create it.
        //                Directory.CreateDirectory(path);

        //            }
        //            string filenameownerphotofile = Path.GetFileName(ownerphotofile.FileName);
        //            string contentTypeownerphotofile = ownerphotofile.ContentType;
        //            string extension = Path.GetExtension(ownerphotofile.FileName);

        //            ownerphotofile.SaveAs(Path.Combine(path, filenameownerphotofile + extension));



        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }


        //    return Json(" Files Uploaded!");
        //}

        //[HttpPost]
        //public ActionResult UploadCover()
        //{
        //    HttpPostedFileBase ownerphotofile = Request.Files["ownerphotofile"];
        //    if (ownerphotofile != null)
        //    {
        //        try
        //        {
        //            string path = Server.MapPath("~/RegistrationImages/OwnerPhoto/");
        //            if (!Directory.Exists(path))
        //            {
        //                //If Directory (Folder) does not exists Create it.
        //                Directory.CreateDirectory(path);

        //            }
        //            string filenameownerphotofile = Path.GetFileName(ownerphotofile.FileName);
        //            string contentTypeownerphotofile = ownerphotofile.ContentType;
        //            string extension = Path.GetExtension(ownerphotofile.FileName);

        //            ownerphotofile.SaveAs(Path.Combine(path, filenameownerphotofile + extension));



        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }


        //    return Json(" Files Uploaded!");
        //}
        #endregion
    }
}