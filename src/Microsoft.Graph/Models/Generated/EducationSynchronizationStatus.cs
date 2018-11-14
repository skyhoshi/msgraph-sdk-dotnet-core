// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.

// Template Source: Templates\CSharp\Model\EnumType.cs.tt


namespace Microsoft.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// The enum EducationSynchronizationStatus.
    /// </summary>
    [JsonConverter(typeof(EnumConverter))]
    public enum EducationSynchronizationStatus
    {
    
        /// <summary>
        /// paused
        /// </summary>
        Paused = 0,
	
        /// <summary>
        /// in Progress
        /// </summary>
        InProgress = 1,
	
        /// <summary>
        /// success
        /// </summary>
        Success = 2,
	
        /// <summary>
        /// error
        /// </summary>
        Error = 3,
	
        /// <summary>
        /// validation Error
        /// </summary>
        ValidationError = 4,
	
        /// <summary>
        /// quarantined
        /// </summary>
        Quarantined = 5,
	
        /// <summary>
        /// unknown Future Value
        /// </summary>
        UnknownFutureValue = 6,
	
    }
}