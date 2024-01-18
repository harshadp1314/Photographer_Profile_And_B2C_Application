using UploadMusic.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class StarRatingController : Controller
    {
        

        // GET: StarRating
        public ActionResult Index()
        {
            return View();
        }

        // GET: StarRating
        public ActionResult StarRating(string PhotographerID,string Pincode)
        {
            if (string.IsNullOrEmpty(PhotographerID) && string.IsNullOrEmpty(Pincode))
            {
                return HttpNotFound();
            }
            RatingModel rating = new RatingModel();
            rating.EncodedPhotographerID = PhotographerID;
            int PhotographerID2 = Convert.ToInt32(Utility.Utility.Decode(PhotographerID));
            rating.PhotographerID = PhotographerID2;
            if(Pincode != null)
            {
                rating.SecurityCode = Pincode;
            }
           
            return View(rating);
        }

        [HttpPost]
        public ActionResult SaveRating(RatingModel rating)
        {
            PhotoGrapherDetail objPhotoGrapherDetail = new PhotoGrapherDetail();
            Utility.Utility u = new Utility.Utility();
            string ConnectionString = string.Empty;
            string result = string.Empty;

            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                

                #region Passing Parameters To Stored Procedure
                Log.Info("StarRating-SaveRating-Passing Parameters To Stored Procedure");

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    //Check if photographer is rating itself

                    bool rateitself= CheckPhotographerRatingItself(rating.PhotographerID,rating.CommentedBy);
                    //Check if photographer is rating itself

                    //One Rating Per EmailID
                    bool ratingExists = CheckEmailIDRateExists(rating.PhotographerID, rating.CommentedBy);
                    //One Rating Per EmailID

                    if (rateitself ==false && ratingExists ==false)
                    {
                        SqlCommand objSqlCommand = new SqlCommand("Ratings_Insert", objSqlConnection);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataTable dt = new DataTable();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@P_NoOfStars", rating.NoOfStars);
                        objSqlCommand.Parameters.AddWithValue("@P_Comment", Convert.ToString(rating.Comment));
                        objSqlCommand.Parameters.AddWithValue("@P_CommentedBy", Convert.ToString(rating.CommentedBy));
                        objSqlCommand.Parameters.AddWithValue("@P_Title", Convert.ToString(rating.Title));
                        objSqlCommand.Parameters.AddWithValue("@P_Name", Convert.ToString(rating.Name));
                        objSqlCommand.Parameters.AddWithValue("@P_PhotographerID", rating.PhotographerID);//1

                        SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                        @P_SuccessFlag.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessFlag);

                        SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                        @P_SuccessMessage.Direction = ParameterDirection.Output;
                        objSqlCommand.Parameters.Add(@P_SuccessMessage);
                        objSqlConnection.Close();
                        adapt.Fill(dt);

                        objPhotoGrapherDetail.PhotographerID = rating.PhotographerID;
                        if (dt.Rows.Count > 0)
                        {

                        }
                        else
                        {
                        }
                        result = "Redirect";
                    }
                    else
                    {
                        if(rateitself ==true)
                        {
                            result = "RateItself";
                        }
                        else if(ratingExists ==true)
                        {
                            result = "RatingExists";
                        }
                        
                    }
                    Log.Info("StarRating-SaveRating- End Passing Parameters To Stored Procedure");
                }

                    //
                    
                #endregion

            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                string Message = ex.Message;
            }
            
            
            return Json(new { result= result, url=Url.Action("PhotographerProfile", "PhotographerProfile",new {obj1= (Convert.ToString(rating.SecurityCode)), PhotographerID = u.Encode(Convert.ToString(rating.PhotographerID)) })});
            
        }

        [HttpPost]
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
                    SqlCommand objSqlCommand = new SqlCommand("GetRatings", objSqlConnection);
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
                            rating.NoOfStars = Convert.ToDouble(rd["NoOfStars"]);
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
            
        }


        #region  Checking If Photographer Is Rating Himself

        public bool CheckPhotographerRatingItself(int photographerID,string CommentedBy)
        {
            bool result = true;
            string Email = string.Empty;
            string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            SqlConnection objConn = new SqlConnection(ConnectionString);
            try
            {
                objConn.Open();
                string query = "SELECT Email FROM Tbl_PhotographerProfile WHERE PhotographerID='" + photographerID + "' ";
                SqlCommand cmd = new SqlCommand(query, objConn);

                var res=cmd.ExecuteScalar();

                if(res !=null)
                {
                    Email = Convert.ToString(res);
                    if(Email != CommentedBy)
                    {
                        result = false;
                    }
                }

            }
            catch(Exception ex)
            {
                string Excetion = ex.Message;
            }
            
            return result;
        }

        #endregion

        #region  Checking If One comment per EmailID

        public bool CheckEmailIDRateExists(int photographerID, string CommentedBy)
        {
            bool result = false;
            string Email = string.Empty;
            string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            SqlConnection objConn = new SqlConnection(ConnectionString);
            try
            {
                objConn.Open();
                string query = "SELECT CommentedBY FROM Tbl_Ratings WHERE CommentedBy='" + CommentedBy + "' and PhotographerID='" + photographerID + "'";
                SqlCommand cmd = new SqlCommand(query, objConn);

                var res = cmd.ExecuteScalar();

                if (res != null)
                {
                    Email = Convert.ToString(res);
                    if (Email != CommentedBy)
                    {
                        result = true;
                    }
                }

            }
            catch (Exception ex)
            {
                string Excetion = ex.Message;
            }

            return result;
        }

        #endregion
        
    }
}