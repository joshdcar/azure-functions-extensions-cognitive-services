using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    public class CognitiveServicesClient : ICognitiveServicesClient
    {
        private static HttpClient _client = new HttpClient();
        private PolicyWrap<HttpResponseMessage> _retryPolicyWrapper;
        private ILogger _log;


        public HttpClient GetHttpClientInstance()
        {
            return _client;
        }

        public CognitiveServicesClient(RetryPolicy retryPolicy, ILoggerFactory loggerFactory)
        {
            this._log = loggerFactory?.CreateLogger("Host.Bindings.CognitiveServicesClient");

            Random jitter = new Random();

            var timeoutPolicy = Policy
                .TimeoutAsync(TimeSpan.FromSeconds(retryPolicy.MaxRetryWaitTimeInSeconds), TimeoutStrategy.Pessimistic);

            var throttleRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode)429)
                .WaitAndRetryAsync(retryPolicy.MaxRetryAttemptsAfterThrottle,
                                   retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
                                   onRetry: (exception, retryCount, context) =>
                                   {
                                       _log.LogWarning($"Cognitive Service - Retry {retryCount} of {context.PolicyKey}, due to 429 throttling.");
                                   }
                );


            _retryPolicyWrapper = timeoutPolicy.WrapAsync(throttleRetryPolicy);

        }

        public async Task<ServiceResultModel> PostAsync(string uri, string key, StringContent content, ReturnType returnType)
        {
            var httpResponse = await _retryPolicyWrapper.ExecuteAsync(async () => {

                _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

                var response = await _client.PostAsync(uri, content);

                return response;

            });

            var result = new ServiceResultModel { HttpStatusCode = (int)httpResponse.StatusCode };

            if (returnType == ReturnType.String)
            {
                result.Contents = await httpResponse.Content.ReadAsStringAsync();
                result.Headers = httpResponse.Headers;
            }

            if (returnType == ReturnType.Binary)
            {
                result.Binary = await httpResponse.Content.ReadAsByteArrayAsync();
                result.Headers = httpResponse.Headers;
            }


            return result;


        }

        public async Task<ServiceResultModel> PostAsync(string uri, string key, ByteArrayContent content, ReturnType returnType)
        {
            var httpResponse = await _retryPolicyWrapper.ExecuteAsync(async () => {

                _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

                var response = await _client.PostAsync(uri, content);

                return response;

            });

            var result = new ServiceResultModel { HttpStatusCode = (int)httpResponse.StatusCode };

            if (returnType == ReturnType.String)
            {
                result.Contents = await httpResponse.Content.ReadAsStringAsync();
                result.Headers = httpResponse.Headers;
            }

            if (returnType == ReturnType.Binary)
            {
                result.Binary = await httpResponse.Content.ReadAsByteArrayAsync();
                result.Headers = httpResponse.Headers;
            }


            return result;
        }

        public async Task<ServiceResultModel> GetAsync(string uri, string key, ReturnType returnType)
        {
            var httpResponse = await _retryPolicyWrapper.ExecuteAsync(async () => {

                _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                var response = await _client.GetAsync(uri);

                return response;

            });

            var result = new ServiceResultModel { HttpStatusCode = (int)httpResponse.StatusCode };

            if (returnType == ReturnType.String)
            {
                result.Headers = httpResponse.Headers;
                result.Contents = await httpResponse.Content.ReadAsStringAsync();
            }

            if (returnType == ReturnType.Binary)
            {
                result.Headers = httpResponse.Headers;
                result.Binary = await httpResponse.Content.ReadAsByteArrayAsync();
            }

            return result;
        }
    }
}
