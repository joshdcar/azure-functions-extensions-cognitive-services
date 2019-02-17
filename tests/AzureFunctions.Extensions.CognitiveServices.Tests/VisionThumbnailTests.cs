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
    public class VisionThumbnailTests
    {
        private static byte[] visionThumbnailResult;

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
                    builder.AddVisionThumbnail();
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
        public static async Task TestVisionThumbnailWithUrl()
        {
            await RunTestAsync("VisionThumbnailWithUrl", null);

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        [Fact]
        public static async Task TestVisionThumbnailWithImageBytes()
        {
            await RunTestAsync("VisionThumbnailWithImageBytes", null);

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        [Fact]
        public static async Task TestVisionThumbnailWithImageWithResize()
        {
            await RunTestAsync("VisionThumbnailWithTooBigImageBytesWithResize", null);

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);

        }

        [Fact]
        public static async Task TestVisionThumbnailImageBytesTooLarge()
        {
           
            string exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => RunTestAsync("VisionThumbnailWithTooBigImageBytes", null));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);

        }

        [Fact]
        public static async Task TestVisionThumbnailMissingFile()
        {

            var exception = await Record.ExceptionAsync(() => RunTestAsync("VisionThumbnailMissingFile", null));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);

        }

        private class VisionFunctions
        {

            public async Task VisionThumbnailWithUrl(
                [VisionThumbnail( Width ="100", Height="100")]
                 VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest();
                request.ImageUrl = "http://www.blah";

                var result = await client.ThumbnailAsync(request);

                visionThumbnailResult = result;
            }

            public async Task VisionThumbnailWithImageBytes(
                [VisionThumbnail(Width ="100", Height="100")]
                 VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest();
                request.ImageBytes = MockResults.SamplePhoto;

                var result = await client.ThumbnailAsync(request);

                visionThumbnailResult = result;
            }

            public async Task VisionThumbnailWithTooBigImageBytes(
                [VisionThumbnail(AutoResize = false,  Width ="100", Height="100")]
                 VisionThumbnailClient client)
            {

                var request = new VisionThumbnailRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.ThumbnailAsync(request);

            }

            public async Task VisionThumbnailWithTooBigImageBytesWithResize(
                [VisionThumbnail(AutoResize = true, Width ="100", Height="100")]
                 VisionThumbnailClient client)
            {

                var request = new VisionThumbnailRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.ThumbnailAsync(request);

                visionThumbnailResult = result;

            }

            public async Task VisionThumbnailMissingFile(
                [VisionThumbnail(Width ="100", Height="100")]
                 VisionThumbnailClient client)
            {

                var request = new VisionThumbnailRequest();

                var result = await client.ThumbnailAsync(request);

            }

            public async Task VisionThumbnailKeyvault(
                [VisionThumbnail(Width ="100", Height="100")]
                 VisionThumbnailClient client)
            {

                var request = new VisionThumbnailRequest();

                var result = await client.ThumbnailAsync(request);

            }
        }
    }
}
