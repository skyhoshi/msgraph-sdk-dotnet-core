﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// Options class used to configure the authentication providers.
    /// </summary>
    internal class ScopedAuthenticationProviderOptions : IAuthenticationProviderOption
    {

        /// <summary>
        /// Scopes to use when authenticating.
        /// </summary>
        public string[] Scopes { get; set; }

    }
}