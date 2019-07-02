// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Middleware
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TelemetryHandlerTests : IDisposable
    {
        private MockRedirectHandler testHttpMessageHandler;
        private TelemetryHandler telemetryHandler;
        private HttpMessageInvoker invoker;
        private Version coreAssemblyVersion;
        private Version modelAssemblyVersion;

        public TelemetryHandlerTests()
        {
            testHttpMessageHandler = new MockRedirectHandler();
            telemetryHandler = new TelemetryHandler(testHttpMessageHandler);
            invoker = new HttpMessageInvoker(telemetryHandler);
            coreAssemblyVersion = typeof(TelemetryHandler).GetTypeInfo().Assembly.GetName().Version;
            modelAssemblyVersion = (telemetryHandler.GetAssembly("Microsoft.Graph") ?? telemetryHandler.GetAssembly("Microsoft.Graph.Beta")).GetName().Version;
        }
        public void Dispose()
        {
            invoker.Dispose();
        }

        [Fact]
        public async Task TelemetryHandler_Should_Use_Feature_Flag_Set_On_The_Constructor()
        {
            using (TelemetryHandler handler = new TelemetryHandler(FeatureFlag.DefaultHttpProvider))
            using (HttpMessageInvoker messageInvoker = new HttpMessageInvoker(handler))
            {
                string expectedCoreSdkVersion = $"graph-dotnet-core/{coreAssemblyVersion.Major}.{coreAssemblyVersion.Minor}.{coreAssemblyVersion.Build} (featureUsage=00000008)";
                string expectedModelSdkVersion = $"graph-dotnet/{modelAssemblyVersion.Major}.{modelAssemblyVersion.Minor}.{modelAssemblyVersion.Build}";
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");

                HttpResponseMessage telemetryResponse = new HttpResponseMessage(HttpStatusCode.OK);
                testHttpMessageHandler.SetHttpResponse(telemetryResponse);
                handler.InnerHandler = testHttpMessageHandler;

                HttpResponseMessage response = await messageInvoker.SendAsync(httpRequestMessage, new CancellationToken());

                Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId));
                Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName));
                Assert.Equal($"{expectedModelSdkVersion}, {expectedCoreSdkVersion}", response.RequestMessage.Headers.GetValues(CoreConstants.Headers.SdkVersionHeaderName).First());
            }
        }

        [Fact]
        public async Task TelemetryHandler_Should_Use_Existing_Client_Request_Id_If_Present()
        {
            string expectedCoreSdkVersion = $"graph-dotnet-core/{coreAssemblyVersion.Major}.{coreAssemblyVersion.Minor}.{coreAssemblyVersion.Build}";
            string expectedModelSdkVersion = $"graph-dotnet/{modelAssemblyVersion.Major}.{modelAssemblyVersion.Minor}.{modelAssemblyVersion.Build}";
            string expectedClientRequestId = Guid.NewGuid().ToString();

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Add(CoreConstants.Headers.ClientRequestId, expectedClientRequestId);

            HttpResponseMessage telemetryResponse = new HttpResponseMessage(HttpStatusCode.OK);
            testHttpMessageHandler.SetHttpResponse(telemetryResponse);

            HttpResponseMessage response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId));
            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName));
            Assert.Equal(expectedClientRequestId, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.ClientRequestId).First());
            Assert.Equal($"{expectedModelSdkVersion}, {expectedCoreSdkVersion}", response.RequestMessage.Headers.GetValues(CoreConstants.Headers.SdkVersionHeaderName).First());
        }

        [Fact]
        public async Task TelemetryHandler_Should_Add_Telemetry_Headers_From_Request_Content()
        {
            GraphRequestContext requestContext = new GraphRequestContext
            {
                ClientRequestId = Guid.NewGuid().ToString(),
                FeatureUsage = FeatureFlag.AuthHandler | FeatureFlag.RetryHandler | FeatureFlag.RedirectHandler | FeatureFlag.DefaultHttpProvider
            };
            string expectedCoreSdkVersion = $"graph-dotnet-core/{coreAssemblyVersion.Major}.{coreAssemblyVersion.Minor}.{coreAssemblyVersion.Build} (featureUsage={ Enum.Format(typeof(FeatureFlag), requestContext.FeatureUsage, "x")})";
            string expectedModelSdkVersion = $"graph-dotnet/{modelAssemblyVersion.Major}.{modelAssemblyVersion.Minor}.{modelAssemblyVersion.Build}";

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Properties.Add(typeof(GraphRequestContext).ToString(), requestContext);

            HttpResponseMessage telemetryResponse = new HttpResponseMessage(HttpStatusCode.OK);
            testHttpMessageHandler.SetHttpResponse(telemetryResponse);

            HttpResponseMessage response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId));
            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName));
            Assert.Equal(requestContext.ClientRequestId, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.ClientRequestId).First());
            Assert.Equal($"{expectedModelSdkVersion}, {expectedCoreSdkVersion}", response.RequestMessage.Headers.GetValues(CoreConstants.Headers.SdkVersionHeaderName).First());
        }

        [Fact]
        public void GetAssembly_Should_Load_Referenced_Assembly()
        {
            Assembly coreAssembly = telemetryHandler.GetAssembly("Microsoft.Graph");

            Assert.NotNull(coreAssembly);
            Assert.Equal("Microsoft.Graph", coreAssembly.GetName().Name);
        }

        [Fact]
        public void GetAssembly_Should_Not_Load_UnReferenced_Assembly()
        {
            Assembly authAssembly = telemetryHandler.GetAssembly("Microsoft.Graph.Auth");

            Assert.Null(authAssembly);
        }
    }
}