using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PMDWebApplication
{
    public static class Extension
    {
        static Extension()
        {
            ImageTypes = new Dictionary<string, string>();
            ImageTypes.Add("FFD8", "jpg");
            ImageTypes.Add("424D", "bmp");
            ImageTypes.Add("474946", "gif");
            ImageTypes.Add("89504E470D0A1A0A", "png");

           /* PDFTypes = new Dictionary<string, string>();
            //PDFTypes.Add("25504446", "pdf");
            PDFTypes.Add("255044462D", "pdf");*/
        }

        /// <summary>
        ///     <para> Registers a hexadecimal value used for a given image type </para>
        ///     <param name="imageType"> The type of image, example: "png" </param>
        ///     <param name="uniqueHeaderAsHex"> The type of image, example: "89504E470D0A1A0A" </param>
        /// </summary>
        public static void RegisterImageHeaderSignature(string imageType, string uniqueHeaderAsHex)
        {
            Regex validator = new Regex(@"^[A-F0-9]+$", RegexOptions.CultureInvariant);

            uniqueHeaderAsHex = uniqueHeaderAsHex.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(imageType)) throw new ArgumentNullException("imageType");
            if (string.IsNullOrWhiteSpace(uniqueHeaderAsHex)) throw new ArgumentNullException("uniqueHeaderAsHex");
            if (uniqueHeaderAsHex.Length % 2 != 0) throw new ArgumentException("Hexadecimal value is invalid");
            if (!validator.IsMatch(uniqueHeaderAsHex)) throw new ArgumentException("Hexadecimal value is invalid");

            ImageTypes.Add(uniqueHeaderAsHex, imageType);
        }

        private static Dictionary<string, string> ImageTypes;
        //private static Dictionary<string, string> PDFTypes;


        public static bool IsImage(this Stream stream)
        {
            string imageType;
            return stream.IsImage(out imageType);
        }

        public static bool IsImage(this Stream stream, out string imageType)
        {
            stream.Seek(0, SeekOrigin.Begin);
            StringBuilder builder = new StringBuilder();
            int largestByteHeader = ImageTypes.Max(img => img.Value.Length);

            for (int i = 0; i < largestByteHeader; i++)
            {
                string bit = stream.ReadByte().ToString("X2");
                builder.Append(bit);

                string builtHex = builder.ToString();
                bool isImage = ImageTypes.Keys.Any(img => img == builtHex);
                if (isImage)
                {
                    imageType = ImageTypes[builder.ToString()];
                    return true;
                }
            }
            imageType = null;
            return false;
        }
/*
        public static bool IsPDF(this Stream stream)
        {
            string pdfType;
            return stream.IsPDF(out pdfType);
        }

        public static bool IsPDF(this Stream stream, out string pdfType)
        {
            stream.Seek(0, SeekOrigin.Begin);
            StringBuilder builder = new StringBuilder();
            int largestByteHeader = PDFTypes.Max(pdf => pdf.Value.Length);

            for (int i = 0; i < largestByteHeader; i++)
            {
                string bit = stream.ReadByte().ToString("X2");
                builder.Append(bit);

                string builtHex = builder.ToString();
                bool isPDF = PDFTypes.Keys.Any(pdf => pdf == builtHex);
                if (isPDF)
                {
                    pdfType = PDFTypes[builder.ToString()];
                    return true;
                }
            }
            pdfType = null;
            return false;
        }*/
    }
}