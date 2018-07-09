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

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    public class VisionDescribeClient
    {
        IVisionBinding _config;
        VisionDescribeAttribute _attr;
        ILogger _log;

        public VisionDescribeClient(IVisionBinding config, VisionDescribeAttribute attr, ILoggerFactory loggerFactory)
        {
            this._config = config;
            this._attr = attr;
            this._log = loggerFactory?.CreateLogger("Host.Bindings.VisionDescribe");
        }

        public async Task<VisionDescribeModel> DescribeAsync(VisionDescribeRequest request)
        {
            try
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
            catch(Exception ex )
            {
                throw ex;
            }
        }

        private async Task<VisionDescribeModel> SubmitRequest(VisionDescribeRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string uri = $"{request.Url}/describe?maxCandidates={request.MaxCandidates}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                _log.LogTrace($"Submitting Vision Describe Request");

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
                _log.LogTrace($"Describe Request Results: {requestResult.Contents}");

                VisionDescribeModel result = JsonConvert.DeserializeObject<VisionDescribeModel>(requestResult.Contents);

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

        private async Task<VisionDescribeRequest> MergeProperties(VisionDescribeRequest operation, IVisionBinding config, VisionDescribeAttribute attr)
        {

            var visionOperation = new VisionDescribeRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                MaxCandidates = operation.MaxCandidates,
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
