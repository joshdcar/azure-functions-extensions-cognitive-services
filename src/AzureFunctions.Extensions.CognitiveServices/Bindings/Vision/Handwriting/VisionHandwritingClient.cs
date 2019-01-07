using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    public class VisionHandwritingClient
    {
        IVisionBinding _config;
        VisionHandwritingAttribute _attr;
        ILogger _log;

        public VisionHandwritingClient(IVisionBinding config, VisionHandwritingAttribute attr, ILoggerFactory loggerFactory)
        {
            this._config = config;
            this._attr = attr;
            this._log = loggerFactory?.CreateLogger("Host.Bindings.VisionHandwriting");
        }

        public async Task<VisionHandwritingModel> HandwritingAsync(VisionHandwritingRequest request)
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

                    _log.LogMetric("VisionOcrImageResizeDurationMillisecond", imageResizeSW.ElapsedMilliseconds);

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

        private async Task<VisionHandwritingModel> SubmitRequest(VisionHandwritingRequest request)
        {
            Stopwatch sw = new Stopwatch();

            //ocr/language=unk&detectOrientation=true
            string uri = $"{request.Url}/recognizeText?handwriting={request.Handwriting.ToString()}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                _log.LogTrace($"Submitting Vision Handwriting Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                _log.LogMetric("VisionHandwritingDurationMillisecond", sw.ElapsedMilliseconds);

            }
            else
            {
                using (ByteArrayContent content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.String);
                }
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.Accepted)
            {

                var operationLocation = string.Empty;

                operationLocation = requestResult.Headers.GetValues("Operation-Location").FirstOrDefault();

                _log.LogTrace($"Handwriting Request Async Operation Url (Polling) : {operationLocation}");

                VisionHandwritingModel result = await CheckForResult(operationLocation, request);

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

        private async Task<VisionHandwritingModel> CheckForResult(string operationUrl, VisionHandwritingRequest request)
        {

            PollingPolicy policy = new PollingPolicy();

            Random jitter = new Random();

            var timeoutPolicy = Policy
               .TimeoutAsync(TimeSpan.FromSeconds(policy.MaxRetryWaitTimeInSeconds), TimeoutStrategy.Pessimistic);

            var pollingRetryPolicy = Policy
                .HandleResult<VisionHandwritingModel>(r => r.Status != "Succeeded")
                .WaitAndRetryAsync(policy.MaxRetryAttempts,
                                   retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
                                   onRetry: (exception, retryCount, context) =>
                                   {
                                       _log.LogWarning($"Cognitive Service - Polling for Handwriting {retryCount} of {context.PolicyKey}, due to no status of Succeeded.");
                                   }
                );

            var pollingWrapper = timeoutPolicy.WrapAsync(pollingRetryPolicy);

            var visionHandwritingModel = await pollingWrapper.ExecuteAsync(async () => {

                var requestResult = await this._config.Client.GetAsync(operationUrl, request.Key, ReturnType.String);

                if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
                {
                    VisionHandwritingModel result = JsonConvert.DeserializeObject<VisionHandwritingModel>(requestResult.Contents);

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

            });

            return visionHandwritingModel;


        }

        private async Task<VisionHandwritingRequest> MergeProperties(VisionHandwritingRequest operation, IVisionBinding config, VisionHandwritingAttribute attr)
        {

            var visionOperation = new VisionHandwritingRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                AutoResize = attr.AutoResize,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
                Handwriting = attr.Handwriting.HasValue ? attr.Handwriting.Value : operation.Handwriting
            };


            if (string.IsNullOrEmpty(visionOperation.Key))
            {
                _log.LogWarning(VisionExceptionMessages.KeyMissing);
                throw new ArgumentException(VisionExceptionMessages.KeyMissing);
            }

            return visionOperation;

        }
    }
}
