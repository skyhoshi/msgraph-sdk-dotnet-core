// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
#if NETCOREAPP3_1

    using Azure.Core;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using System;
    using Microsoft.Identity.Web;

    /// <summary>
    /// An AuthProvider to handle instances of <see cref="TokenCredential"/> from Azure.Core and Azure.Identity
    /// </summary>
    public class TokenAcquisitionAuthProvider : IAuthenticationProvider
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IEnumerable<string> _scopes;

        /// <summary>
        /// An AuthProvider to handle instances of <see cref="ITokenAcquisition"/> from Microsoft.Identity.Web
        /// </summary>
        /// <param name="tokenAcquisition">The <see cref="ITokenAcquisition"/> to use for authentication</param>
        /// <param name="scopes">Scopes required to access Microsoft Graph. This defaults to https://graph.microsoft.com/.default when none is set.</param>
        /// <exception cref="ArgumentException"> When a null <see cref="TokenCredential"/> is passed</exception>
        public TokenAcquisitionAuthProvider(ITokenAcquisition tokenAcquisition, IEnumerable<string> scopes = null)
        {
            _tokenAcquisition = tokenAcquisition ?? throw new ArgumentException(
                                    string.Format(ErrorConstants.Messages.NullParameter, nameof(tokenAcquisition)),
                                    nameof(tokenAcquisition));
            _scopes = scopes ?? new List<string> { AuthConstants.DefaultScopeUrl };
        }

        /// <summary>
        /// Adds an authentication header to the incoming request by checking using the <see cref="TokenCredential"/> provided
        /// during the creation of this class
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate</param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            //First try to read the scopes off the requestContext.
            MsalAuthenticationProviderOption msalAuthProviderOption = request.GetMsalAuthProviderOption();
            string token = await _tokenAcquisition.GetAccessTokenForUserAsync(msalAuthProviderOption.Scopes ?? this._scopes);
            request.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, token);
        }
    }

#endif
}
