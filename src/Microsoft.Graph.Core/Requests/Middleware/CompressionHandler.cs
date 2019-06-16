// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using System.IO.Compression;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that handles compression.
    /// </summary>
    public class CompressionHandler: DelegatingHandler
    {
        /// <summary>
        /// Constructs a new <see cref="CompressionHandler"/>.
        /// </summary>
        public CompressionHandler()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CompressionHandler"/>.
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        public CompressionHandler(HttpMessageHandler innerHandler)
            :this()
        {
            InnerHandler = innerHandler;
        }

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            StringWithQualityHeaderValue gzipQHeaderValue = new StringWithQualityHeaderValue(CoreConstants.Encoding.GZip);

            // Add Accept-encoding: gzip header to incoming request if it doesn't have one.
            if (!httpRequestMessage.Headers.AcceptEncoding.Contains(gzipQHeaderValue))
            {
                httpRequestMessage.Headers.AcceptEncoding.Add(gzipQHeaderValue);
            }

            httpRequestMessage.SetFeatureFlag(FeatureFlag.CompressionHandler);
            HttpResponseMessage response = await base.SendAsync(httpRequestMessage, cancellationToken);

            // Decompress response content when Content-Encoding: gzip header is present.
            if (ShouldDecompressContent(response))
            {
                response.Content = new StreamContent(new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress));
            }

            return response;
        }

        /// <summary>
        /// Checks if a <see cref="HttpResponseMessage"/> contains a Content-Encoding: gzip header.
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponseMessage"/> to check for header.</param>
        /// <returns></returns>
        private bool ShouldDecompressContent(HttpResponseMessage httpResponse)
        {
            return httpResponse.Content != null && httpResponse.Content.Headers.ContentEncoding.Contains(CoreConstants.Encoding.GZip);
        }
    }
}
