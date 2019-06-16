// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Middleware
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TelemetryHandlerTests : IDisposable
    {
        private MockRedirectHandler testHttpMessageHandler;
        private TelemetryHandler telemetryHandler;
        private HttpMessageInvoker invoker;
        private Version assemblyVersion;

        public TelemetryHandlerTests()
        {
            this.testHttpMessageHandler = new MockRedirectHandler();
            this.telemetryHandler = new TelemetryHandler(testHttpMessageHandler);
            this.invoker = new HttpMessageInvoker(this.telemetryHandler);
            this.assemblyVersion = typeof(TelemetryHandler).GetTypeInfo().Assembly.GetName().Version;
        }
        public void Dispose()
        {
            this.invoker.Dispose();
        }

        [Fact]
        public async Task TelemetryHandler_should_use_feature_flag_set_in_constructor()
        {
            using (TelemetryHandler handler = new TelemetryHandler(FeatureFlag.DefaultHttpProvider))
            using (HttpMessageInvoker messageInvoker = new HttpMessageInvoker(handler))
            {
                string expectedSdkVersionHeader = $"graph-dotnet/{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build} (featureUsage=00000008)";
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");

                HttpResponseMessage telemetryResponse = new HttpResponseMessage(HttpStatusCode.OK);
                this.testHttpMessageHandler.SetHttpResponse(telemetryResponse);
                handler.InnerHandler = this.testHttpMessageHandler;

                HttpResponseMessage response = await messageInvoker.SendAsync(httpRequestMessage, new CancellationToken());

                Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId));
                Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName));
                Assert.Equal(expectedSdkVersionHeader, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.SdkVersionHeaderName).First());
            }
        }

        [Fact]
        public async Task TelemetryHandler_should_use_existing_client_request_id_if_present()
        {
            string expectedSdkVersionHeader = $"graph-dotnet/{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
            string expectedClientRequestId = Guid.NewGuid().ToString();

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Add(CoreConstants.Headers.ClientRequestId, expectedClientRequestId);

            HttpResponseMessage telemetryResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this.testHttpMessageHandler.SetHttpResponse(telemetryResponse);

            HttpResponseMessage response = await this.invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId));
            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName));
            Assert.Equal(expectedClientRequestId, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.ClientRequestId).First());
            Assert.Equal(expectedSdkVersionHeader, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.SdkVersionHeaderName).First());
        }

        [Fact]
        public async Task TelemetryHandler_should_add_telemetry_headers_from_request_content()
        {
            GraphRequestContext requestContext = new GraphRequestContext
            {
                ClientRequestId = Guid.NewGuid().ToString(),
                FeatureUsage = FeatureFlag.AuthHandler | FeatureFlag.RetryHandler | FeatureFlag.RedirectHandler | FeatureFlag.DefaultHttpProvider
            };
            // graph-dotnet/1.14.0 (featureUsage=0000000F)
            string expectedSdkVersionHeader = $"graph-dotnet/{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build} (featureUsage={ Enum.Format(typeof(FeatureFlag), requestContext.FeatureUsage, "x")})";

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Properties.Add(typeof(GraphRequestContext).ToString(), requestContext);

            HttpResponseMessage telemetryResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this.testHttpMessageHandler.SetHttpResponse(telemetryResponse);

            HttpResponseMessage response = await this.invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId));
            Assert.True(response.RequestMessage.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName));
            Assert.Equal(requestContext.ClientRequestId, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.ClientRequestId).First());
            Assert.Equal(expectedSdkVersionHeader, response.RequestMessage.Headers.GetValues(CoreConstants.Headers.SdkVersionHeaderName).First());
        }
    }
}