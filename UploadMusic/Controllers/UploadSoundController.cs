using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UploadMusic.Models;

namespace UploadMusic.Controllers
{
    public class UploadSoundController : Controller
    {
        // GET: UploadSound
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult AudioBytes()
        {
            List<AudioFile> audiolist = new List<AudioFile>();
            string CS = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
           // byte[] audioBytes;
            SqlConnection con = new SqlConnection(CS);


            SqlCommand cmd = new SqlCommand("spGetAllAudioFile", con);
            cmd.CommandType = CommandType.StoredProcedure;
            con.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            AudioFile audio = new AudioFile();
            while (rdr.Read())
            {

                audio.ID = Convert.ToInt32(rdr["ID"]);
                audio.Name = rdr["Name"].ToString();
                audio.FileSize = Convert.ToInt32(rdr["FileSize"]);
                audio.FileBytes = (byte[])(rdr["FileBytes"]);
                //byte[] FileBytes = (byte[])(rdr["FileBytes"]);
                //audio.FileBytes = Convert.ToBase64String(FileBytes);
                // string base64 = Convert.ToBase64String(FileBytes);
                //audio.FileBytes = base64;
                audiolist.Add(audio);

            }

           
            return base.File(audio.FileBytes, "audio/wav");
        }

        [HttpGet]
        public ActionResult UploadAudio()
        {
            List<AudioFile> audiolist = new List<AudioFile>();
            string CS = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
           // byte[] audioBytes;
            SqlConnection con = new SqlConnection(CS);
           
               
                SqlCommand cmd = new SqlCommand("spGetAllAudioFile", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
            AudioFile audio = new AudioFile();
            while (rdr.Read())
                {
                    
                    audio.ID = Convert.ToInt32(rdr["ID"]);
                    audio.Name = rdr["Name"].ToString();
                    audio.FileSize = Convert.ToInt32(rdr["FileSize"]);
                audio.FileBytes= (byte[])(rdr["FileBytes"]);
                //byte[] FileBytes = (byte[])(rdr["FileBytes"]);
                //audio.FileBytes = Convert.ToBase64String(FileBytes);
                // string base64 = Convert.ToBase64String(FileBytes);
                //audio.FileBytes = base64;
                audiolist.Add(audio);
                    
                }

            return View(audiolist);
           // return base.File(audio.FileBytes, "audio/wav");
        }
        [HttpPost]
        public ActionResult UploadAudio(HttpPostedFileBase fileupload)
        {
            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                Stream str = fileupload.InputStream;


                    BinaryReader Br = new BinaryReader(str);
                  
                        byte[] FileDet = Br.ReadBytes((Int32)str.Length);
               
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000000;
                //fileupload.SaveAs(Server.MapPath("~/AudioFileUpload/" + fileName));

                string CS = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                using (SqlConnection con = new SqlConnection(CS))
                {
                    SqlCommand cmd = new SqlCommand("spAddNewAudioFile", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    con.Open();
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@FileSize", Size);
                    cmd.Parameters.AddWithValue("@FileBytes", FileDet);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("UploadAudio");
        }
    }
}