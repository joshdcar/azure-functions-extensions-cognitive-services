using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using AzureFunctions.Extensions.CognitiveServices.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctions.Extensions.CognitiveServics.Samples
{
    [StorageAccount("storageaccount")]
    public static class CognitiveServicesFunctions
    {

        #region Vision Analysis

        /// <summary>
        /// Sample calling Vision Analysis Triggered from a blob storage 
        ///     Trigger: Blob Storage
        ///     Vision Binding:  Model Binding w/ Blob Data Source
        /// </summary>
        /// <param name="storageBlob"></param>
        /// <param name="result"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("VisionAnalysisModelBlobFunction")]
        public static async Task VisionAnalysisModelBlobFunctionRun(
           [BlobTrigger("analysismodel/{name}")]Stream storageBlob,
           [VisionAnalysis(BlobStorageAccount = "StorageAccount",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionAnalysisModel result,
           string name,
           TraceWriter log)
        {

            log.Info($"Analysis Results:{result}");

        }


        [FunctionName("VisionAnalysisBlobFunction")]
        public static async Task VisionAnalysisRun(
           [BlobTrigger("analysis/{name}")]Stream storageBlob,
           [Table("VisionResults")]IAsyncCollector<VisionResult> results,
           [VisionAnalysis()]VisionAnalysisClient visionclient,
           string name,
           TraceWriter log)
        {

            var result = await visionclient.AnalyzeAsync(new VisionAnalysisRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionAnalysis") { ResultJson = result.ToString() });

            log.Info($"Analysis Results:{result.ToString()}");

        }

        #endregion


        [FunctionName("VisionDesribeBlobFunction")]
        public static async Task VisionDescribeBlobFunction(
            [BlobTrigger("describe/{name}")]Stream storageBlob,
            [Table("VisionResults")]IAsyncCollector<VisionResult> results,
            [VisionDescribe()]VisionDescribeClient visionclient,
            string name,
            TraceWriter log
            )
        {
            var result = await visionclient.DescribeAsync(new VisionDescribeRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDescribe") { ResultJson = result.ToString() });

            log.Info($"Describe Results:{result.ToString()}");

        }

        [FunctionName("VisionDescribeModelBlobFunction")]
        public static async Task VisionDescribeModelBlobFunction(
            [BlobTrigger("describe/{name}")]Stream storageBlob,
            [Table("VisionResults")]IAsyncCollector<VisionResult> results,
            [VisionDescribe(BlobStorageAccount = "StorageAccount",
                           BlobStoragePath = "describemodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionDescribeModel result,
            string name,
            TraceWriter log
            )
        {
            log.Info($"Describe Results:{result.ToString()}");
        }

        [FunctionName("VisionThumbnailBlobFunction")]
        public static async Task VisionThumbnailBlobFunction(
         [BlobTrigger("thumbnail/{name}")]Stream storageBlob,
         [VisionThumbnail(AutoResize = true,
                          Height ="100",
                          Width = "100",
                          SmartCropping =true)]VisionThumbnailClient visionclient,
         [Blob("thumbnailresults/{name}", FileAccess.Write)]Stream thumbnailBlob,
         string name,
         TraceWriter log
         )
        {

            var result = await visionclient.ThumbnailAsync(new VisionThumbnailRequest(storageBlob));

            using (MemoryStream stream = new MemoryStream(result))
            {
                await stream.CopyToAsync(thumbnailBlob);
            }

            log.Info($"Image thumbnail generated");

        }

        [FunctionName("VisionOcrBlobFunction")]
        public static async Task VisionOcrBlobFunction(
            [BlobTrigger("ocrrequest/{name}")]Stream storageBlob,
            [Table("VisionResults")]IAsyncCollector<VisionResult> results,
            [VisionOcr()]VisionOcrClient visionclient,
            string name,
            TraceWriter log
            )
        {
            var result = await visionclient.OCRAsync(new VisionOcrRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionOCR") { ResultJson = result.ToString() });

            log.Info($"OCR Results:{result.ToString()}");

        }

        [FunctionName("VisionHandwritingBlobFunction")]
        public static async Task VisionHandwritingBlobFunction(
            [BlobTrigger("handwriting/{name}")]Stream storageBlob,
            [Table("VisionResults")]IAsyncCollector<VisionResult> results,
            [VisionHandwriting()]VisionHandwritingClient visionclient,
            string name,
            TraceWriter log
            )
        {
            var result = await visionclient.HandwritingAsync(new VisionHandwritingRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionHandwriting") { ResultJson = result.ToString() });

            log.Info($"Handwriting Results:{result.ToString()}");

        }

        [FunctionName("VisionCelebrityBlobFunction")]
        public static async Task VisionCelebrityBlobFunction(
           [BlobTrigger("celebrity/{name}")]Stream storageBlob,
           [Table("VisionResults")]IAsyncCollector<VisionResult> results,
           [VisionDomain(Domain = VisionDomainRequest.CELEBRITY_DOMAIN)]VisionDomainClient visionclient,
           string name,
           TraceWriter log
           )
        {
            var request = new VisionDomainRequest(storageBlob) { Domain = VisionDomainOptions.Celebrity };

            var celebrityResult = await visionclient.AnalyzeCelebrityAsync(request);

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDomain") { ResultJson = celebrityResult.ToString() });

            log.Info($"Celebrity Domain results:{celebrityResult.ToString()}");


        }

        [FunctionName("VisionLandmarkBlobFunction")]
        public static async Task VisionLandmarkBlobFunction(
          [BlobTrigger("landmarks/{name}")]Stream storageBlob,
          [Table("VisionResults")]IAsyncCollector<VisionResult> results,
          [VisionDomain(Domain = VisionDomainRequest.LANDMARK_DOMAIN)]VisionDomainClient visionclient,
          string name,
          TraceWriter log
          )
        {
            var landmarkResult = await visionclient.AnalyzeLandmarkAsync(new VisionDomainRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDomain") { ResultJson = landmarkResult.ToString() });

            log.Info($"Celebrity Domain results:{landmarkResult.ToString()}");


        }
    }
}
