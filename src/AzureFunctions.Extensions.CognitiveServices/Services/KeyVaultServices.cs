using AzureFunctions.Extensions.CognitiveServices.Config;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    public class KeyVaultServices 
    {
        public static async Task<string> GetValue(string secretKey,  HttpClient httpClient)
        {
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

                KeyVaultClient client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback), httpClient);
                var setting = await client.GetSecretAsync(secretKey);

                return setting.Value;
            }
            catch (Exception ex)
            {
                var msg = string.Format(string.Format(VisionExceptionMessages.KeyvaultException, ex.ToString()));
                throw new Exception(msg,ex);
            }
       
        }


    }
}
