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
    public class ForgotPasswordController : Controller
    {
        #region Not uSed
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/ForgotPassword/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("roshni.dahake13@gmail.com", "Pixthon");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "RoshHarsh@1314"; // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your Dotnet Awesome account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/>br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
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
                            model.ActivationCode = Convert.ToString(rd["ActivationCode"]);
                            model.ConfirmPassword = Convert.ToString(rd["ConfirmPassword"]);
                            model.Contact = Convert.ToString(rd["Contact"]);
                            model.Email = Convert.ToString(rd["Email"]);
                            model.IsAdmin = Convert.ToBoolean(rd["IsAdmin"]);
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
                        message = "Reset password link has been sent to your email id.";
                    }
                    else
                    {
                        message = "Account not found";
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            
            ViewBag.Message = message;
            return View("Index","Login");
        }
        #endregion 

        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            string ConnectionString = string.Empty;
            LoginModel model = new LoginModel();
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("GetPhotographerOnResetPasswordCode", objSqlConnection);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@P_ResetPasswordCode",id);//1

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
                            model.ActivationCode = Convert.ToString(rd["ActivationCode"]);
                            model.ConfirmPassword = Convert.ToString(rd["ConfirmPassword"]);
                            model.Contact = Convert.ToString(rd["Contact"]);
                            model.Email = Convert.ToString(rd["Email"]);
                            model.IsAdmin = Convert.ToBoolean(rd["IsAdmin"]);
                            //model.IsEmailVerified = Convert.ToBoolean(rd["IsEmailVerified"]);
                            model.Name = Convert.ToString(rd["Name"]);
                            model.Password = Convert.ToString(rd["Password"]);
                            model.ResetPasswordCode = Convert.ToString(rd["ResetPasswordCode"]);
                            model.RegistrationID = Convert.ToInt32(rd["RegistrationID"]);



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
            if (model != null)
            {
                ResetPasswordModel model1 = new ResetPasswordModel();
                model1.ResetCode = id;
                return View(model1);
            }
            else
            {

                return HttpNotFound();
            }
          
        }

       

        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model1)
        {
            var message = "";
            //string Url = "";
            //string action = "";
            string ConnectionString = string.Empty;
            LoginModel obj = new LoginModel();
            if (ModelState.IsValid)
            {
                
                LoginModel model = new LoginModel();
                try
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {
                        objSqlConnection.Open();
                        SqlCommand objSqlCommand = new SqlCommand("GetPhotographerOnResetPasswordCode", objSqlConnection);
                        objSqlCommand.CommandType = CommandType.StoredProcedure;

                        objSqlCommand.Parameters.AddWithValue("@P_ResetPasswordCode", model1.ResetCode);//1

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
                               // model.ActivationCode = Convert.ToString(rd["ActivationCode"]);
                                model.ConfirmPassword = Convert.ToString(rd["ConfirmPassword"]);
                                model.Contact = Convert.ToString(rd["Contact"]);
                                model.Email = Convert.ToString(rd["Email"]);
                                //model.IsAdmin = Convert.ToBoolean(rd["IsAdmin"]);
                                //model.IsEmailVerified = Convert.ToBoolean(rd["IsEmailVerified"]);
                                model.Name = Convert.ToString(rd["Name"]);
                                model.Password = Convert.ToString(rd["Password"]);
                                model.ResetPasswordCode = Convert.ToString(rd["ResetPasswordCode"]);
                                model.RegistrationID = Convert.ToInt32(rd["RegistrationID"]);



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
                    return Json(new { result = "Error" });
                }
                if (model != null)
                {
                    try
                    {
                        ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                        #region Passing Parameters To Stored Procedure

                        using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                        {
                            objSqlConnection.Open();
                            SqlCommand objSqlCommand = new SqlCommand("UpdatePasswordForgot", objSqlConnection);
                            SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                            DataTable dt = new DataTable();
                            objSqlCommand.CommandType = CommandType.StoredProcedure;


                            objSqlCommand.Parameters.AddWithValue("@Password", Crypto.Hash(model1.NewPassword));
                            objSqlCommand.Parameters.AddWithValue("@P_ResetPasswordCode", model1.ResetCode);




                            SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                            @P_SuccessFlag.Direction = ParameterDirection.Output;
                            objSqlCommand.Parameters.Add(@P_SuccessFlag);

                            SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                            @P_SuccessMessage.Direction = ParameterDirection.Output;
                            objSqlCommand.Parameters.Add(@P_SuccessMessage);
                            objSqlConnection.Close();

                            adapt.Fill(dt);
                            //objSqlConnection.Open();
                            //SqlCommand objSqlCommand = new SqlCommand("UpdatePasswordForgot", objSqlConnection);
                            //objSqlCommand.CommandType = CommandType.StoredProcedure;

                            //objSqlCommand.Parameters.AddWithValue("@Password", Crypto.Hash(model1.NewPassword));//1
                            //objSqlCommand.Parameters.AddWithValue("@P_ResetPasswordCode", "");//1

                            //SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                            //@P_SuccessFlag.Direction = ParameterDirection.Output;
                            //objSqlCommand.Parameters.Add(@P_SuccessFlag);

                            //SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                            //@P_SuccessMessage.Direction = ParameterDirection.Output;
                            //objSqlCommand.Parameters.Add(@P_SuccessMessage);
                            //objSqlCommand.ExecuteNonQuery();
                            //adapt.Fill(dt);


                            //objSqlConnection.Close();

                            message = "New password updated successfully";
                            return Json(new { result = "Redirect", url = Url.Action("Index", "Login") });

                        }

                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                        string Message = ex.Message;
                        return Json(new { result = "Error" });
                    }
                  
                }
            }
            else
            {
                
                message = "Something invalid";
                return Json(new { result = "Error" });
            }
            ViewBag.Message = message;
            return Json(new { result = "Redirect", url = Url.Action("Index", "Login") });
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateHeaderAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            var httpContext = filterContext.HttpContext;
            var cookie = httpContext.Request.Cookies[AntiForgeryConfig.CookieName];
            AntiForgery.Validate(cookie != null ? cookie.Value : null, httpContext.Request.Headers["__RequestVerificationToken"]);
        }
    }
}