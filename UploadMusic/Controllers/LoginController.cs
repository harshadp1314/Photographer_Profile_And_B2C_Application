using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UploadMusic.Models;
using System.Net;
using System.Net.Mail;
using System.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Web.Security;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class LoginController : Controller
    {
        
        #region Login
        //View Login Page
        public ActionResult Index()
        {
            return View();
        }

        //On Login Submit
        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public ActionResult GetLogin(LoginModel obj)
        {
           // bool status = false;
            Utility.Utility u = new Utility.Utility();
            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Checking for Valid user

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetLogin", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;


                    objSqlCommand.Parameters.AddWithValue("@Email", obj.Email);
                   // objSqlCommand.Parameters.AddWithValue("@Password", Crypto.Hash(obj.Password));
                    objSqlCommand.Parameters.AddWithValue("@Password", (obj.Password));


                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    objSqlConnection.Close();

                    adapt.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        if ( Convert.ToInt32(dt.Rows[0]["IsEmailVerified"]) != 0 )
                        {
                            //If valid Photographer or Admin
                            string tokenString = GenerateJSONWebToken();

                            bool isAdmin = false;
                            if (Convert.ToInt32(dt.Rows[0]["RoleID"]) == 1 || Convert.ToInt32(dt.Rows[0]["RoleID"]) == 1003)
                            {
                                isAdmin = true;
                            }
                            if (isAdmin == false)
                            {
                                //Photographer
                                int PhotographerID = Convert.ToInt32(dt.Rows[0]["PhotographerID"]);
                                bool port = checkingportfolio(obj.Email);
                                if (port == true)
                                {
                                    //Portfolio Filled
                                    FormsAuthentication.SetAuthCookie(obj.Email, false);
                                    return Json(new { email = obj.Email, result = "Redirect", url = Url.Action("PhotographerProfile", "PhotographerProfile", new { PhotographerID = u.Encode(Convert.ToString(PhotographerID)) }), Token = tokenString });
                                }
                                else
                                {
                                    //Portfolio Not Filled
                                    FormsAuthentication.SetAuthCookie(obj.Email, false);
                                    return Json(new { email = obj.Email, result = "Redirect", url = Url.Action("Registration", "Registration", new { id = u.Encode(Convert.ToString(PhotographerID)) }), Token = tokenString });
                                }

                            }
                            else
                            {
                                //Admin
                                FormsAuthentication.SetAuthCookie(obj.Email, false);
                                return Json(new { email = obj.Email, result = "Redirect", url = Url.Action("AdminPage", "Admin"), Token = tokenString });
                            }
                        }
                        else
                        {
                            return Json(new { result = "EmailNotVerified", msg = "Invalid Email Or Password" });
                            //ViewBag.Message = "Please verify your email first";
                        }
                    }
                    else
                    {
                        return Json(new { result = "Invalid", msg = "Invalid Email Or Password" });
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            return Json(new { email = obj.Email, result = "Redirect", url = Url.Action("Registration", "Registration", new { id = u.Encode(Convert.ToString(0)) }) });
            // return status;
        }

        public bool checkingportfolio(string email)
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
                    SqlCommand objSqlCommand = new SqlCommand("checkingportfolio", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;


                    objSqlCommand.Parameters.AddWithValue("@Email", email);



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
        #endregion Login

        #region Registration

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult InsertLogin(LoginModel obj)
        {
            
            SqlTransaction tran = null;
            //Check if email exists
            if (IsAlreadySignedUp(obj.Email.Trim()) != false)
            {
                return Json(new { result = "Already a Member" });
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

                    objSqlCommand.Parameters.AddWithValue("@Email", obj.Email.Trim());
                    objSqlCommand.Parameters.AddWithValue("@Contact", Convert.ToString(obj.Contact));
                    objSqlCommand.Parameters.AddWithValue("@Name", obj.Name);
                    objSqlCommand.Parameters.AddWithValue("@Password", Crypto.Hash(obj.Password));
                    objSqlCommand.Parameters.AddWithValue("@ConfirmPassword", Crypto.Hash(obj.Password));
                    objSqlCommand.Parameters.AddWithValue("@IsProfileCompleted", 0);
                    

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
                    objSqlCmd.Parameters.AddWithValue("@P_RoleID", 3);

                    SqlParameter @P_SuccessFlag2 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag2.Direction = ParameterDirection.Output;
                    objSqlCmd.Parameters.Add(@P_SuccessFlag2);

                    SqlParameter @P_SuccessMessage2 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                    @P_SuccessMessage2.Direction = ParameterDirection.Output;
                    objSqlCmd.Parameters.Add(@P_SuccessMessage2);
                    int j = objSqlCmd.ExecuteNonQuery();
                    string SuccessFlag2 = Convert.ToString(objSqlCmd.Parameters["@P_SUCCESSFLAG"].Value);

                    #endregion

                    #region Insert Photographer Table

                    SqlCommand objSqlCommand1 = new SqlCommand("InsertProfile", objSqlConnection);
                    SqlDataAdapter adapt1 = new SqlDataAdapter(objSqlCommand1);
                    DataTable dt1 = new DataTable();
                    objSqlCommand1.CommandType = CommandType.StoredProcedure;
                    objSqlCommand1.Transaction = tran;

                    objSqlCommand1.Parameters.AddWithValue("@RegistrationID", RegistrationID);
                    objSqlCommand1.Parameters.AddWithValue("@Email", obj.Email);
                    objSqlCommand1.Parameters.AddWithValue("@Contact", Convert.ToString(obj.Contact));
                    objSqlCommand1.Parameters.AddWithValue("@Name", obj.Name);

                    SqlParameter @P_SuccessFlag1 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag1.Direction = ParameterDirection.Output;
                    objSqlCommand1.Parameters.Add(@P_SuccessFlag1);

                    SqlParameter @P_SuccessMessage1 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, -1);
                    @P_SuccessMessage1.Direction = ParameterDirection.Output;
                    objSqlCommand1.Parameters.Add(@P_SuccessMessage1);
                    objSqlCommand1.ExecuteNonQuery();
                    string SuccessFlag1 = Convert.ToString(objSqlCommand1.Parameters["@P_SUCCESSFLAG"].Value);
                    #endregion

                    #region Generate Activation Code 
                    var ActivationCode = Guid.NewGuid();
                    #endregion

                    //Send Email to User
                    SendVerificationLinkEmail(obj.Email.Trim(), Convert.ToString(ActivationCode));
                    string message = "Registration successfully done. Account activation link " +
                        " has been sent to your email id:" + obj.Email.Trim();

                    //update in logindetails isemailverifies 0 and activation code

                    using (SqlCommand cmd = new SqlCommand("Update logindetails SET IsEmailVerified=0 , ActivationCode='"+ ActivationCode.ToString() + "' WHERE RegistrationID='" + RegistrationID + "' ", objSqlConnection))
                    {
                        cmd.Transaction = tran;
                        tran.Save("save2");
                        cmd.ExecuteNonQuery();
                        //status = true;
                    }
                    // Status = true;
                    if (SuccessFlag == "Y" && SuccessFlag1 == "Y" && SuccessFlag2 == "Y")
                    {
                        tran.Commit();
                        return Json(new { result = "Redirect", url = Url.Action("Index", "Login") });
                    }
                    else
                    {
                        tran.Rollback();
                        return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
                    }

                }
                
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                if (tran != null)
                    tran.Rollback();
                string Message = ex.Message;
                return Json(new { result = "Invalid", msg = "Failed To register Photographer" });
            }

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

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            bool Status = false;
            int RegistrationID = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select * from logindetails  where ActivationCode ='"+ new Guid(id) + "' ", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                 RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                //string Email = Convert.ToString(rd["Email"]);
                                
                            }
                        }
                    }
                    if(RegistrationID != 0)
                    {
                        using (SqlCommand cmd = new SqlCommand("Update logindetails SET IsEmailVerified=1   WHERE RegistrationID='" + RegistrationID + "' ", con))
                        {

                            cmd.ExecuteNonQuery();
                            Status = true;
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
            //using (PixthonPhotographyEntities dc = new PixthonPhotographyEntities())
            //{
            //    //dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid 
            //                                                    // Confirm password does not match issue on save changes
            //    var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
            //    if (v != null)
            //    {
            //        v.IsEmailVerified = true;
            //        dc.SaveChanges();
            //        Status = true;
            //    }
            //    else
            //    {
            //        ViewBag.Message = "Invalid Request";
            //    }
            //}
            ViewBag.Status = Status;
            return View();
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

        #endregion Registration

        #region ForgotPassword
        #region By OTP(Not Used)
        [HttpPost]
        public ActionResult OnSubmit(string email)
        {
            string newPassword = GetPassword();

            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                //HashF(newPassword); //Getting Password In Encrypted Hash Format
                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("ForgotPassword", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;


                    objSqlCommand.Parameters.AddWithValue("@Email", email);
                    objSqlCommand.Parameters.AddWithValue("@Password", newPassword);
                    objSqlCommand.Parameters.AddWithValue("@ConfirmPassword", newPassword);


                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    objSqlConnection.Close();

                    adapt.Fill(dt);

                    string SuccessFlag = Convert.ToString(objSqlCommand.Parameters["@P_SUCCESSFLAG"].Value);


                    if (SuccessFlag == "Y")
                    {
                        try
                        {
                            string To = email;
                            string From = ConfigurationManager.AppSettings["EmailFrom"];
                            string mailbody = "Hi " + To + "," + "<br/>You recently requested to reset your password for your Pixthon account.<br/>Here is your new password:" + newPassword + "<br/><br/>Thanks,<br/>Pixthon Digital Solutions ";
                            string Subject = "Password Reset Request ";
                            Utility.Utility u = new Utility.Utility();
                            string reply = u.SendEmail(To, From, mailbody, Subject);
                            if (reply == "Email Sent Successfully")
                                return Json(new { result = "Success" });
                            else if (reply == "Sending Email Failed")
                                return Json(new { result = "Error" });
                        }
                        catch (Exception ex)
                        {
                            string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                            string Message = ex.Message;
                            return Json(new { result = "Error" });
                        }
                    }
                    else
                    {
                        return Json(new { result = "Invalid", msg = "Server TimeOut" });
                    }


                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
                return Json(new { result = "Invalid", msg = "Server TimeOut" });
            }

            return Json(new { result = "Success" });

        }
        #endregion By OTP
        #region By Reset Link
        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
         
            var verifyUrl = "";
            if (emailFor != "VerifyAccount")
            {
                verifyUrl = "/ForgotPassword/" + emailFor + "/" + activationCode;
            }
            else {
                verifyUrl = "/Login/" + emailFor + "/" + activationCode;
            }
            
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            string EmailFrom= ConfigurationManager.AppSettings["EmailFrom"].ToString();
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
                         "<br/><br/><a href=" + link + ">Reset Password link</a>"+
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
        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string action = "";
            string message = "";
            //bool status = false;
            string ConnectionString = string.Empty;
            LoginModel model = new LoginModel();
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetPhotographerOnEmail", objSqlConnection);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_Email", EmailID);//1

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
                            //model.ActivationCode = Convert.ToString(rd["ActivationCode"]);
                            model.ConfirmPassword = Convert.ToString(rd["ConfirmPassword"]);
                            model.Contact = Convert.ToString(rd["Contact"]);
                            model.Email = Convert.ToString(rd["Email"]);
                            //model.IsAdmin = Convert.ToBoolean(rd["IsAdmin"]);
                            // model.IsEmailVerified = Convert.ToBoolean(rd["IsEmailVerified"]);
                            model.Name = Convert.ToString(rd["Name"]);
                            model.Password = Convert.ToString(rd["Password"]);
                            model.ResetPasswordCode = Convert.ToString(rd["ResetPasswordCode"]);
                            model.RegistrationID = Convert.ToInt32(rd["RegistrationID"]);

                        }

                    }

                    if (model != null)
                    {
                        //Send email for reset password
                        string resetCode = Guid.NewGuid().ToString();
                        SendVerificationLinkEmail(model.Email, resetCode, "ResetPassword");
                        model.ResetPasswordCode = resetCode;
                        //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                        //in our model class in part 1
                        SqlCommand objSqlCommand1 = new SqlCommand("InsertResetPasswordCode", objSqlConnection);
                        objSqlCommand1.CommandType = CommandType.StoredProcedure;

                        objSqlCommand1.Parameters.AddWithValue("@P_Email", EmailID);
                        objSqlCommand1.Parameters.AddWithValue("@ResetPasswordCode", model.ResetPasswordCode);

                        SqlParameter @P_SuccessFlag1 = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessFlag1);

                        SqlParameter @P_SuccessMessage1 = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage1.Direction = ParameterDirection.Output;
                        objSqlCommand1.Parameters.Add(@P_SuccessMessage1);
                        objSqlCommand1.ExecuteNonQuery();
                        //adapt.Fill(dt);
                        //dc.Configuration.ValidateOnSaveEnabled = false;
                        //dc.SaveChanges();
                        action = "Success";
                        message = "Reset password link has been sent to your email id.";
                    }
                    else
                    {
                        action = "Error";
                        message = "Account not found";
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
                action = "Invalid";
                message = "Server Timeout";

            }

            ViewBag.Message = message;
            return Json(new { msg = message, result = action });
        }
        #endregion By Reset Link
        #endregion ForgotPassword

        #region Logout
        public ActionResult Logout()
        {
            Request.Cookies.Remove("username");
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
        #endregion Logout

        #region Generate Token For Form Security(Currently Not used)

        private string GenerateJSONWebToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RoshHarsh@1314 Jaan Hai Tu Meri"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //var claims = new List<Claim>
            //{
            //    new Claim("Username", userLogin.Username),
            //    new Claim("Password", userLogin.Password)
            //};

            var token = new JwtSecurityToken(
                issuer: "https://www.pixthonprofile.com",
                audience: "https://www.pixthonprofile.com",
                notBefore: null,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
                //claims: claims
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion Generate Token For Form Security
    }
}