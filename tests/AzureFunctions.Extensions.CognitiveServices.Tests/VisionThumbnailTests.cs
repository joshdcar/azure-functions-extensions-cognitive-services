using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
using FluentAssertions;
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

        [Fact]
        public static async Task TestVisionThumbnailWithUrl()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
  
            await TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithUrl");

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        [Fact]
        public static async Task TestVisionThumbnailWithImageBytes()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
           
            await TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithImageBytes");

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        [Fact]
        public static async Task TestVisionThumbnailWithImageWithResize()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            await TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithTooBigImageBytesWithResize");

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);

        }

        [Fact]
        public static async Task TestVisionThumbnailImageBytesTooLarge()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            string exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>
                        (client, "VisionFunctions.VisionThumbnailWithTooBigImageBytes"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);

        }

        [Fact]
        public static async Task TestVisionThumbnailMissingFile()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>
                        (client, "VisionFunctions.VisionThumbnailMissingFile"));

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
