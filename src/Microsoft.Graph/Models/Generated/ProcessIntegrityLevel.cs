// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.

// Template Source: Templates\CSharp\Model\EnumType.cs.tt


namespace Microsoft.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// The enum ProcessIntegrityLevel.
    /// </summary>
    [JsonConverter(typeof(EnumConverter))]
    public enum ProcessIntegrityLevel
    {
    
        /// <summary>
        /// unknown
        /// </summary>
        Unknown = 0,
	
        /// <summary>
        /// untrusted
        /// </summary>
        Untrusted = 1,
	
        /// <summary>
        /// low
        /// </summary>
        Low = 2,
	
        /// <summary>
        /// medium
        /// </summary>
        Medium = 3,
	
        /// <summary>
        /// high
        /// </summary>
        High = 4,
	
        /// <summary>
        /// system
        /// </summary>
        System = 5,
	
        /// <summary>
        /// unknown Future Value
        /// </summary>
        UnknownFutureValue = 127,
	
    }
}
