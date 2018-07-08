using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AzureFunctions.Extensions.CognitiveServices.Config;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace AzureFunctions.Extensions.CognitiveServices.Services
{

    public class ImageResizeService
    {
        private const string JPG_HEADER = "FFD8FF";
        private const string BMP_HEADER = "424D";
        private const string GIF_HEADER = "474946";
        private const string PNG_HEADER = "89504E470D0A1A0A";

        /// <summary>
        /// Use the known header approach to identify if a file is an image
        /// (more reliable then just looking at the extension)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsImage(Byte[] file)
        {
            byte[] fileHeaderBuffer = new byte[8];

            string header = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                            file[0].ToString("X2"),
                                            file[1].ToString("X2"),
                                            file[2].ToString("X2"),
                                            file[3].ToString("X2"),
                                            file[4].ToString("X2"),
                                            file[5].ToString("X2"),
                                            file[6].ToString("X2"),
                                            file[7].ToString("X2"));

            if (header.StartsWith(JPG_HEADER) || 
                header.StartsWith(GIF_HEADER) ||
                header.StartsWith(BMP_HEADER) ||
                header.StartsWith(PNG_HEADER))
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Resizes an image with the goal of getting the file under the default file size limit for Cognitive Services
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] ResizeImage(Byte[] file)
        {

            using (var img = Image.Load(file))
            {
                var jpegEncoder = new JpegEncoder() { Quality = VisionConfiguration.DefaultResizeQuality };

                using (var resizedImage = new MemoryStream())
                {

                    var options = new ResizeOptions
                    {
                        Size = new Size(img.Width / 2, img.Height / 2),
                        Mode = ResizeMode.Max
                    };

                    img.Mutate(x => x.Resize(options));

                    img.SaveAsJpeg(resizedImage, jpegEncoder);

                    byte[] results = resizedImage.ToArray();

                    return results;
                }

            }

        }

    }
}
