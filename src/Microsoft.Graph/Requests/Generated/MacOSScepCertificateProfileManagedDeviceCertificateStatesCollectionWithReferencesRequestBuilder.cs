// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.

// Template Source: Templates\CSharp\Requests\EntityCollectionWithReferencesRequestBuilder.cs.tt
namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The type MacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequestBuilder.
    /// </summary>
    public partial class MacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequestBuilder : BaseRequestBuilder, IMacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequestBuilder
    {

        /// <summary>
        /// Constructs a new MacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequestBuilder.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        public MacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequestBuilder(
            string requestUrl,
            IBaseClient client)
            : base(requestUrl, client)
        {
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <returns>The built request.</returns>
        public IMacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequest Request()
        {
            return this.Request(null);
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>The built request.</returns>
        public IMacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequest Request(IEnumerable<Option> options)
        {
            return new MacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionWithReferencesRequest(this.RequestUrl, this.Client, options);
        }

        /// <summary>
        /// Gets an <see cref="IManagedDeviceCertificateStateWithReferenceRequestBuilder"/> for the specified MacOSScepCertificateProfileManagedDeviceCertificateState.
        /// </summary>
        /// <param name="id">The ID for the MacOSScepCertificateProfileManagedDeviceCertificateState.</param>
        /// <returns>The <see cref="IManagedDeviceCertificateStateWithReferenceRequestBuilder"/>.</returns>
        public IManagedDeviceCertificateStateWithReferenceRequestBuilder this[string id]
        {
            get
            {
                return new ManagedDeviceCertificateStateWithReferenceRequestBuilder(this.AppendSegmentToRequestUrl(id), this.Client);
            }
        }

        
        /// <summary>
        /// Gets an <see cref="IMacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionReferencesRequestBuilder"/> for the references in the collection.
        /// </summary>
        /// <returns>The <see cref="IMacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionReferencesRequestBuilder"/>.</returns>
        public IMacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionReferencesRequestBuilder References
        {
            get
            {
                return new MacOSScepCertificateProfileManagedDeviceCertificateStatesCollectionReferencesRequestBuilder(this.AppendSegmentToRequestUrl("$ref"), this.Client);
            }
        }

    }
}