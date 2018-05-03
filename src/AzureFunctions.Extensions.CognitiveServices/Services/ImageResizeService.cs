using System;
using System.IO;
using AzureFunctions.Extensions.CognitiveServices.Config;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Jpeg;
//using SixLabors.ImageSharp.Formats.Png;
//using SixLabors.ImageSharp.Processing;
//using SixLabors.ImageSharp.Processing.Transforms;
//using SixLabors.Primitives;

namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    public class ImageResizeService
    {

        /// <summary>
        /// Resizes an image with the goal of getting the file under the default file size limit for Cognitive Services
        /// 
        /// NOTE: This feature is currently disabled. The feature is based on the SixLabors.ImageSharp project (in preview) that
        /// depends on a System.Numerics.Vectors library version also in preview.  The preview version of System.Numerics.Vectors
        /// causes an assembly load error w/ the production version of System.Numerics.Vectors. Currently ImageSharp is the only 
        /// known fully native .net core image manipulation library. 
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] ResizeImage(Byte[] file)
        {

            throw new NotSupportedException("Due to preview library version conflicts image resize is not yet supported with the preview.");


            //using (var img = Image.Load(file))
            //{
            //    var jpegEncoder = new JpegEncoder() { Quality = VisionConfiguration.DefaultResizeQuality };

            //    using (var resizedImage = new MemoryStream())
            //    {

            //        var options = new ResizeOptions
            //        {
            //            Size = new Size(img.Width / 2, img.Height / 2),
            //            Mode = ResizeMode.Max
            //        };

            //        img.Mutate(x => x.Resize(options));

            //        img.SaveAsJpeg(resizedImage, jpegEncoder);

            //        byte[] results = resizedImage.ToArray();

            //        return results;
            //    }

            //}

        }

    }
}
