using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UploadMusic.Models;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class HomeController : Controller
    {
        #region Unused
        public ActionResult DisplayImages(string PinCode)
        {
            PhotoDetailsModel images = GetImages(PinCode);
            return View(images);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

       
        [HttpPost]
        public ActionResult Upload()
        {
            HttpFileCollectionBase files = Request.Files;
            try
            {
                string path = Server.MapPath("~/Upload/");
                if (!Directory.Exists(path))
                {
                    //If Directory (Folder) does not exists Create it.
                    Directory.CreateDirectory(path);
                }
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    file.SaveAs(path + file.FileName);


                    string filename = file.FileName;//Path.GetFileName(FileUpload1.PostedFile.FileName);
                    string contentType = file.ContentType;
                    using (Stream fs = file.InputStream)
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            byte[] bytes = br.ReadBytes((Int32)fs.Length);
                            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                            using (SqlConnection con = new SqlConnection(constr))
                            {
                                string query = "insert into tblFiles values (@Name, @ContentType, @Data)";
                                using (SqlCommand cmd = new SqlCommand(query))
                                {
                                    cmd.Connection = con;
                                    cmd.Parameters.AddWithValue("@Name", filename);
                                    cmd.Parameters.AddWithValue("@ContentType", contentType);
                                    cmd.Parameters.AddWithValue("@Data", bytes);
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
            }

            return Json(files.Count + " Files Uploaded!");
        }
        #region For SIngle File Upload
        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase files)
        {

            String FileExt = Path.GetExtension(files.FileName).ToUpper();

            if (FileExt == ".PDF")
            {


                using (Stream str = files.InputStream)
                {
                    using (BinaryReader Br = new BinaryReader(str))
                    {
                        Byte[] FileDet = Br.ReadBytes((Int32)str.Length);
                        string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                        using (SqlConnection con = new SqlConnection(constr))
                        {
                            string query = "insert into Tbl_Photos (PhotoFileBytes,SecurityCode,PhotoGrapherID,PhotographyCategoriesID,CreatedOn,ModifiedOn) " +
                                "values(@PhotoFileBytes,@SecurityCode,@PhotoGrapherID,@PhotographyCategoriesID,@CreatedOn,@ModifiedOn)";
                            using (SqlCommand cmd = new SqlCommand(query))
                            {
                                cmd.Connection = con;
                                cmd.Parameters.AddWithValue("@PhotoFileBytes", FileDet);
                                cmd.Parameters.AddWithValue("@SecurityCode", "testsec123");
                                cmd.Parameters.AddWithValue("@PhotoGrapherID", 1);
                                cmd.Parameters.AddWithValue("@PhotographyCategoriesID", 1);
                                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                                cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);

                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                    }
                }
                //SaveFileDetails(Fd);
                return RedirectToAction("FileUpload");
            }
            else
            {

                ViewBag.FileStatus = "Invalid file format.";
                return View();

            }

        }
        #endregion

        public string CreateSecurityCode()
        {
            string securitycode = "";
            Random r = new Random();
            string random = r.Next(10000000, 99999999).ToString();
            securitycode = "PIXTHON" + random;
            return securitycode;
        }

        public string GenerateSecurityCodeDB()
        {
            string securitycode = "";

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GenerateSecurityCode";

                    cmd.Parameters.Add("@serialnumber", SqlDbType.NVarChar, 100);
                    cmd.Parameters["@serialnumber"].Direction = ParameterDirection.Output;

                    try
                    {
                        con.Open();
                        int i = cmd.ExecuteNonQuery();
                        securitycode = Convert.ToString(cmd.Parameters["@serialnumber"].Value);
                    }
                    catch (Exception ex) {
                        string error = Utility.Utility.LogErrorS(ex);
                        Log.Error(error);
                    }
                    finally
                    {
                        con.Close();
                    }

                }

                return securitycode;
            }


        }
        [HttpPost]
        public JsonResult Index(string Prefix)
        {
            //Note : you can bind same list from database  
            List<City> ObjList = new List<City>()
            {

                new City {Id=1,CityName="Latur" },
                new City {Id=2,CityName="Mumbai" },
                new City {Id=3,CityName="Pune" },
                new City {Id=4,CityName="Delhi" },
                new City {Id=5,CityName="Dehradun" },
                new City {Id=6,CityName="Noida" },
                new City {Id=7,CityName="New Delhi" }

        };
            //Searching records from list using LINQ query  
            var CityList = (from N in ObjList
                            where N.CityName.ToUpper().StartsWith(Prefix.ToUpper())
                            select new { N });
            return Json(CityList, JsonRequestBehavior.AllowGet);
        }

        #endregion Unused

        #region Contact
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ContactSubmit(PhotographerProfileModel objEmail)
        {
            PhotoGrapherDetail obj = new PhotoGrapherDetail();
            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            string PixthonEmailmsg = "";
            string CustomerEmailmsg = "";
            //string PhotographerEmailmsg = "";
            //string Smsmsg = "";
            // string service = GetService(objEmail.EmailDetailsModel.Category);
            try
            {
                // obj = GetPhotographerDetails("", objEmail.EmailDetailsModel.PhotographerID);
                string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];

                #region For Sending Email to Pixthon
                string PixthonMail = ConfigurationManager.AppSettings["EnquiryPixthon"];
                string HtmlBody = "Hello, <br/> Here are the details of your new query<br/><br/>";
                HtmlBody += "<b>Name :</b> " + objEmail.EmailDetailsModel.CustName + "<br/> ";
                HtmlBody += "<b>Email ID :</b> " + objEmail.EmailDetailsModel.CustEmail + " <br/>";
                HtmlBody += "<b>Phone No :</b> " + objEmail.EmailDetailsModel.CustPhone + " <br/>";
                HtmlBody += "<b>Message : </b>" + objEmail.EmailDetailsModel.CustMsg + "<br/>";
     
                //string mailbody = obj.StudioName + " has received an inquiry from " + objEmail.EmailDetailsModel.CustName + "<br/> Customer name: " + objEmail.EmailDetailsModel.CustName + "<br/> Customer Email: " + objEmail.EmailDetailsModel.CustEmail + "<br/> Customer Contact No:" + objEmail.EmailDetailsModel.CustPhone + "<br/> Enquiry Message: " + objEmail.EmailDetailsModel.CustMsg + "<br/> Thanks";
                string Subject = "You have a new query";
                PixthonEmailmsg = utility.SendEmail(PixthonMail, EmailFrom, HtmlBody, Subject);
                #endregion



                #region For Sending Email to Customer
                string CustomerMail = objEmail.EmailDetailsModel.CustEmail;
                //string mailbodyCustomer = "Thank you for enquiring with " + obj.StudioName + ". We will get in touch with you shortly.<br/>Thanks for choosing us<br/><br/>Thanks,<br/>" + obj.PhotographerName + "<br/>" + obj.StudioName;
                string mailbodyCustomer = "Hi " + objEmail.EmailDetailsModel.CustEmail + ",<br/>";
                mailbodyCustomer += "Thank you for contacting Pixthon.<br/>";
                mailbodyCustomer += "We have received your email and will get back to you with a response as soon as possible.<br/>";
                mailbodyCustomer += "<br/>If you have general questions you can check out our FAQs.<br/>";
                mailbodyCustomer += "If you have any additional information that you think will help us to assist you, please feel free to reply to this email.<br/>";
                mailbodyCustomer += "<br/>Cheers,<br/>";
                mailbodyCustomer += "Pixthon Digital Solutions Pvt Ltd<br/>";

                string SubjectCustomer = "Thank you for contacting us";
                CustomerEmailmsg = utility.SendEmail(CustomerMail, EmailFrom, mailbodyCustomer, SubjectCustomer);
                #endregion

                #region For Sending SMS
                //Smsmsg = utility.SendSMS(objEmail.EmailDetailsModel.Receipientno, objEmail.EmailDetailsModel.Msgtxt);
                #endregion

                if (PixthonEmailmsg == "Email Sent Successfully" && CustomerEmailmsg == "Email Sent Successfully")
                {
                    msg = "Email Sent Successfully";
                }

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                msg = "sent UNSuccessfully";
            }
            return Json(new { Message = msg });

        }
        #endregion

        #region Upload PhotoEbook



        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult UploadPDF(string on)
        {
            if (string.IsNullOrEmpty(on))
            {
                return HttpNotFound();
            }
            Utility.Utility u = new Utility.Utility();
            PhotoBookModel obj = new PhotoBookModel();
            if (on == "first")
            {
                on = u.Encode(on);
                obj.PhotographerList = Getcategory(null);
                obj.SizeList = GetSize(null);
            }
            else
            {
                on = u.Encode(on);
            }
            string id2 = Convert.ToString(Utility.Utility.Decode(on));


            if (id2 != "first")
            {
                try
                {
                    string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {
                        objSqlConnection.Open();
                        SqlCommand objSqlCommand = new SqlCommand("GetPhotobookDetails", objSqlConnection);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataSet ds = new DataSet();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@OrderNo", id2);

                        SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessFlag);

                        SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessMessage);
                        adapt.Fill(ds);

                        DataTable dtprofile = ds.Tables[0];
                       
                       


                        if (dtprofile.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtprofile.Rows.Count; i++)
                            {

                                obj.SecurityCode = Convert.ToString(dtprofile.Rows[i]["SecurityCode"]);
                                obj.Title = (Convert.ToString(dtprofile.Rows[i]["Title"]));
                                obj.OrderNo = Convert.ToString(dtprofile.Rows[i]["OrderNo"]);
                                obj.PhotographerName = Convert.ToString(dtprofile.Rows[i]["PhotograherName"]);
                                obj.StudioName = Convert.ToString(dtprofile.Rows[i]["StudioName"]);
                                obj.Email = Convert.ToString(dtprofile.Rows[i]["Email"]);
                                obj.SizeList= GetSize(dtprofile);
                                obj.PhotographerList = Getcategory(dtprofile);
                                obj.PhotographerId= Convert.ToInt32(dtprofile.Rows[i]["PhotographerID"]);


                            }




                        }
                        else
                        {

                        }

                       

                        objSqlConnection.Close();

                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    string Message = ex.Message;
                }


            }
            else
            {
               
            }
            return View(obj);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult ImageUpload(PhotoBookModel obj)
        {
            #region Declaration
            int i = 1;
            string SecurityCode = "";
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            SqlTransaction tran = null;
            HttpFileCollectionBase files = Request.Files;
            List<string> pathlist = new List<string>();
            string actiontype = "";
            string msg1 = string.Empty;
            #endregion

            #region Check for insert or update order no ,validity
            Log.Info("HomeController-ImageUpload-Check for insert or update order no ,validity");
            if ((obj.PhotographerId) == 0)
            {
                Log.Info("HomeController-ImageUpload-PhotographerID is 0");
                Log.Info("HomeController-ImageUpload-PhotographerID is 0");
                return Json(new { result = "Invalid Photographer" });
            }
            Log.Info("HomeController-ImageUpload-PhotographerID -"+ obj.PhotographerId);

            if (OrderNoAlreadyExist(obj.OrderNo) != false)
            {
                Log.Info("HomeController-ImageUpload- Update functionality");
                if (obj.Parameter=="first")
                {
                    return Json(new { result = "ALready a Member" });
                }
                obj.NoofViews = GetNoofViews(obj.OrderNo);
                actiontype = "Update";
                SecurityCode =GetSecurityCodeOnOrderNO(obj.OrderNo);
                Log.Info("HomeController-ImageUpload-Getting SecurityCode");
            }
            else
            {
                obj.NoofViews = 0;
                Log.Info("HomeController-ImageUpload- Update functionality");
                actiontype = "Insert";
            }
            Log.Info("HomeController-ImageUpload-End Check for insert or update order no ,validity");
            #endregion

            #region Check Email based on PhotographerID
            Log.Info("HomeController-ImageUpload-Check Email based on PhotographerID");
            try
            {
                
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "GetEmail";
                        cmd.Parameters.AddWithValue("@PhotographerID", obj.PhotographerId);
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                obj.Email = Convert.ToString(sdr["Email"]);
                            }
                        }
                        con.Close();
                    }
                    if (obj.Email == "")
                    {
                        return Json(new { result = "Invalid" });
                    }



                }

            }
            catch (Exception ex)
            {
                string error=Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                msg1 = "Server Timeout";
                return Json(new { result = "Error" });

            }
            Log.Info("HomeController-ImageUpload-Check Email based on PhotographerID");
            #endregion

            using (SqlConnection con = new SqlConnection(constr))
            {
                try
                {
                   
                    con.Open();
                    tran = con.BeginTransaction();
                    #region Generating SecurityCode on Insert
                    Log.Info("HomeController-ImageUpload-Generating SecurityCode on Insert");
                    if (actiontype=="Insert")
                    {
                        //Generate Pixthon Code
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GenerateSecurityCode";
                            cmd.Transaction = tran;//NEwly Added

                            cmd.Parameters.Add("@serialnumber", SqlDbType.NVarChar, 100);
                            cmd.Parameters["@serialnumber"].Direction = ParameterDirection.Output;


                            tran.Save("save1");
                            cmd.ExecuteReader();
                            SecurityCode = Convert.ToString(cmd.Parameters["@serialnumber"].Value);
                        }

                    }
                    Log.Info("HomeController-ImageUpload-End Generating SecurityCode on Insert");
                    #endregion

                   

                    #region Insert Or Update Ebook
                    //[Harcoded]
                    Log.Info("HomeController-ImageUpload-Insert Or Update Ebook");
                    byte[] music = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;//Newly Added
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "InsertSecurityCode";
                        cmd.Transaction = tran;
                        cmd.Parameters.AddWithValue("@SecurityCode", SecurityCode);
                        cmd.Parameters.Add("@MusicFileBytes",SqlDbType.VarBinary,-1).Value= music;
                        cmd.Parameters.AddWithValue("@PhotographerID", obj.PhotographerId);
                        cmd.Parameters.AddWithValue("@PhotographyCategoriesID",obj.PhotographyCategoriesID );
                        cmd.Parameters.AddWithValue("@SizeID", obj.SizeID);
                        cmd.Parameters.AddWithValue("@Title",obj.Title);
                        cmd.Parameters.AddWithValue("@OrderNo",obj.OrderNo);
                        cmd.Parameters.AddWithValue("@NoofViews", obj.NoofViews);
                        cmd.Parameters.AddWithValue("@TypeofPhotobook", obj.TypeofPhotobook);
                        cmd.Parameters.AddWithValue("@actionType", actiontype);
                        
                        tran.Save("save2");
                        cmd.ExecuteNonQuery();
                       
                    }
                    Log.Info("HomeController-ImageUpload-Insert Or Update Ebook");
                    #endregion

                    #region Saving Bytes and Path of Images
                    Log.Info("HomeController-ImageUpload-Saving Bytes and Path of Images");

                    byte[] FileDet;
                    string path = Server.MapPath("~/RegistrationImages/PhotoEbook/" + obj.OrderNo + "/");
                    if (!Directory.Exists(path))
                    {
                        //If Directory (Folder) does not exists Create it.
                        Directory.CreateDirectory(path);
                    }
                    
                    //string Path = Server.MapPath("~/RegistrationImages/PhotoEbook/" + obj.OrderNo + "/");
                    DirectoryInfo di = new DirectoryInfo(path);
                    List<string> filePaths = new List<string>();/* Directory.GetFiles("~/RegistrationImages/Temp/" + objCompression.OrderNo + "/", "*.jpg").ToList();*/
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        string FileDataImages = "\\RegistrationImages\\PhotoEbook\\" + obj.OrderNo + "\\" + fi.Name;
                        FileDet = System.IO.File.ReadAllBytes(path + fi.Name);
                        string photoname = Path.GetFileNameWithoutExtension(path + fi.Name);
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "InsertPhotos";
                            cmd.Transaction = tran;
                            cmd.Parameters.AddWithValue("@PhotoFileBytes", FileDet);
                            cmd.Parameters.AddWithValue("@ImagePath", FileDataImages);
                            cmd.Parameters.AddWithValue("@PhotoNo", i); //For Unqiue Photo Numbers
                            cmd.Parameters.AddWithValue("@SecurityCode", SecurityCode);
                            cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                            cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);
                            cmd.Parameters.AddWithValue("@PhotoName", photoname);

                            tran.Save("save3");
                            cmd.ExecuteNonQuery();
                            i++;

                        }
                    }

                    Log.Info("HomeController-ImageUpload-End Saving Bytes and Path of Images");
                    #endregion
                    #region Sending Email of PhotoBook Creation
                    Log.Info("HomeController-ImageUpload-Sending Email of PhotoBook Creation");
                    Utility.Utility utility = new Utility.Utility();
                    string msg = "";
                    try
                    {
                        string From = ConfigurationManager.AppSettings["EmailFrom"];
                        string Subject = "PIXTHON Photobook Security Code";
                        msg = SendMailForSecuritycode(obj.Title, SecurityCode, obj.Email, From, Subject);
                       
                    }
                    catch (Exception ex)
                    {
                        string error = Utility.Utility.LogErrorS(ex);
                        Log.Error(error);
                        if (tran != null)
                            tran.Rollback();
                        msg = "Server Timeout";
                    }
                    Log.Info("HomeController-ImageUpload-Sending Email of PhotoBook Creation");
                    #endregion

                    #region Delete From Temp Folder
                    Log.Info("HomeController-ImageUpload-Delete From Temp Folder");
                    // Delete all files from the Directory
                    string path1 = Server.MapPath("~/RegistrationImages/Temp/" + obj.OrderNo + "/");
                    if (Directory.Exists(path1))
                    {
                        foreach (string filename in Directory.GetFiles(path1))

                        {

                            System.IO.File.Delete(filename);

                        }

                        // Check all child Directories and delete files

                        //foreach (string subfolder in Directory.GetDirectories(path1))

                        //{

                        //    DeleteDirectory(subfolder);

                        //}

                        Directory.Delete(path1);
                    }
                    Log.Info("HomeController-ImageUpload-End Delete From Temp Folder");
                    #endregion

                    tran.Commit();
                }

                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    if (tran != null)
                        tran.Rollback();
                }
         
            }

            return Json(new { result = "Redirect", url = Url.Action("Index", "PhotographerProfile") });
        }


        public bool OrderNoAlreadyExist(string orderno)
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
                    SqlCommand objSqlCommand = new SqlCommand("OrderNoAlreadyExist", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_OrderNo", orderno);

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
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return result;
        }

        public string GetSecurityCodeOnOrderNO(string orderno)
        {
            string result = "";
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select SecurityCode from Tbl_SecurityCode  where OrderNo='" + orderno + "'", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            result = "";
                            while (rd.Read())
                            {
                                
                                    result = Convert.ToString(rd["SecurityCode"]);
                                
                            }
                        }
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

            return result;
        }

        public int GetNoofViews(string orderno)
        {
            int result = 0;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select NoofViews from Tbl_SecurityCode  where OrderNo='" + orderno + "'", con))
                    {
                        
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            result = 0;
                            while (rd.Read())
                            {

                                result = Convert.ToInt32(rd["NoofViews"]);

                            }
                        }
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

            return result;
        }

        public int GetNoofViewsOnPincode(string pincode)
        {
            int result = 0;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select NoofViews from Tbl_SecurityCode  where SecurityCode='" + pincode + "'", con))
                    {

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            result = 0;
                            while (rd.Read())
                            {

                                result = Convert.ToInt32(rd["NoofViews"]);

                            }
                        }
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

            return result;
        }



        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult ValidateOrder(string orderno)
        {
            string result = "";
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            if (orderno ==null || orderno=="")
            {
                result = "Invalid";
                return Json(new { result = result });
            }
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("OrderNoAlreadyExist", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_OrderNo", orderno);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    adapt.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        result = "Invalid";
                    }
                    else
                    {
                        result = "Success";
                    }

                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }

            return Json(new {result= result });
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult GetImage(string OrderNo)
        {
            string[] fileEntries = null;
            List<string> Entries = new List<string>();
            try
            {
                
                string directoryPath = @"\RegistrationImages\PhotoEbook\" + OrderNo + "\\";
                fileEntries = Directory.GetFiles(Server.MapPath(directoryPath));

                foreach (string fileName in fileEntries)
                {

                    Entries.Add(directoryPath + System.IO.Path.GetFileName(fileName));
                }
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Exception = ex.Message;
            }


            return Json(Entries);
        }

        #region Storing Images In Physical Path
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult UploadImage(string OrderNo, string sizeID, string FolderPath, int bookType)
        {
            if(string.IsNullOrEmpty(OrderNo) || string.IsNullOrEmpty(sizeID) || bookType ==0|| sizeID=="0")
            {
                return Json(new { msg="FillDetails"});
            }
            HttpFileCollectionBase files = Request.Files;
            CompressionModel compressionModel = new CompressionModel();
            compressionModel.OrderNo = OrderNo;
            string Flag = string.Empty;
            //int Count = 0;
            List<string> pathlist = new List<string>();
            try
            {
                Log.Info("HomeController-UploadImage-Check saving photobook");
                string path = Server.MapPath("~/RegistrationImages/PhotoEbook/" + OrderNo + "/");
                if (!Directory.Exists(path))
                {
                    //If Directory (Folder) does not exists Create it.
                    Directory.CreateDirectory(path);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    #region Crop Image Into Equal Parts


                    string SourcePath = Server.MapPath(file.FileName);
                    string TempPath = Server.MapPath("~/RegistrationImages/Temp/" + OrderNo + "/");
                    Log.Info("HomeController-UploadImage-Check saving photobook to temppath");
                    if (!Directory.Exists(TempPath))
                    {
                        //If Directory (Folder) does not exists Create it.
                        Directory.CreateDirectory(TempPath);
                    }
                    #region  Converting Image to Bytes and path
                    compressionModel.ImagePath = TempPath;
                    #endregion
                    //Get Filename without Extension
                    Log.Info("HomeController-UploadImage-Check Gettiing File Name without Extension");

                    string FileName = Path.GetFileNameWithoutExtension(file.FileName).Replace("(", "");
                    FileName = FileName.Trim().ToLower().Replace(")", "");
                    int lastpage = ((files.Count - 3) * 2) + 2;
                    Bitmap secondHalf = null;
                    Bitmap firstHalf = null;
                    string Extension = Path.GetExtension(file.FileName);
                    int page = 0;
                    string first = FileName.Trim().ToLower().Replace(" ", "");
                    string last = first.ToLower().Replace("_", "");
                    string cover = last.ToLower().Replace("-", "");
                    #region Checking If Book Type Is Photobook Or RGB  //He bagh baal he change kele

                    //if (bookType == 1) //For Photobook
                    //{

                        if (cover != "firstpage" && cover != "lastpage" && cover != "coverpad" && cover != "fancycover")
                        {
                            page = Convert.ToInt32(FileName) * 2;
                        }
                   // }
                    //else
                    //{
                    //    if (cover != "firstpage" && cover != "lastpage" && cover != "coverpad" && cover != "fancycover")
                    //    {
                    //        page = ((Convert.ToInt32(FileName) + 1) * 2) - 3;
                    //    }
                    //}

                        Log.Info("HomeController-UploadImage-Check cover = firstpage || cover == lastpage");

                        if (cover == "firstpage" || cover == "lastpage")
                        {
                            Bitmap originalImage = new Bitmap(file.InputStream);
                            //Rectangle rect = new Rectangle(0, 0, originalImage.Width / 1, originalImage.Height);
                            firstHalf = originalImage;
                        }
                        else if (cover == "fancycover")
                        {
                            Bitmap originalImage = new Bitmap(file.InputStream);
                            firstHalf = originalImage;
                        }
                        else
                        {
                            Log.Info("HomeController-UploadImage-Cropping Image Into Equal Parts");
                            Bitmap originalImage = new Bitmap(file.InputStream);
                            Rectangle rect = new Rectangle(0, 0, originalImage.Width / 2, originalImage.Height);
                            firstHalf = originalImage.Clone(rect, originalImage.PixelFormat);
                            rect = new Rectangle(originalImage.Width / 2, 0, originalImage.Width / 2, originalImage.Height);
                            secondHalf = originalImage.Clone(rect, originalImage.PixelFormat);
                        }
                        Log.Info("HomeController-UploadImage-Checking If Image is firstpage or lastpage or coverpad");

                        if (cover == "firstpage")
                        {
                            firstHalf.Save(TempPath + "1" + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else if (cover == "lastpage")
                        {
                            firstHalf.Save(TempPath + Convert.ToString(lastpage) + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else if (cover == "coverpad")
                        {
                            firstHalf.Save(TempPath + "Back" + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                            secondHalf.Save(TempPath + "Front" + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else if (cover == "fancycover")
                        {
                           
                            firstHalf.Save(TempPath + "Front" + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else
                        {
                            Log.Info("HomeController-UploadImage-Saving Cropped Images To Temp Path");
                            firstHalf.Save(TempPath + Convert.ToString(page) + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                            secondHalf.Save(TempPath + Convert.ToString(page + 1) + Extension, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                    

                    #endregion
                    #endregion
                }


                #region Compressing Image
                Log.Info("HomeController-UploadImage-Compressing Images According To Dimensions");
                Utility.Utility u = new Utility.Utility();
                int SizeOrientation = Convert.ToInt32(sizeID);

                if (SizeOrientation == 1)
                {
                    compressionModel.CompressedImageWidth = 1350;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 2)
                {
                    compressionModel.CompressedImageWidth = 1260;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 3)
                {
                    compressionModel.CompressedImageWidth = 600;
                    compressionModel.CompressedImageHeight = 600;
                }
                if (SizeOrientation == 4)
                {
                    compressionModel.CompressedImageWidth = 1125;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 5)
                {
                    compressionModel.CompressedImageWidth = 1350;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 6)
                {
                    compressionModel.CompressedImageWidth = 1350;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 7)
                {
                    compressionModel.CompressedImageWidth = 1214;
                    compressionModel.CompressedImageHeight = 850;
                }
                if (SizeOrientation == 8)
                {
                    compressionModel.CompressedImageWidth = 600;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 9)
                {
                    compressionModel.CompressedImageWidth = 600;
                    compressionModel.CompressedImageHeight = 900;
                }
                if (SizeOrientation == 10)
                {
                    compressionModel.CompressedImageWidth = 720;
                    compressionModel.CompressedImageHeight = 900;
                }
                u.CompressImage(compressionModel);

                Log.Info("HomeController-UploadImage-Compressing Images According To Dimensions-Completed");

                #endregion

                #region Calling Compression Api
                //try
                //{
                //    using (HttpClient client = new HttpClient())
                //    {
                //        //var headers = HttpContext.Request.Form[1]; ghe baalohpo

                //        client.BaseAddress = new Uri("http://localhost:59870/api/");
                //        MediaTypeWithQualityHeaderValue contentType = new MediaTypeWithQualityHeaderValue("application/json");
                //        client.DefaultRequestHeaders.Accept.Add(contentType);
                //        //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + headers);
                //        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",headers);
                //        string jsonResponse = JsonConvert.SerializeObject(compressionModel);
                //        HttpResponseMessage response = client.PostAsync("CompressionTool/CompressImage", new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")).Result;
                //        string stringData = response.Content.ReadAsStringAsync().Result;

                //        compressionModel = JsonConvert.DeserializeObject<CompressionModel>(stringData);

                //        if (compressionModel.Flag == "Y")
                //        {
                //            // Count = i++;
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{

                //}


                #endregion

                string ImagePath = Server.MapPath("~/RegistrationImages/PhotoEbook/" + OrderNo + "/");
                DirectoryInfo di = new DirectoryInfo(ImagePath);
                List<string> filePaths = new List<string>();/* Directory.GetFiles("~/RegistrationImages/Temp/" + objCompression.OrderNo + "/", "*.jpg").ToList();*/
                foreach (FileInfo fi in di.GetFiles())
                {
                    string imagepath = "\\RegistrationImages\\PhotoEbook\\" + OrderNo + "\\" + fi.Name;
                    pathlist.Add(imagepath);
                    Log.Info("HomeController-UploadImage-Adding Images To PathList-Completed");
                }

            }
            catch (Exception ex)
            {
                #region Delete From Temp Folder
                Log.Info("HomeController-ImageUpload-Delete From Temp Folder");
                // Delete all files from the Directory
                string path1 = Server.MapPath("~/RegistrationImages/Temp/" + OrderNo + "/");
                if (Directory.Exists(path1))
                {
                    foreach (string filename in Directory.GetFiles(path1))
                    {
                        System.IO.File.Delete(filename);
                    }
                    Directory.Delete(path1);
                }
                Log.Info("HomeController-ImageUpload-End Delete From Temp Folder");
                #endregion

                #region Delete From PhotoEbook Folder
                Log.Info("HomeController-ImageUpload-Delete From Temp Folder");
                // Delete all files from the Directory
                string path2 = Server.MapPath("~/RegistrationImages/PhotoEbook/" + OrderNo + "/");
                if (Directory.Exists(path1))
                {
                    foreach (string filename in Directory.GetFiles(path1))
                    {
                        System.IO.File.Delete(filename);
                    }
                    Directory.Delete(path1);
                }
                Log.Info("HomeController-ImageUpload-End Delete From Temp Folder");
                #endregion
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Exception = ex.Message;
                return Json(new { msg="UploadError"});
            }
            Log.Info("HomeController-UploadImage- PathList Returned Successfully");
            return Json(pathlist);
        }

        #endregion

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult DeleteImage(string path, string OrderNo)
        {
            int result = 0;
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

                bool order=HasOrderNo(OrderNo);
                if (order==true)
                {
                    #region Database path

                    string ImagePath = path;
                    string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {

                        objSqlConnection.Open();


                        SqlCommand cmd2 = new SqlCommand("Delete_PhotobookPhotos", objSqlConnection);
                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        DataTable dtable2 = new DataTable();
                        cmd2.CommandType = CommandType.StoredProcedure;


                        cmd2.Parameters.AddWithValue("@ImagePath", ImagePath);
                        cmd2.Parameters.AddWithValue("@OrderNo", OrderNo);


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


            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                result = 0;
            }

            return Json(result);
        }

        public bool HasOrderNo(string OrderNo)
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
                    SqlCommand objSqlCommand = new SqlCommand("checkingorderno", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;


                    objSqlCommand.Parameters.AddWithValue("@OrderNo", OrderNo);



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
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
                status = false;
            }
            return status;
        }
        public ActionResult GetEmailPhotographer(string PhotographerName)
        {

            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "GetEmailPhotographer";
                        cmd.Parameters.AddWithValue("@PhotographerName", PhotographerName);

                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {


                                msg = Convert.ToString(sdr["Email"]);


                            }
                        }
                        con.Close();
                    }
                    if (msg == "")
                    {
                        return Json(new { status = "Invalid", email = msg });
                    }



                }

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                msg = "Server Timeout";
                return Json(new { status = "Error", email = msg });

            }
            return Json(new { status = "Success", email = msg });

        }

        public ActionResult GetEmailStudio(string StudioName)
        {

            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "GetEmailStudio";
                        cmd.Parameters.AddWithValue("@StudioName", StudioName);

                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {


                                msg = Convert.ToString(sdr["Email"]);


                            }
                        }
                        con.Close();
                    }
                    if (msg == "")
                    {
                        return Json(new { status = "Invalid", email = msg });
                    }

                }

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                msg = "Server Timeout";
                return Json(new { status = "Error", email = msg });

            }
            return Json(new { status = "Success", email = msg });

        }

       


        #region AutoComplete
        [HttpPost]
        public JsonResult GetPhotographers(string Prefix)
        {

            List<PhotographerAuto> rate = new List<PhotographerAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            if (Prefix != null)
            {
                try
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {
                        objSqlConnection.Open();
                        SqlCommand objSqlCommand = new SqlCommand("GetPhotographerORStudio", objSqlConnection);
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

                                rate.Add(new PhotographerAuto
                                {

                                    PhotographerID = Convert.ToInt32(rd["PhotographerID"]),
                                    PhotographerName = Convert.ToString(rd["Name"]),

                                });


                            }

                        }
                        objSqlConnection.Close();


                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    string Message = ex.Message;
                }

                var StudioList = (from N in rate
                                  where N.PhotographerName.ToUpper().StartsWith(Prefix.ToUpper())
                                  select new { N });


                return Json(StudioList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetStudio(string Prefix)
        {

            List<StudioAuto> rate = new List<StudioAuto>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            if (Prefix != null)
            {
                try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetPhotographerORStudio", objSqlConnection);
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

                            rate.Add(new StudioAuto
                            {

                                PhotographerID = Convert.ToInt32(rd["PhotographerID"]),
                                StudioName = Convert.ToString(rd["StudioName"]),

                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                    string error = Utility.Utility.LogErrorS(ex);
                    Log.Error(error);
                    string Message = ex.Message;
            }
            var StudioList = (from N in rate
                            where N.StudioName.ToUpper().StartsWith(Prefix.ToUpper())
                            select new { N });
            return Json(StudioList, JsonRequestBehavior.AllowGet);
        }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
    }

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
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
                }
                
                return Json(rate, JsonRequestBehavior.AllowGet);
          

        }

        

        public string IsSelected(DataTable dt, int ID)
        {
            string status = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (Convert.ToInt32(dt.Rows[i]["PhotographyCategoriesID"]) == ID)
                        {
                            status = "selected";
                        }
                    }
                }
            }


            return status;
        }

        public string IsSelectedSize(DataTable dt, int ID)
        {
            string status = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (Convert.ToInt32(dt.Rows[i]["OrientationID"]) == ID)
                        {
                            status = "selected";
                        }
                    }
                }
            }


            return status;
        }

        public List<SelectModel> Getcategory(DataTable dt)
        {

            List<SelectModel> list  = new List<SelectModel>();
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

                            list.Add(new SelectModel
                            {

                                Value = Convert.ToString(rd["PhotographyCategoriesID"]),
                                Text = Convert.ToString(rd["Name"]),
                                Selected=IsSelected(dt, Convert.ToInt32(rd["PhotographyCategoriesID"]))
                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }
            return list;
        }

        public List<SizeModel> GetSize(DataTable dt)
        {

            List<SizeModel> list = new List<SizeModel>();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetPhotographyBookSize", objSqlConnection);
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

                            list.Add(new SizeModel
                            {

                                Value = Convert.ToString(rd["OrientationID"]),
                                Text = Convert.ToString(rd["TypeOfOrientation"]),
                                Selected = IsSelectedSize(dt, Convert.ToInt32(rd["OrientationID"]))
                            });


                        }

                    }
                    objSqlConnection.Close();


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                string Message = ex.Message;
            }
            return list;
        }

        #endregion AutoComplete

        #endregion

        #region View Photo E-Book

        public ActionResult ViewEBook(string PinCode)
            {
            if (string.IsNullOrEmpty(PinCode))
            {
                return HttpNotFound();
            }
            PhotoDetailsModel images = GetImages(PinCode);


            // Add +1 to no of View(Update)
            images.NoofViews=UpdateNoofReviews(PinCode);
            //Add +1 to no of View(Update)
            

            return View(images);
        }

        public int UpdateNoofReviews(string id)
        {
            Utility.Utility u = new Utility.Utility();
            int noofviews = 0;

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
                         noofviews=GetNoofViewsOnPincode(id);
                        int count = noofviews + 1;
                        using (SqlCommand cmd = new SqlCommand("Update Tbl_SecurityCode set NoofViews='" + count + "' WHERE SecurityCode='" + id + "' ", con))
                        {

                            cmd.ExecuteNonQuery();
                            noofviews = noofviews + 1;

                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = Utility.Utility.LogErrorS(ex); Log.Error(error);
                    noofviews = 0;
                    string Exception = ex.Message;
                }
            }

            return noofviews;

        }

        private PhotoDetailsModel GetImages(string SecurityCode)
        {
            Utility.Utility u = new Utility.Utility();
            List<FileDetailsModel> images = new List<FileDetailsModel>();
            PhotoDetailsModel objPhotoDetailsModel = new PhotoDetailsModel();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ViewPhotoEbook";
                    cmd.Parameters.AddWithValue("@SecurityCode", SecurityCode);
                    cmd.Parameters.AddWithValue("@Part", "Pages");
                    
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            images.Add(new FileDetailsModel
                            {
                                Id = Convert.ToInt32(sdr["PhotoID"]),
                                //FileData = (byte[])sdr["PhotoFileBytes"],
                               FileName = Convert.ToString(sdr["ImagePath"]).Replace("\\", "/")
                            

                            });
                            objPhotoDetailsModel.front = GetFront(SecurityCode);
                            objPhotoDetailsModel.back = GetBack(SecurityCode);
                            objPhotoDetailsModel.photolist = images;
                            objPhotoDetailsModel.TypeofPhotobook = Convert.ToInt32(sdr["TypeofPhotobook"]);
                            objPhotoDetailsModel.PinCode = Convert.ToString(sdr["SecurityCode"]);
                            objPhotoDetailsModel.PhotographerName = Convert.ToString(sdr["Name"]);
                            objPhotoDetailsModel.Title = Convert.ToString(sdr["Title"]);
                            objPhotoDetailsModel.PhotographerID = Convert.ToInt32(sdr["PhotographerID"]);
                            objPhotoDetailsModel.StudioName = Convert.ToString(sdr["StudioName"]);
                            objPhotoDetailsModel.EncodedPhotographerID = u.Encode(Convert.ToString(objPhotoDetailsModel.PhotographerID));
                            int SizeOrientation=Convert.ToInt32(sdr["SizeID"]);
                
                            if (SizeOrientation == 1)
                            {
                                objPhotoDetailsModel.page_height = 390;
                                objPhotoDetailsModel.page_width = 520;
                            }
                            if (SizeOrientation == 2)
                            {
                                objPhotoDetailsModel.page_height = 520;
                                objPhotoDetailsModel.page_width = 650;
                            }
                            if (SizeOrientation == 3)
                            {
                                objPhotoDetailsModel.page_height = 520;
                                objPhotoDetailsModel.page_width = 520;
                            }
                            if (SizeOrientation == 4)
                            {
                                objPhotoDetailsModel.page_height = 520;
                                objPhotoDetailsModel.page_width = 650;
                            }
                            if (SizeOrientation == 5)
                            {
                                objPhotoDetailsModel.page_height = 520;
                                objPhotoDetailsModel.page_width = 780;
                            }

                        }
                    }
                    con.Close();
                }

                return objPhotoDetailsModel;
            }
        }

        private FileDetailsModel GetFront(string SecurityCode)
        {

           
            FileDetailsModel objPhotoDetailsModel = new FileDetailsModel();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ViewPhotoEbook";
                    cmd.Parameters.AddWithValue("@SecurityCode", SecurityCode);
                    cmd.Parameters.AddWithValue("@Part", "Front");
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {

                            objPhotoDetailsModel.Id = Convert.ToInt32(sdr["PhotoID"]);
                            //objPhotoDetailsModel.FileData = (byte[])sdr["PhotoFileBytes"];
                            objPhotoDetailsModel.FileName = Convert.ToString(sdr["ImagePath"]).Replace("\\","/");
                           // "/RegistrationImages/PhotoEbook/RGBDEMO4/Front.jpg"
                        }
                    }
                    con.Close();
                }

                return objPhotoDetailsModel;
            }
        }

        private FileDetailsModel GetBack(string SecurityCode)
        {

            
            FileDetailsModel objPhotoDetailsModel = new FileDetailsModel();
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ViewPhotoEbook";
                    cmd.Parameters.AddWithValue("@SecurityCode", SecurityCode);
                    cmd.Parameters.AddWithValue("@Part", "Back");
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            objPhotoDetailsModel.Id = Convert.ToInt32(sdr["PhotoID"]);
                            //objPhotoDetailsModel.FileData = (byte[])sdr["PhotoFileBytes"];
                            objPhotoDetailsModel.FileName = Convert.ToString(sdr["ImagePath"]).Replace("\\", "/");

                        }
                    }
                    con.Close();
                }

                return objPhotoDetailsModel;
            }
        }

        #region Send EMail

        public ActionResult SendMail(PhotoDetailsModel objEmail)
        {

            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            try
            {
                string From = ConfigurationManager.AppSettings["EmailFrom"];
                //string Subject = "PIXTHON Photobook Security Code";
               
                msg = SendMailForSecuritycode(objEmail.Title, objEmail.PinCode, objEmail.To, From, objEmail.Subject);
                //#region For Sending Email"
                //objEmail.From = ConfigurationManager.AppSettings["EmailFrom"]; 
                ////string HtmlBody= "<div id= " + "\"" + "email" + "\"" + " style = " + "\""+"border:1px solid #fff;padding:20px;background: #d8e7ef;border-radius:5px;"+"\""+ ">Hello,<br/>I would like to share my Photobook<b> "+ objEmail.Title+"</b> with you. To view this book, please download the Pixthon android or iOS app from http://www.pixthon.com and enter the PIN mentioned below.";
                ////                   HtmlBody += "<b>Photobook PIN : " + objEmail.PinCode + " </b></div>";
                //string HtmlBody = "Hello, <br/> I would like to share my Photobook<b> " + objEmail.Title + "</b> with you. To view this book, please download the Pixthon android or iOS app from http://www.pixthon.com and enter the PIN mentioned below.<br/>";
                //HtmlBody += "<b>Photobook PIN : " + /*objEmail*/.PinCode + " </b>";
                ////HtmlDocument doc = new HtmlDocument();
                ////doc.LoadHtml(HtmlBody);
                ////string lookfor = doc.GetElementbyId("email").InnerText;
                //objEmail.Body = HtmlBody;
                //msg = utility.SendEmail(objEmail.To, objEmail.From, objEmail.Body, objEmail.Subject);

                //#endregion
                
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                msg = "Server Timeout";
            }
            return Json(new { message = msg });

        }

        public string SendMailForSecuritycode(string Title,string PinCode,string To, string From, string Subject)
        {
            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            try
            {
                #region For Sending Email"
                
              // From = ConfigurationManager.AppSettings["EmailFrom"];
                //string HtmlBody= "<div id= " + "\"" + "email" + "\"" + " style = " + "\""+"border:1px solid #fff;padding:20px;background: #d8e7ef;border-radius:5px;"+"\""+ ">Hello,<br/>I would like to share my Photobook<b> "+ objEmail.Title+"</b> with you. To view this book, please download the Pixthon android or iOS app from http://www.pixthon.com and enter the PIN mentioned below.";
                //                   HtmlBody += "<b>Photobook PIN : " + objEmail.PinCode + " </b></div>";
                string HtmlBody = "Hello, <br/> I would like to share my Photobook<b> " + Title + "</b> with you. To view this E-Album, please go the Pixthon website from http://profile.pixthon.com/PhotographerProfile and enter the PIN mentioned below.<br/>";
                HtmlBody += "<b>Photobook PIN : " + PinCode + " </b>";
                //HtmlDocument doc = new HtmlDocument();
                //doc.LoadHtml(HtmlBody);
                //string lookfor = doc.GetElementbyId("email").InnerText;
               // objEmail.Body = HtmlBody;
                msg = utility.SendEmail(To, From, HtmlBody, Subject);

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                msg = "Server Timeout";
            }
            return msg;
        }

        #endregion
        #endregion View Photo E-Book

       

        public ActionResult PixthonContactUs(PhotographerProfileModel objEmail)
        {
            return View();

        }

        public ActionResult PixthonSubmit(PhotographerProfileModel objEmail)
        {
            PhotoGrapherDetail obj = new PhotoGrapherDetail();
            Utility.Utility utility = new Utility.Utility();
            string msg = "";
            string PixthonEmailmsg = "";
            string CustomerEmailmsg = "";
            //string PhotographerEmailmsg = "";
            //string Smsmsg = "";
            // string service = GetService(objEmail.EmailDetailsModel.Category);
            try
            {
                // obj = GetPhotographerDetails("", objEmail.EmailDetailsModel.PhotographerID);
                string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];

                #region For Sending Email to Pixthon
                string PixthonMail = ConfigurationManager.AppSettings["EnquiryPixthon"];
                string HtmlBody = "Hello, <br/> Here are the details of your new query<br/><br/>";
                HtmlBody += "<b>Name :</b> " + objEmail.EmailDetailsModel.CustName + "<br/> ";
                HtmlBody += "<b>Email ID :</b> " + objEmail.EmailDetailsModel.CustEmail + " <br/>";
                HtmlBody += "<b>Services :</b> " + objEmail.EmailDetailsModel.Category + " <br/>";
                HtmlBody += "<b>Phone No : </b>" + objEmail.EmailDetailsModel.CustPhone + "<br/>";
                HtmlBody += "<b>Message : </b>" + objEmail.EmailDetailsModel.CustMsg + " <br/>";

                //string mailbody = obj.StudioName + " has received an inquiry from " + objEmail.EmailDetailsModel.CustName + "<br/> Customer name: " + objEmail.EmailDetailsModel.CustName + "<br/> Customer Email: " + objEmail.EmailDetailsModel.CustEmail + "<br/> Customer Contact No:" + objEmail.EmailDetailsModel.CustPhone + "<br/> Enquiry Message: " + objEmail.EmailDetailsModel.CustMsg + "<br/> Thanks";
                string Subject = "You have a new query";
                PixthonEmailmsg = utility.SendEmail(PixthonMail, EmailFrom, HtmlBody, Subject);
                #endregion

                #region For Sending Email to Customer
                string CustomerMail = objEmail.EmailDetailsModel.CustEmail;
                //string mailbodyCustomer = "Thank you for enquiring with " + obj.StudioName + ". We will get in touch with you shortly.<br/>Thanks for choosing us<br/><br/>Thanks,<br/>" + obj.PhotographerName + "<br/>" + obj.StudioName;
                string mailbodyCustomer = "Hi " + objEmail.EmailDetailsModel.CustEmail + ",<br/>";
                mailbodyCustomer += "Thank you for contacting Pixthon.<br/>";
                mailbodyCustomer += "We have received your email and will get back to you with a response as soon as possible.<br/>";
                mailbodyCustomer += "<br/>If you have general questions you can check out our FAQs.<br/>";
                mailbodyCustomer += "If you have any additional information that you think will help us to assist you, please feel free to reply to this email.<br/>";
                mailbodyCustomer += "<br/>Cheers,<br/>";
                mailbodyCustomer += "Pixthon Digital Solutions Pvt Ltd<br/>";

                string SubjectCustomer = "Thank you for contacting us";
                CustomerEmailmsg = utility.SendEmail(CustomerMail, EmailFrom, mailbodyCustomer, SubjectCustomer);
                #endregion

                #region For Sending SMS
                //Smsmsg = utility.SendSMS(objEmail.EmailDetailsModel.Receipientno, objEmail.EmailDetailsModel.Msgtxt);
                #endregion

                if (PixthonEmailmsg == "Email Sent Successfully" && CustomerEmailmsg == "Email Sent Successfully")
                {
                    msg = "Email Sent Successfully";
                }

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);
                Log.Error(error);
                msg = "sent UNSuccessfully";
            }
            return Json(new { Message = msg });

        }

    }


}
