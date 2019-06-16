// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TelemetryHandler: DelegatingHandler
    {
        internal readonly FeatureFlag DefaultFeatureFlag;
        private readonly Assembly TargetAssembly;

        public TelemetryHandler()
        {
            TargetAssembly = typeof(TelemetryHandler).GetTypeInfo().Assembly;
        }

        public TelemetryHandler(HttpMessageHandler innderHandler)
            : this()
        {
            this.InnerHandler = innderHandler;
        }

        internal TelemetryHandler(FeatureFlag featureFlag)
            :this()
        {
            DefaultFeatureFlag = featureFlag;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            string sdkVersionHeaderValue = string.Format(
                   CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                   "graph",
                   TargetAssembly.GetName().Version.Major,
                   TargetAssembly.GetName().Version.Minor,
                   TargetAssembly.GetName().Version.Build);

            // Get graph request context from incoming request.
            GraphRequestContext requestContext = httpRequestMessage.GetRequestContext();
            requestContext.FeatureUsage |= DefaultFeatureFlag;

            // Only add client-request-id header when non is present.
            if (!httpRequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(CoreConstants.Headers.ClientRequestId, requestContext.ClientRequestId);
            }

            // Prepend feature usage if one is set.
            if (!requestContext.FeatureUsage.Equals(FeatureFlag.None))
            {
                string featureUsage = Enum.Format(typeof(FeatureFlag), requestContext.FeatureUsage, "x");
                sdkVersionHeaderValue = $"{sdkVersionHeaderValue} ({CoreConstants.Headers.FeatureUsage}={featureUsage})";
            }

            // Add sdk version header without duplicates.
            if (!(httpRequestMessage.Headers.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out var existingSdkHeader) && !existingSdkHeader.Contains(sdkVersionHeaderValue)))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(CoreConstants.Headers.SdkVersionHeaderName, sdkVersionHeaderValue);
            }

            // Add OS + Framework stats.
#if NET45

            if (httpRequestMessage.Headers.Contains("x-client-OS"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("x-client-OS", Environment.OSVersion.ToString());
            }
            if (httpRequestMessage.Headers.Contains("x-framework-Name"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("x-framework-Name", Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
            }
#else
            if (httpRequestMessage.Headers.Contains("x-client-OS"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("x-client-OS", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
            }

            if (httpRequestMessage.Headers.Contains("x-framework-Name"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("x-framework-Name", TargetAssembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
            }
#endif
            return base.SendAsync(httpRequestMessage, cancellationToken);
        }
    }
}
