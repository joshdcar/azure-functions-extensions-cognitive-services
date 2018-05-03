using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    public class TestCognitiveServicesClient : ICognitiveServicesClient
    {
        public Task<ServiceResultModel> GetAsync(string uri, string key, ReturnType returnType)
        {   
            throw new NotImplementedException();
        }

        public HttpClient GetHttpClientInstance()
        {
            return new HttpClient();
        }

        public Task<ServiceResultModel> PostAsync(string uri, string key, StringContent content, ReturnType returnType)
        {
            ServiceResultModel result = null;
            
            if(uri.Contains("vision") == true)
            {
                result =  new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionAnalysisResults };
            }

            if (uri.Contains("describe") == true)
            {
                result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionDescribeResults };
            }

            return Task.FromResult<ServiceResultModel>(result);
        }

        public Task<ServiceResultModel> PostAsync(string uri, string key, ByteArrayContent content, ReturnType returnType)
        {
            ServiceResultModel result = null;

            if (returnType == ReturnType.String)
            {
                if (uri.Contains("vision") == true)
                {
                    result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionAnalysisResults };
                }

                if (uri.Contains("describe") == true)
                {
                    result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionDescribeResults };
                }

            }

            if (returnType == ReturnType.Binary)
            {
                result = new ServiceResultModel { HttpStatusCode = 200, Binary = MockResults.SamplePhoto };
 
            }

            return Task.FromResult<ServiceResultModel>(result);
        }
    }
}
