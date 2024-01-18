using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using System.Web.Mvc;
using UploadMusic.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using System.Drawing.Imaging;
using UploadMusic.Utility;

namespace UploadMusic.Controllers
{
    public class CompressionToolController : ApiController
    {
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public IHttpActionResult CompressImage(CompressionModel objCompression)
        {
            string Final = HttpContext.Current.Server.MapPath("~/RegistrationImages/PhotoEbook/");
            List<Bitmap> pathList = new List<Bitmap>();
            try
            {
                for(int i=0;i<pathList.Count;i++)
                {
                    #region Resizing Image Height/Width

                    RezizeImage(pathList[i], objCompression.CompressedImageHeight, objCompression.CompressedImageWidth);

                    #endregion

                    #region Compress Image

                    var newWidth = (int)(objCompression.CompressedImageWidth * objCompression.scaleFactor);
                    var newHeight = (int)(objCompression.CompressedImageHeight * objCompression.scaleFactor);
                    var thumbnailImg = new Bitmap(newWidth, newHeight);
                    var thumbGraph = Graphics.FromImage(thumbnailImg);
                    thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                    thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                    thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                    thumbGraph.DrawImage(pathList[i], imageRectangle);

                    #region Save Compressed Image In New Folder

                    //thumbnailImg.Save(objCompression.targetPath, originalImage.RawFormat);

                    #endregion

                    //using (var image = System.Drawing.Image.FromStream(objCompression.sourcePath))
                    //{

                    //}

                    #endregion
                }
                
                objCompression.Flag = "Y";
            }
            catch(Exception ex)
            {
                string error = Utility.Utility.LogErrorS(ex);Log.Error(error);
                objCompression.Flag = "F";
                string Exception = ex.Message;
            }
            
            return Ok(objCompression);
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

        private MemoryStream BytearrayToStream(byte[] arr)
        {
            return new MemoryStream(arr, 0, arr.Length);
        }

        private void HandleImageUpload(byte[] binaryImage)
        {
            Image img = RezizeImage(Image.FromStream(BytearrayToStream(binaryImage)), 103, 32);
            img.Save("IMAGELOCATION.png", System.Drawing.Imaging.ImageFormat.Gif);
        }

        #region Commented Code
        //HttpFileCollection files = HttpContext.Current.Request.Files;
        //List<string> pathlist = new List<string>();

        //try
        //{
        //    string path = HttpContext.Current.Server.MapPath("~/RegistrationImages/PhotoEbook/" + objCompression.OrderNo + "/");
        //    if (!Directory.Exists(path))
        //    {
        //        //If Directory (Folder) does not exists Create it.
        //        Directory.CreateDirectory(path);
        //    }

        //    for (int i = 0; i < files.Count; i++)
        //    {
        //        HttpPostedFile postedFile = files[i];

        //        string filename = Path.GetFileName(postedFile.FileName);
        //        string photoname = filename.Substring(0, filename.IndexOf("."));
        //        string contentType = postedFile.ContentType;
        //        objCompression.sourcePath = postedFile.InputStream;

        //        objCompression.targetPath = "\\RegistrationImages\\ImagePortfolios\\" + objCompression.OrderNo + "\\" + postedFile.FileName;

        //        //which Path to take ???????????????????

        //        pathlist.Add(@"\RegistrationImages\PhotoEbook\" + objCompression.OrderNo + "\\" + postedFile.FileName);

        //        #region Reduce Image Size

        //        using (var image = System.Drawing.Image.FromStream(objCompression.sourcePath))
        //        {
        //            var newWidth = (int)(image.Width * objCompression.scaleFactor);
        //            var newHeight = (int)(image.Height * objCompression.scaleFactor);
        //            var thumbnailImg = new Bitmap(newWidth, newHeight);
        //            var thumbGraph = Graphics.FromImage(thumbnailImg);
        //            thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
        //            thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
        //            thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //            var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
        //            thumbGraph.DrawImage(image, imageRectangle);
        //            thumbnailImg.Save(objCompression.targetPath, image.RawFormat);
        //        }

        //        #endregion

        //        #region Resize Image Height And Width
        //        using (var img = System.Drawing.Image.FromStream(objCompression.sourcePath))
        //        {
        //            if (img.Height < objCompression.CompressedImageHeight && img.Width < objCompression.CompressedImageWidth) return Json();

        //            using (img)
        //            {
        //                Double xRatio = (double)img.Width / maxWidth;
        //                Double yRatio = (double)img.Height / maxHeight;
        //                Double ratio = Math.Max(xRatio, yRatio);
        //                int nnx = (int)Math.Floor(img.Width / ratio);
        //                int nny = (int)Math.Floor(img.Height / ratio);
        //                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
        //                using (Graphics gr = Graphics.FromImage(cpy))
        //                {
        //                    gr.Clear(Color.Transparent);

        //                    // This is said to give best quality when resizing images
        //                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

        //                    gr.DrawImage(img,
        //                        new Rectangle(0, 0, nnx, nny),
        //                        new Rectangle(0, 0, img.Width, img.Height),
        //                        GraphicsUnit.Pixel);
        //                }
        //                return cpy;
        //            }
        //        }



        //        #endregion
        //    }
        //}
        //catch (Exception ex)
        //{
        //    string Exception = ex.Message;
        //}
        #endregion
    }
}
