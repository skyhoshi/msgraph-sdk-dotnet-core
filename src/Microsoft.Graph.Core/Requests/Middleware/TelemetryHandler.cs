// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
namespace Microsoft.Graph
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that handles capturing of the .NET SDK telemetry.
    /// </summary>
    internal class TelemetryHandler: DelegatingHandler
    {
        internal readonly FeatureFlag DefaultFeatureFlag;
        private readonly string SdkHeaderVersion;
        private readonly string HostOS;
        private readonly string RuntimeEnvironment;

        /// <summary>
        /// Constructs a new <see cref="TelemetryHandler"/>.
        /// </summary>
        public TelemetryHandler()
        {
            Assembly coreAssembly = typeof(TelemetryHandler).GetTypeInfo().Assembly;
            Assembly modelAssembly = GetAssembly("Microsoft.Graph") ?? GetAssembly("Microsoft.Graph.Beta");
            Assembly authAssembly = GetAssembly("Microsoft.Graph.Auth");

            // Compose SdkHeaderVersion in the order of {modelSdkVersion}, {authSdkVersion}, {coreSdkVersion}.
            SdkHeaderVersion = GetSDKVersionHeaderValue(modelAssembly, authAssembly, coreAssembly);

            HostOS = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

#if NETSTANDARD1_1
            RuntimeEnvironment = FormartRuntimeEnvironment(coreAssembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
#else
            RuntimeEnvironment = FormartRuntimeEnvironment((Assembly.GetEntryAssembly() ?? coreAssembly).GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
#endif
        }

        /// <summary>
        /// Constructs a new <see cref="TelemetryHandler"/>.
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        public TelemetryHandler(HttpMessageHandler innerHandler)
            : this()
        {
            this.InnerHandler = innerHandler;
        }

        /// <summary>
        /// Constructs a new <see cref="TelemetryHandler"/>.
        /// </summary>
        /// <param name="featureFlag">Extra <see cref="FeatureFlag"/> to register when instantiating a <see cref="TelemetryHandler"/>.</param>
        internal TelemetryHandler(FeatureFlag featureFlag)
            :this()
        {
            DefaultFeatureFlag = featureFlag;
        }

        /// <summary>
        /// Sends a HTTP request to the next <see cref="DelegatingHandler"/> in the pipeline.
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> to be send.</param>>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            // Get graph request context from incoming request.
            GraphRequestContext requestContext = httpRequestMessage.GetRequestContext();
            requestContext.FeatureUsage |= DefaultFeatureFlag;

            // Only add client-request-id header when non is present.
            if (!httpRequestMessage.Headers.Contains(CoreConstants.Headers.ClientRequestId))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(CoreConstants.Headers.ClientRequestId, requestContext.ClientRequestId);
            }
            string sdkVersionValues = SdkHeaderVersion;

            // Append feature flag to core SDK version if one exists.
            if (!requestContext.FeatureUsage.Equals(FeatureFlag.None))
            {
                string featureUsage = Enum.Format(typeof(FeatureFlag), requestContext.FeatureUsage, "x");
                sdkVersionValues = $"{SdkHeaderVersion} ({CoreConstants.Headers.FeatureUsage}={featureUsage})";
            }

            // Add SDK version header without duplicates.
            if (!(httpRequestMessage.Headers.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out var existingSdkHeader) && existingSdkHeader.Contains(sdkVersionValues)))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(CoreConstants.Headers.SdkVersionHeaderName, sdkVersionValues);
            }

            if (!httpRequestMessage.Headers.Contains("HostOS"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("HostOS", HostOS);
            }

            if (!httpRequestMessage.Headers.Contains("RuntimeEnvironment"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("RuntimeEnvironment", RuntimeEnvironment);
            }
            return base.SendAsync(httpRequestMessage, cancellationToken);
        }

        /// <summary>
        /// Gets the service model <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assemblyName">Name of the <see cref="Assembly"/> to load.</param>
        /// <returns><see cref="Assembly"/>.</returns>
        internal Assembly GetAssembly(string assemblyName)
        {
            Assembly modelAssembly = null;
            try
            {
                modelAssembly = Assembly.Load(new AssemblyName(assemblyName));
            }
            catch (FileNotFoundException)
            {
            }
            return modelAssembly;
        }

        /// <summary>
        /// Formats a runtime environment string from ".NETStandard,Version=v1.1" to ".NETStandard/1.1".
        /// </summary>
        /// <param name="runtimeEnvrironment">Runtime string to format.</param>
        /// <returns>A formated runtime environment string.</returns>
        private string FormartRuntimeEnvironment(string runtimeEnvrironment)
        {
            string formatedOutput = runtimeEnvrironment;
            int targetIndex = runtimeEnvrironment.IndexOf(",");
            if (targetIndex != -1)
            {
                // Substitute ",Version=v" with "/".
                formatedOutput = $"{runtimeEnvrironment.Substring(0, targetIndex)}/{runtimeEnvrironment.Substring(targetIndex + 10)}";
            }
            return formatedOutput;
        }

        /// <summary>
        /// Gets a complete set of .NET SDKs versions in the order of {modelSdkVersion}, {authSdkVersion}, {coreSdkVersion}.
        /// </summary>
        /// <param name="modelAssembly">Service model library assembly.</param>
        /// <param name="authAssembly">Auth library assembly.</param>
        /// <param name="coreAssembly">Core library assembly.</param>
        /// <returns>{modelSdkVersion}, {authSdkVersion}, {coreSdkVersion}</returns>
        private string GetSDKVersionHeaderValue(Assembly modelAssembly, Assembly authAssembly, Assembly coreAssembly)
        {
            string composedSDKVersions = string.Empty;
            if (modelAssembly != null)
            {
                string modelSdkVersion = string.Format(
                   CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                   string.Empty,
                   modelAssembly.GetName().Version.Major,
                   modelAssembly.GetName().Version.Minor,
                   modelAssembly.GetName().Version.Build);

                // Add model SDK version.
                composedSDKVersions += modelSdkVersion;
            }

            if (authAssembly != null)
            {
                string authSdkVersion = string.Format(
                   CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                   "-auth",
                   authAssembly.GetName().Version.Major,
                   authAssembly.GetName().Version.Minor,
                   authAssembly.GetName().Version.Build);

                // Append auth SDK version.
                composedSDKVersions += $", {authSdkVersion}";
            }

            string coreSdkVersion = string.Format(
                   CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                   "-core",
                   coreAssembly.GetName().Version.Major,
                   coreAssembly.GetName().Version.Minor,
                   coreAssembly.GetName().Version.Build);

            if (string.IsNullOrEmpty(composedSDKVersions))
            {
                composedSDKVersions = coreSdkVersion;
            }
            else
            {
                // Append core SDK version.
                composedSDKVersions += $", {coreSdkVersion}";
            }

            // Returns {modelSdkVersion}, {authSdkVersion}, {coreSdkVersion}
            return composedSDKVersions;
        }
    }
}
