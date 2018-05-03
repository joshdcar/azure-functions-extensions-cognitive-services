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

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    public class VisionThumbnailClient
    {
        CognitiveServicesConfiguration _config;
        VisionThumbnailAttribute _attr;
        ILogger _log;

        public VisionThumbnailClient(CognitiveServicesConfiguration config, VisionThumbnailAttribute attr, ILoggerFactory loggerFactory)
        {
            this._config = config;
            this._attr = attr;
            this._log = loggerFactory?.CreateLogger("Host.Bindings.VisionThumbnail");
        }

        public async Task<Byte[]> ThumbnailAsync(VisionThumbnailRequest request)
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

            var result = await SubmitRequest(visionOperation);

            return result;
        }

        private async Task<Byte[]> SubmitRequest(VisionThumbnailRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string uri = $"{request.Url}/generateThumbnail?width={request.Width}&height={request.Height}&smartCropping={request.SmartCropping.ToString()}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                _log.LogTrace($"Submitting Vision Thumbnail Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.Binary);

                sw.Stop();

                _log.LogMetric("VisionRequestDurationMillisecond", sw.ElapsedMilliseconds);

            }
            else
            {
                using (ByteArrayContent content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.Binary);
                }
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                _log.LogTrace($"Thumbnail Request Results");

                byte[] fileResult = requestResult.Binary;

                return fileResult;
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

        private async Task<VisionThumbnailRequest> MergeProperties(VisionThumbnailRequest operation, CognitiveServicesConfiguration config, VisionThumbnailAttribute attr)
        {

            var visionOperation = new VisionThumbnailRequest
            {
                Url = attr.Url ?? operation.Url,
                Key = attr.Key ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                Height = attr.Height,
                Width = attr.Width,
                SmartCropping = attr.SmartCropping,
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
