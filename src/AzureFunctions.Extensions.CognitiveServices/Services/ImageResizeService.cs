using System;
using System.IO;
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
