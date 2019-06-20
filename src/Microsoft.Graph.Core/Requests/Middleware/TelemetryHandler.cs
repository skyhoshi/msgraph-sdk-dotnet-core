// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Diagnostics;
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

            // Append feature flag to SDK version if one exists.
            if (!requestContext.FeatureUsage.Equals(FeatureFlag.None))
            {
                string featureUsage = Enum.Format(typeof(FeatureFlag), requestContext.FeatureUsage, "x");
                sdkVersionHeaderValue = $"{sdkVersionHeaderValue} ({CoreConstants.Headers.FeatureUsage}={featureUsage})";
            }

            var found = httpRequestMessage.Headers.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out var findee);
            var contains = findee?.Contains(sdkVersionHeaderValue);

            // Add sdk version header without duplicates.
            if (!(httpRequestMessage.Headers.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out var existingSdkHeader) && existingSdkHeader.Contains(sdkVersionHeaderValue)))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(CoreConstants.Headers.SdkVersionHeaderName, sdkVersionHeaderValue);
            }

            if (!httpRequestMessage.Headers.Contains("HostOS"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("HostOS", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
            }

            if (!httpRequestMessage.Headers.Contains("RuntimeEnvironment"))
            {
                string runtimeFramrwork = string.Empty;
#if NETSTANDARD1_1
                runtimeFramrwork = TargetAssembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
#else
                runtimeFramrwork = (Assembly.GetEntryAssembly() ?? TargetAssembly).GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
#endif
                // Format runtimeFramework from .NETStandard,Version=v1.1 to .NETStandard/1.1.
                int targetIndex = runtimeFramrwork.IndexOf(",");
                if (targetIndex != -1)
                {
                    // Substitute ",Version=v" with "/".
                    httpRequestMessage.Headers.TryAddWithoutValidation("RuntimeEnvironment", $"{runtimeFramrwork.Substring(0, targetIndex)}/{runtimeFramrwork.Substring(targetIndex + 10)}");
                }
            }
            return base.SendAsync(httpRequestMessage, cancellationToken);
        }
    }
}
