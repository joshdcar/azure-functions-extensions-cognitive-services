using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
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

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    public class VisionOcrClient
    {
        IVisionBinding _config;
        VisionOcrAttribute _attr;
        ILogger _log;

        public VisionOcrClient(IVisionBinding config, VisionOcrAttribute attr, ILoggerFactory loggerFactory)
        {
            this._config = config;
            this._attr = attr;
            this._log = loggerFactory?.CreateLogger("Host.Bindings.VisionOcr");
        }

        public async Task<VisionOcrModel> OCRAsync(VisionOcrRequest request)
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

        private async Task<VisionOcrModel> SubmitRequest(VisionOcrRequest request)
        {
            Stopwatch sw = new Stopwatch();

            //ocr/language=unk&detectOrientation=true
            string uri = $"{request.Url}/ocr?detectOrientation={request.DetectOrientation.ToString()}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                _log.LogTrace($"Submitting Vision Ocr Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this._config.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                _log.LogMetric("VisionOCRDurationMillisecond", sw.ElapsedMilliseconds);

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
                _log.LogTrace($"OCR Request Results: {requestResult.Contents}");

                VisionOcrModel result = JsonConvert.DeserializeObject<VisionOcrModel>(requestResult.Contents);

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

        private async Task<VisionOcrRequest> MergeProperties(VisionOcrRequest operation, IVisionBinding config, VisionOcrAttribute attr)
        {

            var visionOperation = new VisionOcrRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                AutoResize = attr.AutoResize,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
                DetectOrientation = attr.DetectOrientation.HasValue ? attr.DetectOrientation.Value : operation.DetectOrientation
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
