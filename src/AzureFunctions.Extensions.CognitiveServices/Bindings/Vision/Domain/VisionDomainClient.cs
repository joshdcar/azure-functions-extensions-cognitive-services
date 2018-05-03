using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
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

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    public class VisionDomainClient
    {
        CognitiveServicesConfiguration _config;
        VisionDomainAttribute _attr;
        ILogger _log;

        public VisionDomainClient(CognitiveServicesConfiguration config, VisionDomainAttribute attr, ILoggerFactory loggerFactory)
        {
            this._config = config;
            this._attr = attr;
            this._log = loggerFactory?.CreateLogger("Host.Bindings.VisionDomain");
        }

        public async Task<VisionDomainCelebrityModel> AnalyzeCelebrityAsync(VisionDomainRequest request)
        {
            var result = await AnalyzeAsync<VisionDomainCelebrityModel>(request);

            return result;
        }

        public async Task<VisionDomainLandmarkModel> AnalyzeLandscapeAsync(VisionDomainRequest request)
        {
            var result = await AnalyzeAsync<VisionDomainLandmarkModel>(request);

            return result;
        }

        private async Task<T> AnalyzeAsync<T>(VisionDomainRequest request)
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

            var result = await SubmitRequest<T>(visionOperation);

            return result;
        }

        private async Task<T> SubmitRequest<T>(VisionDomainRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string requestParameters = GetVisionOperationParameters(request);

            string uri = $"{request.Url}/{requestParameters}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                _log.LogTrace($"Submitting Vision Domain Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                _log.LogMetric("VisionDomainRequestDurationMillisecond", sw.ElapsedMilliseconds);

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

                var result = JsonConvert.DeserializeObject<T>(requestResult.Contents);

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

        private string GetVisionOperationParameters(VisionDomainRequest request)
        {

            string optionsParam = string.Empty;

            switch (request.Domain)
            {
                case VisionDomainOptions.Celebrity:
                    optionsParam = "models/celebrities/analyze";
                    break;

                case VisionDomainOptions.Landmark:
                    optionsParam = "models/landmarks/analyze ";
                    break;
            }

            return optionsParam;
        }

        private async Task<VisionDomainRequest> MergeProperties(VisionDomainRequest operation, CognitiveServicesConfiguration config, VisionDomainAttribute attr)
        {
            //Attributes do not allow for enum types so we have to validate
            //the string passed into the attribute to ensure it matches
            //a valid VisionDomainOption. 
            VisionDomainOptions attrDomain = VisionDomainOptions.None;

            if (!string.IsNullOrEmpty(attr.Domain))
            {
                var valid = Enum.TryParse<VisionDomainOptions>(attr.Domain, out attrDomain);

                if (!valid)
                {
                    var message = string.Format(VisionExceptionMessages.InvalidDomainName, attr.Domain);
                    _log.LogWarning(message);

                    throw new ArgumentException(message);
                }
            }
            else
            {
                if(operation.Domain == VisionDomainOptions.None)
                {
                    var message = string.Format(VisionExceptionMessages.InvalidDomainName, "None");
                    _log.LogWarning(message);

                    throw new ArgumentException(message);
                }
            }


            var visionOperation = new VisionDomainRequest
            {
                Url = attr.Url ?? operation.Url,
                Key = attr.Key ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                Domain = attrDomain == VisionDomainOptions.None ? operation.Domain : attrDomain,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
            };

            if (string.IsNullOrEmpty(visionOperation.Key) && string.IsNullOrEmpty(visionOperation.SecureKey))
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
