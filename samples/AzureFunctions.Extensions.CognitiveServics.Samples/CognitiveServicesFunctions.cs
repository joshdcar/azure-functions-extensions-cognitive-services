using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctions.Extensions.CognitiveServics.Samples
{
    [StorageAccount("storageaccount")]
    public static class CognitiveServicesFunctions
    {

        [FunctionName("VisionAnalysisWithKeyVaultBlobFunction")]
        public static async Task VisionAnalysisWithKeyVaultRun(
           [BlobTrigger("analysiskeyvault/{name}")]Stream storageBlob,
           [Table("VisionResults")]IAsyncCollector<VisionResult> results,
           [VisionAnalysis(SecureKey = "%VisionApiKeyVaultSetting%", Url = "%VisionUrl%")]VisionAnalysisClient visionclient,
           string name,
           TraceWriter log)
        {

            var result = await visionclient.AnalyzeAsync(new VisionAnalysisRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionAnalysis") { ResultJson = result.ToString() });

            log.Info($"Analysis Results:{result.ToString()}");

        }

        [FunctionName("VisionAnalysisBlobFunction")]
        public static async Task VisionAnalysisRun(
           [BlobTrigger("analysis/{name}")]Stream storageBlob,
           [Table("VisionResults")]IAsyncCollector<VisionResult> results,
           [VisionAnalysis(Key = "%VisionKey%", Url = "%VisionUrl%", AutoResize = true)]VisionAnalysisClient visionclient,
           string name,
           TraceWriter log)
        {

            var result = await visionclient.AnalyzeAsync(new VisionAnalysisRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionAnalysis") { ResultJson = result.ToString() });

            log.Info($"Analysis Results:{result.ToString()}");

        }

        [FunctionName("VisionDesribeBlobFunction")]
        public static async Task VisionDescribeBlobFunction(
            [BlobTrigger("describe/{name}")]Stream storageBlob,
            [Table("VisionResults")]IAsyncCollector<VisionResult> results,
            [VisionDescribe(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionDescribeClient visionclient,
            string name,
            TraceWriter log
            )
        {
            var result = await visionclient.DescribeAsync(new VisionDescribeRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDescribe") { ResultJson = result.ToString() });

            log.Info($"Describe Results:{result.ToString()}");

        }

        [FunctionName("VisionThumbnailBlobFunction")]
        public static async Task VisionThumbnailBlobFunction(
         [BlobTrigger("thumbnail/{name}")]Stream storageBlob,
         [VisionThumbnail(Key = "%VisionKey%",
                          Url = "%VisionUrl%",
                          AutoResize = true,
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
            [VisionOcr(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionOcrClient visionclient,
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
            [VisionHandwriting(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionHandwritingClient visionclient,
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
           [VisionDomain(Key = "%VisionKey%", Url = "%VisionUrl%", Domain = VisionDomainRequest.CELEBRITY_DOMAIN)]VisionDomainClient visionclient,
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
          [VisionDomain(Key = "%VisionKey%", Url = "%VisionUrl%", Domain = VisionDomainRequest.LANDMARK_DOMAIN)]VisionDomainClient visionclient,
          string name,
          TraceWriter log
          )
        {
            var landmarkResult = await visionclient.AnalyzeLandscapeAsync(new VisionDomainRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDomain") { ResultJson = landmarkResult.ToString() });

            log.Info($"Celebrity Domain results:{landmarkResult.ToString()}");


        }
    }
}
