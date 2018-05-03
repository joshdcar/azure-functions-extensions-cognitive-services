using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    public enum ReturnType
    {
        String,
        Binary
    }

    public interface ICognitiveServicesClient
    {

        HttpClient GetHttpClientInstance();

        Task<ServiceResultModel> PostAsync(string uri, string key, StringContent content, ReturnType returnType);

        Task<ServiceResultModel> PostAsync(string uri, string key, ByteArrayContent content, ReturnType returnType);

        Task<ServiceResultModel> GetAsync(string uri, string key, ReturnType returnType);

    }
}
