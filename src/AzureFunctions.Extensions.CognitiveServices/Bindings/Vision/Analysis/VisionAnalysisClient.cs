using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    public class VisionAnalysisClient
    {
        IVisionBinding _config;
        VisionAnalysisAttribute _attr;
        ILogger _log;

        public VisionAnalysisClient(IVisionBinding config, VisionAnalysisAttribute attr, ILoggerFactory loggerFactory)
        {
            this._config = config;
            this._attr = attr;
            this._log = loggerFactory?.CreateLogger("Host.Bindings.VisionAnalysis");
        }

        public async Task<VisionAnalysisModel> AnalyzeAsync(VisionAnalysisRequest request)
        {
            Stopwatch imageResizeSW = null;

            var visionOperation = await MergeProperties(request, this._config, this._attr);

            if (request.IsUrlImageSource == false)
            {
        
                if (visionOperation.ImageBytes == null || visionOperation.ImageBytes.Length == 0)
                {
                    _log.LogWarning(VisionExceptionMessages.FileMissing);
                    throw new ArgumentException(VisionExceptionMessages.FileMissing);
                }

                if (ImageResizeService.IsImage(visionOperation.ImageBytes) == false)
                {
                    _log.LogWarning(VisionExceptionMessages.InvalidFileType); 
                    throw new ArgumentException(VisionExceptionMessages.InvalidFileType);
                }

                if (visionOperation.Oversized == true && visionOperation.AutoResize == false)
                {
                    var message = string.Format(VisionExceptionMessages.FileTooLarge,
                                                    VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                    _log.LogWarning(message);
                    throw new ArgumentException(message);
                }
                else if (visionOperation.Oversized == true && visionOperation.AutoResize == true)
                {
                    _log.LogTrace("Resizing Image");

                    imageResizeSW = new Stopwatch();

                    imageResizeSW.Start();

                    visionOperation.ImageBytes = ImageResizeService.ResizeImage(visionOperation.ImageBytes);

                    imageResizeSW.Stop();

                    _log.LogMetric("VisionAnalysisImageResizeDurationMillisecond", imageResizeSW.ElapsedMilliseconds);

                    if (visionOperation.Oversized)
                    {
                        var message = string.Format(VisionExceptionMessages.FileTooLargeAfterResize, 
                                                        VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                        _log.LogWarning(message);
                        throw new ArgumentException(message);
                    }

                }
            }

            var result = await SubmitRequest(visionOperation);

            return result;
        }

        private async Task<VisionAnalysisModel> SubmitRequest(VisionAnalysisRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string requestParameters = GetVisionOperationParameters(request);

            string uri = $"{request.Url}/analyze?{requestParameters}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                _log.LogTrace($"Submitting Vision Analysis Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                _log.LogMetric("VisionRequestDurationMillisecond", sw.ElapsedMilliseconds);

            }
            else
            {
                using (ByteArrayContent content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.String);
                }
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                _log.LogTrace($"Analysis Request Results: {requestResult.Contents}");

                VisionAnalysisModel result = JsonConvert.DeserializeObject<VisionAnalysisModel>(requestResult.Contents);

                return result;
            }
            else if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.BadRequest)
            {

                VisionErrorModel error = JsonConvert.DeserializeObject<VisionErrorModel>(requestResult.Contents);
                var message = string.Format(VisionExceptionMessages.CognitiveServicesException, error.Code, error.Message);

                _log.LogWarning(message);

                throw new Exception(message);
            }
            else
            {
                var message = string.Format(VisionExceptionMessages.CognitiveServicesException, requestResult.HttpStatusCode, requestResult.Contents);

                _log.LogError(message);

                throw new Exception(message);
            }

        }

        private string GetVisionOperationParameters(VisionAnalysisRequest request)
        {
            VisionAnalysisOptions options = request.Options;

            string optionsParam = string.Empty;
            string visualFeaturesParam = string.Empty;
            string detailsParam = string.Empty;
            string languageParam = "&language=en";

            if (options == VisionAnalysisOptions.All)
            {
                options = VisionAnalysisOptions.Categories |
                                  VisionAnalysisOptions.Celebrities |
                                  VisionAnalysisOptions.Color |
                                  VisionAnalysisOptions.Description |
                                  VisionAnalysisOptions.Faces |
                                  VisionAnalysisOptions.ImageType |
                                  VisionAnalysisOptions.Landmarks |
                                  VisionAnalysisOptions.Tags;
            }


            //Details Parameters
            if (options.HasFlag(VisionAnalysisOptions.Celebrities)
                || options.HasFlag(VisionAnalysisOptions.Landmarks))
            {

                List<string> details = new List<string>();

                if (options.HasFlag(VisionAnalysisOptions.Celebrities))
                {
                    details.Add("Celebrities");
                }

                if (options.HasFlag(VisionAnalysisOptions.Landmarks))
                {
                    details.Add("Landmarks");
                }

                detailsParam = $"&details={string.Join(",", details)}";

                //Remove the Details Flags from the options so they are not
                //included in subsequent operations
                options = options & ~(VisionAnalysisOptions.Celebrities | VisionAnalysisOptions.Landmarks);

            }

            //Visual Features Parameter
            visualFeaturesParam = options.ToString();
            visualFeaturesParam = visualFeaturesParam.Replace(" ", string.Empty);
            visualFeaturesParam = $"visualFeatures={visualFeaturesParam}";

            //Combine All parameter
            optionsParam = $"{visualFeaturesParam}{detailsParam}{languageParam}";

            return optionsParam;
        }

        private async Task<VisionAnalysisRequest> MergeProperties(VisionAnalysisRequest operation, IVisionBinding config, VisionAnalysisAttribute attr)
        {


            var visionOperation = new VisionAnalysisRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? operation.SecureKey,
                AutoResize = attr.AutoResize,
                Options = operation.Options,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
            };

            if(string.IsNullOrEmpty(visionOperation.Key) && string.IsNullOrEmpty(visionOperation.SecureKey))
            {
                _log.LogWarning(VisionExceptionMessages.KeyMissing);
                throw new ArgumentException(VisionExceptionMessages.KeyMissing);
            }

            if (!string.IsNullOrEmpty(visionOperation.SecureKey))
            {
                HttpClient httpClient = this._config.Client.GetHttpClientInstance();

                visionOperation.Key = await KeyVaultServices.GetValue(visionOperation.SecureKey, httpClient);
            }

            return visionOperation;

        }

    }

}
