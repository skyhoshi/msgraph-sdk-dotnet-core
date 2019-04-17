// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class TelemetryHandler: DelegatingHandler
    {
        private readonly FeatureFlag defaultFeatureFlag;
        private readonly Version assemblyVersion;

        /// <summary>
        /// 
        /// </summary>
        public TelemetryHandler()
        {
            assemblyVersion = typeof(TelemetryHandler).GetTypeInfo().Assembly.GetName().Version;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerHandler"></param>
        public TelemetryHandler(HttpMessageHandler innerHandler)
            :this()
        {
            this.InnerHandler = innerHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureFlag"></param>
        internal TelemetryHandler(FeatureFlag featureFlag)
            :this()
        {
            defaultFeatureFlag = featureFlag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            string sdkVersionHeaderValue = string.Format(
                    CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                    "graph",
                    assemblyVersion.Major,
                    assemblyVersion.Minor,
                    assemblyVersion.Build);

            // Get graph request context from incoming request.
            GraphRequestContext requestContext = httpRequest.GetRequestContext();
            requestContext.FeatureUsage |= defaultFeatureFlag;
            
            // Only add client-request-id header when non is present.
            if (!httpRequest.Headers.Contains(CoreConstants.Headers.ClientRequestId))
                httpRequest.Headers.TryAddWithoutValidation(CoreConstants.Headers.ClientRequestId, requestContext.ClientRequestId);

            // Prepend feature usage if one is set.
            if (!requestContext.FeatureUsage.Equals(FeatureFlag.None))
            {
                string featureUsage = Enum.Format(typeof(FeatureFlag), requestContext.FeatureUsage, "x");
                sdkVersionHeaderValue = $"{sdkVersionHeaderValue} ({CoreConstants.Headers.FeatureUsage}={featureUsage})";
            }

            // Add sdk version header while avoiding duplicates.
            if (!(httpRequest.Headers.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out var existingSdkHeader) && !existingSdkHeader.Contains(sdkVersionHeaderValue)))
                httpRequest.Headers.TryAddWithoutValidation(CoreConstants.Headers.SdkVersionHeaderName, sdkVersionHeaderValue);

            return base.SendAsync(httpRequest, cancellationToken);
        }
    }
}
