using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace UploadMusic.Models
{
    public class CompressionModel
    {
        public double scaleFactor { get; set; }
        public Stream sourcePath { get; set; }
        public string targetPath { get; set; }
        public string OrderNo { get; set; }

        public int CompressedImageHeight { get; set; }
        public int CompressedImageWidth { get; set; }
        public string Flag { get; set; }
        public byte[] ImageByte { get; set; }
        public string ImagePath { get; set; }

    }
}