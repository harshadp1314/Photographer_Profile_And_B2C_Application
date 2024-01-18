using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using UploadMusic.Models;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class ChangePasswordController : Controller
    {
        //byte[] HashOldPassBytes, HashNewPassBytes;
        string OldPasswordHash = string.Empty;
        string NewPasswordHash = string.Empty;

        // GET: ChangePassword
        [Authorize(Roles = "Admin,VerifiedPhotographer,SuperAdmin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,VerifiedPhotographer,SuperAdmin")]
        [Route("ChangePasswordRoute")]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel changePassword)
        {
            try

            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

               // Hash(changePassword.OldPassword, changePassword.NewPassword); //Converting Passwords To Encrypted Hash Bytes

                #region Passing Parameters To Stored Procedure

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    SqlCommand objSqlCommand = new SqlCommand("ChangePassword", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@P_Email", changePassword.Email);
                    objSqlCommand.Parameters.AddWithValue("@P_OldPassword", Crypto.Hash(changePassword.OldPassword));
                    objSqlCommand.Parameters.AddWithValue("@P_NewPassword", Crypto.Hash(changePassword.NewPassword));

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_FLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_MESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);
                    objSqlConnection.Close();
                    adapt.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        changePassword.Flag = Convert.ToString(@P_SuccessFlag.Value);
                       
                    }
                    else
                    {
                        changePassword.Flag = Convert.ToString(@P_SuccessFlag.Value);
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
                return Json(new { changePassword ="Error" });
            }
            return Json(new { changePassword= changePassword.Flag });


        }

        #region Converting Old And New Password To HashBytes

        //public void Hash(string OldPassword,string NewPassword)
        //{
        //    var OldPassBytes = new UTF8Encoding().GetBytes(OldPassword);
        //    var NewPassBytes = new UTF8Encoding().GetBytes(NewPassword);

        //    using (var algorithm = new System.Security.Cryptography.SHA512Managed())
        //    {
        //        HashOldPassBytes = algorithm.ComputeHash(OldPassBytes);
        //        OldPasswordHash = Convert.ToBase64String(HashOldPassBytes);

        //        HashNewPassBytes = algorithm.ComputeHash(NewPassBytes);
        //        NewPasswordHash = Convert.ToBase64String(HashNewPassBytes);
        //    }
        //}

        

        #endregion
    }
}