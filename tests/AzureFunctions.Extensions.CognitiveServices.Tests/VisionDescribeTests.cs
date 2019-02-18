using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    public class VisionDescribeTests
    {
        private static VisionDescribeModel visionDescribeUrlResult;
        private static VisionDescribeModel visionDescribeImageBytesResult;
        private static VisionDescribeModel visionDescribeImageBytesResizeResult;

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
                    builder.AddVisionDescribe();
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
            
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await RunTestAsync("VisionDescribeWithUrl", null);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeUrlResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionDescribeWithImageBytes()
        {
            
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await RunTestAsync("VisionDescribeWithImageBytes", null);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeImageBytesResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionDescribeWithImageWithResize()
        {
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await RunTestAsync("VisionDescribeWithTooBigImageBytesWithResize", null);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeImageBytesResizeResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionDescribeImageBytesTooLarge()
        {
           
            string exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => RunTestAsync("VisionDescribeWithTooBigImageBytes", null));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);

        }


        [Fact]
        public static async Task TestVisionDescribeMissingFile()
        {

            var exception = await Record.ExceptionAsync(() => RunTestAsync("VisionDescribeMissingFile", null));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);

        }

        private class VisionFunctions
        {

            public async Task VisionDescribeWithUrl(
                [VisionDescribe()]
                 VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();
                request.ImageUrl = "http://www.blah";

                var result = await client.DescribeAsync(request);

                visionDescribeUrlResult = result;
            }

            public async Task VisionDescribeWithImageBytes(
                [VisionDescribe()]
                 VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();
                request.ImageBytes = Resources.MockResults.SamplePhoto;

                var result = await client.DescribeAsync(request);

                visionDescribeImageBytesResult = result;
            }

            public async Task VisionDescribeWithTooBigImageBytes(
                [VisionDescribe(AutoResize=false)]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();
                request.AutoResize = false;
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.DescribeAsync(request);

            }

            public async Task VisionDescribeWithTooBigImageBytesWithResize(
                [VisionDescribe()]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.DescribeAsync(request);

                visionDescribeImageBytesResizeResult = result;

            }

            public async Task VisionDescribeMissingFile(
                [VisionDescribe()]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);

            }

            public async Task VisionDescribeKeyvault(
                [VisionDescribe()]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);

            }
        }
    }
}
