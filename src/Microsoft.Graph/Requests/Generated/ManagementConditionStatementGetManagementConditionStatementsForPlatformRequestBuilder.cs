// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.

// Template Source: Templates\CSharp\Requests\MethodRequestBuilder.cs.tt

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// The type ManagementConditionStatementGetManagementConditionStatementsForPlatformRequestBuilder.
    /// </summary>
    public partial class ManagementConditionStatementGetManagementConditionStatementsForPlatformRequestBuilder : BaseFunctionMethodRequestBuilder<IManagementConditionStatementGetManagementConditionStatementsForPlatformRequest>, IManagementConditionStatementGetManagementConditionStatementsForPlatformRequestBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="ManagementConditionStatementGetManagementConditionStatementsForPlatformRequestBuilder"/>.
        /// </summary>
        /// <param name="requestUrl">The URL for the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="platform">A platform parameter for the OData method call.</param>
        public ManagementConditionStatementGetManagementConditionStatementsForPlatformRequestBuilder(
            string requestUrl,
            IBaseClient client,
            DevicePlatformType platform)
            : base(requestUrl, client)
        {
            this.SetParameter("platform", platform, false);
        }

        /// <summary>
        /// A method used by the base class to construct a request class instance.
        /// </summary>
        /// <param name="functionUrl">The request URL to </param>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>An instance of a specific request class.</returns>
        protected override IManagementConditionStatementGetManagementConditionStatementsForPlatformRequest CreateRequest(string functionUrl, IEnumerable<Option> options)
        {
            var request = new ManagementConditionStatementGetManagementConditionStatementsForPlatformRequest(functionUrl, this.Client, options);

            return request;
        }
    }
}