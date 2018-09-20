// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// **NOTE** This file was generated by a tool and any changes will be overwritten.

// Template Source: Templates\CSharp\Model\ComplexType.cs.tt

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The type WindowsUpdateScheduledInstall.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class WindowsUpdateScheduledInstall : WindowsUpdateInstallScheduleType
    {
    
        /// <summary>
        /// Gets or sets scheduledInstallDay.
        /// Scheduled Install Day in week. Possible values are: userDefined, everyday, sunday, monday, tuesday, wednesday, thursday, friday, saturday.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "scheduledInstallDay", Required = Newtonsoft.Json.Required.Default)]
        public WeeklySchedule? ScheduledInstallDay { get; set; }
    
        /// <summary>
        /// Gets or sets scheduledInstallTime.
        /// Scheduled Install Time during day
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "scheduledInstallTime", Required = Newtonsoft.Json.Required.Default)]
        public TimeOfDay ScheduledInstallTime { get; set; }
    
    }
}
