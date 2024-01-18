using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using UploadMusic.Models;
using System.Configuration;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class HirePhotographerController : Controller
    {
        // GET: HirePhotographer
        public ActionResult Hire(string pn)
        {
            Utility.Utility u = new Utility.Utility();
            HireModel obj = new HireModel();
            if (pn == "first")
            {
                pn = u.Encode(pn);
                obj.TypeOfPhotohraphy = Getcategory(null);
            }
            else
            {
                pn = u.Encode(pn);
            }
            string id2 = Convert.ToString(Utility.Utility.Decode(pn));


            if (id2 != "first")
            {
                try
                {
                    string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                    #region Passing Parameters To Stored Procedure

                    using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                    {
                        objSqlConnection.Open();
                        SqlCommand objSqlCommand = new SqlCommand("GetHireDetails", objSqlConnection);
                        SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                        DataSet ds = new DataSet();
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@HireID", id2);

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
                                obj.DateOfEvent = Convert.ToString(dtprofile.Rows[i]["DateOfEvent"]);
                                obj.Message = (Convert.ToString(dtprofile.Rows[i]["Message"]));
                                obj.PhotographyCategoriesID = Convert.ToInt32(dtprofile.Rows[i]["OrderNo"]);
                                obj.PlaceOfEvent = Convert.ToString(dtprofile.Rows[i]["PlaceOfEvent"]);
                                obj.TimeOfEvent = Convert.ToString(dtprofile.Rows[i]["StudioName"]);
                                obj.TypeOfEvent = Convert.ToString(dtprofile.Rows[i]["TypeOfEvent"]);
                                obj.RegistrationID = Convert.ToInt32(dtprofile.Rows[i]["RegistrationID"]);
                                obj.HireID = Convert.ToInt32(dtprofile.Rows[i]["HireID"]);
                                obj.Email = Convert.ToString(dtprofile.Rows[i]["Email"]);
                                obj.TypeOfPhotohraphy = Getcategory(dtprofile);
                               
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
                    string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                    string Message = ex.Message;
                }


            }
            else
            {

            }
            return View(obj);
        }

        public ActionResult GetPhotographerData()
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            Utility.Utility u = new Utility.Utility();
            List<PhotoGrapherDetail> photoGrapherDetails = new List<PhotoGrapherDetail>();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Select PhotographerID,Name,StudioName,PhoneNo,PhotographerAddress,Email,About,CurrentLocation,AlsoShootIn,ProductAndServices FROM Tbl_PhotographerProfile ", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                photoGrapherDetails.Add(new PhotoGrapherDetail
                                {
                                    PhotographerID = Convert.ToInt32(rd["PhotographerID"]),
                                    EncodedPhotographerID= u.Encode(Convert.ToString(rd["PhotographerID"])),
                                    PhotographerName = rd["StudioName"].ToString() + "<br/>" + rd["Name"].ToString() +"<br/>" ,
                                    PhotographerAddress = "<strong style='font-size:smaller'><u>Address</u> :</strong>" + rd["PhotographerAddress"].ToString() + "<br/><strong style='font-size:smaller'><u>Contact Details</u> :</strong>" + rd["PhoneNo"].ToString() + "<br/><strong style='font-size:smaller'><u>EmailID</u> :</strong>" + Convert.ToString(rd["Email"])
                                 
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

            return Json(new { data = photoGrapherDetails }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult Hire(HireModel obj)
        {
            int i = 1;
            //string SecurityCode = "";
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            SqlTransaction tran = null;
            HttpFileCollectionBase files = Request.Files;
            List<string> pathlist = new List<string>();
            string actiontype = "";
            
                if (obj.Parameter != "first")
                {
                actiontype = "Update";

                }
               else
               {
                actiontype = "Insert";
               }
                
            string msg1 = string.Empty;

            string ConnectionString = string.Empty;
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

                #region Inserting Event Details

                using (SqlConnection objSqlConnection = new SqlConnection(ConnectionString))
                {
                    objSqlConnection.Open();
                    tran = objSqlConnection.BeginTransaction();
                    #region Getting Registration No
                    using (SqlCommand cmd = new SqlCommand("select RegistrationID from logindetails where Email='"+ obj.Email + "'" , objSqlConnection))
                    {
                        cmd.Transaction = tran;
                        tran.Save("save1");
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                obj.RegistrationID = Convert.ToInt32(rd["RegistrationID"]);
                                
                            }
                        }
                    }
                    #endregion

                    #region Insert LoginDetails Table
                    SqlCommand objSqlCommand = new SqlCommand("InsertHire", objSqlConnection);
                    SqlDataAdapter adapt = new SqlDataAdapter(objSqlCommand);
                    DataTable dt = new DataTable();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Transaction = tran;
                    
                    objSqlCommand.Parameters.AddWithValue("@RegistrationID", obj.RegistrationID);
                    objSqlCommand.Parameters.AddWithValue("@Email", Convert.ToString(obj.Email));
                    objSqlCommand.Parameters.AddWithValue("@TypeOfEvent", obj.TypeOfEvent);
                    objSqlCommand.Parameters.AddWithValue("@PlaceOfEvent", (obj.PlaceOfEvent));
                    objSqlCommand.Parameters.AddWithValue("@DateOfEvent", (obj.DateOfEvent));
                    objSqlCommand.Parameters.AddWithValue("@TimeOfEvent", obj.TimeOfEvent);
                    objSqlCommand.Parameters.AddWithValue("@TypeOfPhotography", Convert.ToInt32(obj.PhotographyCategoriesID));
                    objSqlCommand.Parameters.AddWithValue("@Message", obj.Message);
                    objSqlCommand.Parameters.AddWithValue("@selectedphotographers",(obj.selectedphotographers));
                    


                    SqlParameter @P_RegistrationID = new SqlParameter("@P_HireID", SqlDbType.Int, 10);
                    @P_RegistrationID.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_RegistrationID);

                    SqlParameter @P_SuccessFlag = new SqlParameter("@P_SUCCESSFLAG", SqlDbType.VarChar, 10);
                    @P_SuccessFlag.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessFlag);

                    SqlParameter @P_SuccessMessage = new SqlParameter("@P_SUCCESSMESSAGE", SqlDbType.VarChar, 30);
                    @P_SuccessMessage.Direction = ParameterDirection.Output;
                    objSqlCommand.Parameters.Add(@P_SuccessMessage);

                    tran.Save("save1");
                    adapt.Fill(dt);
                    string SuccessFlag = Convert.ToString(objSqlCommand.Parameters["@P_SUCCESSFLAG"].Value);
                    int HireID = Convert.ToInt32(objSqlCommand.Parameters["@P_HireID"].Value);
                    #endregion

                    

                    if (SuccessFlag == "Y" )
                    {
                        tran.Commit();
                        return Json(new { result = "Redirect", url = Url.Action("Index", "Login") });
                    }
                    else
                    {
                        tran.Rollback();
                        return Json(new { result = "Invalid", msg = "Failed To Notify" });
                    }

                }

                #endregion

               

                #region Send Mail

                #endregion

                #region Send Notification

                #endregion

               
            }
            catch (Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                tran.Rollback();
                string Message = ex.Message;
                return Json(new { result = "Invalid", msg = "Failed To Notify" });
            }

        }

        public List<SelectModel> Getcategory(DataTable dt)
        {

            List<SelectModel> list = new List<SelectModel>();
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
                                Selected = IsSelected(dt, Convert.ToInt32(rd["PhotographyCategoriesID"]))
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
            return list;
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
    }
}