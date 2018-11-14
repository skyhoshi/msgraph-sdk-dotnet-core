// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.

// Template Source: Templates\CSharp\Requests\IEntityCollectionWithReferencesRequestBuilder.cs.tt
namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The interface IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionWithReferencesRequestBuilder.
    /// </summary>
    public partial interface IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionWithReferencesRequestBuilder
    {
        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <returns>The built request.</returns>
        IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionWithReferencesRequest Request();

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>The built request.</returns>
        IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionWithReferencesRequest Request(IEnumerable<Option> options);

        /// <summary>
        /// Gets an <see cref="IIosTrustedRootCertificateWithReferenceRequestBuilder"/> for the specified IosTrustedRootCertificate.
        /// </summary>
        /// <param name="id">The ID for the IosTrustedRootCertificate.</param>
        /// <returns>The <see cref="IIosTrustedRootCertificateWithReferenceRequestBuilder"/>.</returns>
        IIosTrustedRootCertificateWithReferenceRequestBuilder this[string id] { get; }
        
        /// <summary>
        /// Gets an <see cref="IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionReferencesRequestBuilder"/> for the references in the collection.
        /// </summary>
        /// <returns>The <see cref="IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionReferencesRequestBuilder"/>.</returns>
        IIosEnterpriseWiFiConfigurationRootCertificatesForServerValidationCollectionReferencesRequestBuilder References { get; }

    }
}