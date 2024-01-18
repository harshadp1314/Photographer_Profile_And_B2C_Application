using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using UploadMusic.Models;

namespace UploadMusic.Utility
{
    public class Utility
    {
        #region Send Password Via Email
        public string SendEmail(string To,string From,string mailbody,string Subject)
        {

            string reply = string.Empty;
            string to = To;
            string from = From;
            var fromEmail = new MailAddress(From, "Pixthon");
            var toEmail = new MailAddress(To);
            MailMessage message = new MailMessage(fromEmail, toEmail);
            string Password=ConfigurationManager.AppSettings["Password"];

            message.Subject = Subject;
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient(); //Gmail smtp 
            client.Host = "smtpout.asia.secureserver.net";//hosting.secureserver.net"smtp.gmail.com";
            client.EnableSsl = true;
            NetworkCredential basicCredential1 = new System.Net.NetworkCredential(From, Password);
            client.UseDefaultCredentials = false;
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message); //Allow Gmail Less Secure Apps For Solving Exception
                reply = "Email Sent Successfully";
            }
            catch (Exception ex)
            {
                //throw ex;
                Utility.LogError(ex);
                reply = "Sending Email Failed";
            }

            return reply;
        }
        #endregion

        #region Send SMS
        public string SendSMS(string Receipientno, string Msgtxt)

        { 
            string reply = "";
        try
            {
                // use the API URL here  
                string strUrl = "http://api.mVaayoo.com/mvaayooapi/MessageCompose?user=YourUserName:YourPassword&senderID=YourSenderID&receipientno=" + Receipientno + "&msgtxt=" + Msgtxt + "API&state=4";
                // Create a request object  
                WebRequest request = HttpWebRequest.Create(strUrl);
                // Get the response back  
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = (Stream)response.GetResponseStream();
                StreamReader readStream = new StreamReader(s);
                string dataString = readStream.ReadToEnd();
                response.Close();
                s.Close();
                readStream.Close();

                reply = "Email Sent Successfully";
            }
            catch (Exception ex)
            {
                //throw ex;
                Utility.LogError(ex);
                reply = "Sending Email Failed";
            }
            return reply;
        }
        #endregion
        
        #region Encode Decode
        public string Encode(string encodeMe)
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(encodeMe);
            return Convert.ToBase64String(encoded);
        }

        public static string Decode(string decodeMe)
        {
            byte[] encoded = Convert.FromBase64String(decodeMe);
            return System.Text.Encoding.UTF8.GetString(encoded);
        }

        #endregion

        #region Compressing Image For Photobook Or RGB
        public CompressionModel CompressImage(CompressionModel objCompression)
        {
            string Path= HttpContext.Current.Server.MapPath("~/RegistrationImages/Temp/" + objCompression.OrderNo + "/");

            DirectoryInfo di = new DirectoryInfo(Path);
            List<string> filePaths = new List<string>() ;/* Directory.GetFiles("~/RegistrationImages/Temp/" + objCompression.OrderNo + "/", "*.jpg").ToList();*/
            foreach (FileInfo fi in di.GetFiles())
            {
                string imagepath = Path + fi.Name;
                filePaths.Add(imagepath);
            }
            string Final = HttpContext.Current.Server.MapPath("~/RegistrationImages/PhotoEbook/"+objCompression.OrderNo+"/");
            //List<filePaths> pathList = new List<Bitmap>();
            try
            {
                for (int i = 0; i < filePaths.Count; i++)
                {
                    #region Resizing Image Height/Width

                    //RezizeImage(pathList[i], objCompression.CompressedImageHeight, objCompression.CompressedImageWidth);

                    #endregion

                    #region Compress Image
                    Bitmap originalImage = new Bitmap(filePaths[i]);
                    //Stream strm = originalImage.ToStr;

                    Compressimage(filePaths[i], Final, objCompression.CompressedImageHeight, objCompression.CompressedImageWidth);


                  
                    #endregion
                }

                objCompression.Flag = "Y";
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                objCompression.Flag = "F";
                string Exception = ex.Message;
            }

            return objCompression;
        }

        public static void Compressimage(string sourcePath, string targetPath,int height,int width)
        {


            try
            {
                using (var image = Image.FromFile(sourcePath))
                {
                    float maxHeight = (float)height; //900.0f;
                    float maxWidth = (float)width;//1350.0f;
                    int newWidth;
                    int newHeight;
                    string extension;
                    Bitmap originalBMP = new Bitmap(sourcePath);
                    int originalWidth = originalBMP.Width;
                    int originalHeight = originalBMP.Height;

                    if (originalWidth > maxWidth || originalHeight > maxHeight)
                    {

                        // To preserve the aspect ratio  
                        float ratioX = (float)maxWidth / (float)originalWidth;
                        float ratioY = (float)maxHeight / (float)originalHeight;
                        float ratio = Math.Min(ratioX, ratioY);
                        newWidth = (int)(originalWidth * ratio);
                        newHeight = (int)(originalHeight * ratio);
                    }
                    else
                    {
                        newWidth = (int)originalWidth;
                        newHeight = (int)originalHeight;

                    }
                    Bitmap bitMAP1 = new Bitmap(originalBMP, newWidth, newHeight);
                    Graphics imgGraph = Graphics.FromImage(bitMAP1);
                    extension = Path.GetExtension(sourcePath);
                    string fileName = Path.GetFileName(sourcePath);
                    if (extension.ToLower() == ".png" || extension.ToLower() == ".gif")
                    {
                        imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
                        imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHeight);


                        bitMAP1.Save(targetPath, image.RawFormat);

                        bitMAP1.Dispose();
                        imgGraph.Dispose();
                        originalBMP.Dispose();
                    }
                    else if (extension.ToLower() == ".jpg")
                    {

                        imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
                        imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHeight);
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        bitMAP1.Save(targetPath + fileName, jpgEncoder, myEncoderParameters);

                        bitMAP1.Dispose();
                        imgGraph.Dispose();
                        originalBMP.Dispose();

                    }


                }

            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                string Exception = ex.Message;

            }
        }

        #endregion

        #region Compressing Images For ImagePortfolio

        public static void CompressImage_Imageportfolio(string sourcePath, string targetPath, int height, int width)
        {
            try
            {
                using (var image = Image.FromFile(sourcePath))
                {
                    float maxHeight = (float)height; //900.0f;
                    float maxWidth = (float)width;//1350.0f;
                    int newWidth;
                    int newHeight;
                    string extension;
                    Bitmap originalBMP = new Bitmap(sourcePath);
                    int originalWidth = originalBMP.Width;
                    int originalHeight = originalBMP.Height;

                    if (originalWidth > maxWidth || originalHeight > maxHeight)
                    {

                        // To preserve the aspect ratio  
                        float ratioX = (float)maxWidth / (float)originalWidth;
                        float ratioY = (float)maxHeight / (float)originalHeight;
                        float ratio = Math.Min(ratioX, ratioY);
                        newWidth = (int)(originalWidth * ratio);
                        newHeight = (int)(originalHeight * ratio);
                    }
                    else
                    {
                        newWidth = (int)originalWidth;
                        newHeight = (int)originalHeight;

                    }
                    Bitmap bitMAP1 = new Bitmap(originalBMP, newWidth, newHeight);
                    Graphics imgGraph = Graphics.FromImage(bitMAP1);
                    extension = Path.GetExtension(sourcePath);
                    string fileName = Path.GetFileName(sourcePath);
                    if (extension.ToLower() == ".png" || extension.ToLower() == ".gif")
                    {
                        imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
                        imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHeight);


                        bitMAP1.Save(targetPath, image.RawFormat);

                        bitMAP1.Dispose();
                        imgGraph.Dispose();
                        originalBMP.Dispose();
                    }
                    else if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg")
                    {

                        imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
                        imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHeight);
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        bitMAP1.Save(targetPath, jpgEncoder, myEncoderParameters);

                        bitMAP1.Dispose();
                        imgGraph.Dispose();
                        originalBMP.Dispose();

                    }


                }

            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                string Exception = ex.Message;

            }
        }

        #endregion


        #region Image Compression Methods

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }


        private Image RezizeImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }

        #endregion
        #region Error Logs
        public static void LogofErrorLog(Exception ex)
        {
            try
            {
                string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", ex.Message);
                message += Environment.NewLine;
                message += string.Format("StackTrace: {0}", ex.StackTrace);
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                string path = HttpContext.Current.Server.MapPath("~/ErrorLog/ErrorLogofLogs.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
            }
            catch (Exception exp)
            {
                Utility.LogError(exp);
                string Exception = exp.Message;
            }

        }

        public static void LogError(Exception ex)
        {
            try
            {
                string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", ex.Message);
                message += Environment.NewLine;
                message += string.Format("StackTrace: {0}", ex.StackTrace);
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                string path = HttpContext.Current.Server.MapPath("~/ErrorLog/ErrorLog.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
            }
            catch (Exception exp)
            {
                Utility.LogofErrorLog(exp);
                string Exception = exp.Message;
            }
        }

        public static void LogError(string ex)
        {
            try
            {
                string message = ex;
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";

                string path = System.Web.HttpContext.Current.Server.MapPath("~/ErrorLog/ErrorLog.txt");
                using (StreamWriter writer = new StreamWriter(File.Open(path, System.IO.FileMode.Append)))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
            }
            catch (Exception exp)
            {
                Utility.LogofErrorLog(exp);
                string Exception = exp.Message;
            }
           
        }

        public static string LogErrorS(Exception ex)
        {
            string message = "";
            try
            {
                 message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", ex.Message);
                message += Environment.NewLine;
                message += string.Format("StackTrace: {0}", ex.StackTrace);
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
               
            }
            catch (Exception exp)
            {
                Utility.LogofErrorLog(exp);
                string Exception = exp.Message;
            }
           return message;
        }

        #endregion
    }
}