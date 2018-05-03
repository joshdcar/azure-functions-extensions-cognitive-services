﻿using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    public class TestHelper
    {
        public static async Task ExecuteFunction<T>(ICognitiveServicesClient client, string functionReference)
        {
            var cognitiveServicesConfig = new CognitiveServicesConfiguration();
           
            cognitiveServicesConfig.Client = client;

            var jobHost = NewHost<T>(cognitiveServicesConfig);
            
            var args = new Dictionary<string, object>();
            await jobHost.CallAsync(functionReference, args);
        }

        public static JobHost NewHost<T>(IExtensionConfigProvider ext)
        {
            JobHostConfiguration config = new JobHostConfiguration();
            config.HostId = Guid.NewGuid().ToString("n");
            config.StorageConnectionString = null;
            config.DashboardConnectionString = null;
            config.TypeLocator = new FakeTypeLocator<T>();
            config.AddExtension(ext);
            config.NameResolver = new NameResolver();

            var host = new JobHost(config);
           
            return host;
        }


    }

    public class NameResolver : INameResolver
    {
        IConfigurationRoot _config;

        public NameResolver()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

        }
        public string Resolve(string name)
        {
            name = $"Values:{name}";

            var value = _config[name].ToString();

            return value;
        }
    }

    public class FakeTypeLocator<T> : ITypeLocator
    {
        public IReadOnlyList<Type> Types => new Type[] { typeof(T) };
        public IReadOnlyList<Type> GetTypes()
        {
            return Types;
        }
    }
}
