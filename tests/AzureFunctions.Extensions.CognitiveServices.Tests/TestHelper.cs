using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AzureFunctions.Extensions.CognitiveServices.Tests.Common;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    public class TestHelper2
    {
        private static string functionOut = null;

        public static async Task ExecuteFunction<FunctionType, BindingType>(ICognitiveServicesClient client, 
                                                                            Type hostType,
                                                                            string functionReference)
        {


            //IExtensionConfigProvider binding = null;

            //if (typeof(BindingType) == typeof(VisionAnalysisBinding))
            //{
            //    binding = new VisionAnalysisBinding();
            //}

            //if (typeof(BindingType) == typeof(VisionDescribeBinding))
            //{
            //    binding = new VisionDescribeBinding();
            //}

            //if (typeof(BindingType) == typeof(VisionHandwritingBinding))
            //{
            //    binding = new VisionHandwritingBinding();
            //}

            //if (typeof(BindingType) == typeof(VisionOcrBinding))
            //{
            //    binding = new VisionOcrBinding();
            //}

            //if (typeof(BindingType) == typeof(VisionThumbnailBinding))
            //{
            //    binding = new VisionThumbnailBinding();
            //}

            //(binding as IVisionBinding).Client = client;


            //var jobHost = NewHost<FunctionType>(binding);

            //var args = new Dictionary<string, object>();
            //await jobHost.CallAsync(functionReference, args);

            //Dummy data to use later
            //var args = new Dictionary<string, object>{
            //    //{ "fileName", testFileName  }
            //};

            //// make sure we can write the file to data lake store
            //using (var host = await StartHostAsync(hostType))
            //{
            //    await host.GetJobHost().CallAsync(functionReference, args);
            //    functionOut = null;
            //}



        }

        //public static JobHost NewHost<T>(IExtensionConfigProvider ext)
        //{
        //    var builder = new HostBuilder().ConfigureWebJobs(b =>
        //                                    b.AddAzureStorageCoreServices());

        //    var host = new JobHost()


        //        //JobHostConfiguration config = new JobHostConfiguration();
        //        //config.HostId = Guid.NewGuid().ToString("n");
        //        //config.StorageConnectionString = null;
        //        //config.DashboardConnectionString = null;
        //        //config.TypeLocator = new FakeTypeLocator<T>();
        //        //config.AddExtension(ext);
        //        //config.NameResolver = new NameResolver();

        //        //var host = new JobHost()

        //        //return host;
        //   }

        //public static JobHost CreateJobHost(ILoggerProvider loggerProvider,INameResolver nameResolver)
        //{
        //    IHost host = new HostBuilder()
        //        .ConfigureLogging(
        //            loggingBuilder =>
        //            {
        //                loggingBuilder.AddProvider(loggerProvider);
        //            })
        //        .ConfigureWebJobs(
        //            webJobsBuilder =>
        //            {
        //                webJobsBuilder.AddAzureStorage();
        //            })
        //        .ConfigureServices(
        //            serviceCollection =>
        //            {
        //                ITypeLocator typeLocator = GetTypeLocator();
        //                serviceCollection.AddSingleton(typeLocator);
        //                serviceCollection.AddSingleton(nameResolver);
        //            })
        //        .Build();

        //    return (JobHost)host.Services.GetService<IJobHost>();
        //}



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
