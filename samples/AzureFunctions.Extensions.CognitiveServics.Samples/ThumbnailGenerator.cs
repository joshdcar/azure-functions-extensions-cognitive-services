
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using AzureFunctions.Extensions.CognitiveServices.Bindings;

namespace AzureFunctions.Extensions.CognitiveServics.Samples
{
    public static class ThumbnailGenerator
    {


        [StorageAccount("StorageAccount")]
        [FunctionName("ThumbnailGenApi")]
        public static async Task<IActionResult> ThumbnailGenApi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "generatethumbnail/{filename}")]HttpRequest req,
            [Blob("images/{filename}", FileAccess.Write)]Stream imageStream,
            [Blob("thumbnails/{filename}", FileAccess.Write)]Stream thumbnailImageStream,
            [VisionThumbnail(SmartCropping = true, Height = "150", Width = "150")]VisionThumbnailClient client,
            string fileName,
            TraceWriter log)
        {

            //Load Http Request Body into Thumbnail Request

            var request = new VisionThumbnailRequest(req.Body);

            //Generate Thumbnail
            var thumbnailBytes = await client.ThumbnailAsync(request);

            //Output Original Image to Blob Storage
            using (MemoryStream stream = new MemoryStream(request.ImageBytes))
            {
                await stream.CopyToAsync(imageStream);
            }

            //Output Thumbnail To Blob Storage
            using (MemoryStream stream = new MemoryStream(thumbnailBytes))
            {
                await stream.CopyToAsync(thumbnailImageStream);
            }

                
            return (ActionResult)new OkResult();

        }

        [StorageAccount("StorageAccount")]
        [FunctionName("ThumbnailGenBlobTrigger")]
        public static void ThumbnailGenBlobTrigger(

            [BlobTrigger("myimages/{filename}")]byte[] image,
            [Blob("mythumbnails/{filename}", FileAccess.Write)]out byte[] thumbnailImage,
            [VisionThumbnail(ImageSource=ImageSource.BlobStorage,
                                BlobStorageAccount ="StorageAccount",
                                BlobStoragePath ="images/{filename}",
                                SmartCropping = true,
                                Height = "150",
                                Width = "150")]byte[] thumbnail,
            string fileName,
            TraceWriter log)
        {
            //Output thumbnail
            thumbnailImage = thumbnail;

        }
    }
}
