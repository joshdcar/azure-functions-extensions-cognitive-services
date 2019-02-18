
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Tests.Common;
using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{

    public class VisionAnalysisTests
    {

        private static VisionAnalysisModel visionAnalysisUrlResult;
        private static VisionAnalysisModel visionAnalysisImageBytesResult;
        private static VisionAnalysisModel visionAnalysisImageBytesResizeResult;

        private static readonly TestLoggerProvider _loggerProvider = new TestLoggerProvider();
        

        private static async Task RunTestAsync(string testName, object argument = null)
        {
            Type testType = typeof(VisionFunctions);
            var locator = new ExplicitTypeLocator(testType);
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(_loggerProvider);
            ICognitiveServicesClient testCognitiveServicesClient = new TestCognitiveServicesClient();

            var arguments = new Dictionary<string, object>();
            var resolver = new TestNameResolver();

            IHost host = new HostBuilder()
                .ConfigureWebJobs(builder =>
                {
                    builder.AddVisionAnalysis();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICognitiveServicesClient>(testCognitiveServicesClient);
                    services.AddSingleton<INameResolver>(resolver);
                    services.AddSingleton<ITypeLocator>(locator);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(_loggerProvider);
                })
                .ConfigureAppConfiguration(c =>
                {
                    c.Sources.Clear();

                    var collection = new Dictionary<string, string>
                    {
                        { "VisionKey", "1234XYZ" },
                        { "VisionUrl", "http://url" }
                    };

                    c.AddInMemoryCollection(collection);
                })
                .Build();

            var method = testType.GetMethod(testName);

            await host.GetJobHost().CallAsync(method, arguments);
        }


        [Fact]
        public static async Task TestVisionAnalysisWithUrl()
        {
           
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await RunTestAsync("VisionAnalysisWithUrl", null);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisUrlResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithImageBytes()
        {
            
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await RunTestAsync("VisionAnalysisWithImageBytes", null);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisImageBytesResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithImageWithResize()
        {
            
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await RunTestAsync("VisionAnalysisWithTooBigImageBytesWithResize", null);
           
            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisImageBytesResizeResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisImageBytesTooLarge()
        {
           
            string exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => RunTestAsync("VisionAnalysisWithTooBigImageBytes", null));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);

        }

        [Fact]
        public static async Task TestVisionAnalysisMissingFile()
        {

            var exception = await Record.ExceptionAsync(() => RunTestAsync("VisionAnalysisMissingFile", null));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);

        }

        private class VisionFunctions
        {

            public static async Task VisionAnalysisWithUrl(
                [VisionAnalysis()] VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();
                request.ImageUrl = "http://www.blah";

                var result = await client.AnalyzeAsync(request);

                visionAnalysisUrlResult = result;
            }

            public static async Task VisionAnalysisWithImageBytes(
                [VisionAnalysis()]
                 VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();
                request.ImageBytes = MockResults.SamplePhoto;

                var result = await client.AnalyzeAsync(request);

                visionAnalysisImageBytesResult = result;
            }

            public static async Task VisionAnalysisWithTooBigImageBytes(
                [VisionAnalysis(AutoResize = false)]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();

                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.AnalyzeAsync(request);

            }

            public static async Task VisionAnalysisWithTooBigImageBytesWithResize(
                [VisionAnalysis(AutoResize = true)]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.AnalyzeAsync(request);

                visionAnalysisImageBytesResizeResult = result;

            }

            public static async Task VisionAnalysisMissingFile(
                [VisionAnalysis()]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();

                var result = await client.AnalyzeAsync(request);

            }

            public static async Task VisionAnalysisKeyvault(
                [VisionAnalysis()]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();

                var result = await client.AnalyzeAsync(request);

            }
        }

    }

     
   
}
