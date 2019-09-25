// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Tasks
{
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;
    using Microsoft.Graph;
    using System.Net.Http;
    using System.Diagnostics;
    using Microsoft.Graph.Core.Tasks;

    public class HttpPageIteratorTests
    {

        [Fact]
        public async void IterateOverMessage()
        {
            var authProvider = new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                var token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkxXS1lWRmlvVHVyWHdHeWIwd3QzQTVoRHVNb1FWZVMxRnJxd3UxaGlCVlUiLCJhbGciOiJSUzI1NiIsIng1dCI6ImFQY3R3X29kdlJPb0VOZzNWb09sSWgydGlFcyIsImtpZCI6ImFQY3R3X29kdlJPb0VOZzNWb09sSWgydGlFcyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTY5MzY1Mjg5LCJuYmYiOjE1NjkzNjUyODksImV4cCI6MTU2OTM2OTE4OSwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFVUUF1LzhNQUFBQWFSTlhzbWQreUR2ZERtMkg0TVBxWjdXSFhRSW9keE1oZHcyNWFSY2VwNitiL3JXbUl1SjJENWMyUHIrd2trMXFOUjRiM3JqVUpUMFlUUGUvQ08rUGZRPT0iLCJhbXIiOlsibWZhIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIGV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJmYW1pbHlfbmFtZSI6Ik1haW5lciIsImdpdmVuX25hbWUiOiJNaWNoYWVsIiwiaXBhZGRyIjoiMTMxLjEwNy4xNzQuMTc2IiwibmFtZSI6Ik1pY2hhZWwgTWFpbmVyIiwib2lkIjoiZDA5ZDBlZGItOTRjNC00OTQ4LWE5ZTQtNWRkOTIzNzc3ZDRiIiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTIxMjc1MjExODQtMTYwNDAxMjkyMC0xODg3OTI3NTI3LTIyNzY0NDUiLCJwbGF0ZiI6IjMiLCJwdWlkIjoiMTAwM0JGRkQ4MDI4NDU0RCIsInJoIjoiSSIsInNjcCI6IkNhbGVuZGFycy5SZWFkV3JpdGUgQ29udGFjdHMuUmVhZFdyaXRlIERldmljZU1hbmFnZW1lbnRBcHBzLlJlYWQuQWxsIERldmljZU1hbmFnZW1lbnRBcHBzLlJlYWRXcml0ZS5BbGwgRGV2aWNlTWFuYWdlbWVudENvbmZpZ3VyYXRpb24uUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudENvbmZpZ3VyYXRpb24uUmVhZFdyaXRlLkFsbCBEZXZpY2VNYW5hZ2VtZW50TWFuYWdlZERldmljZXMuUHJpdmlsZWdlZE9wZXJhdGlvbnMuQWxsIERldmljZU1hbmFnZW1lbnRNYW5hZ2VkRGV2aWNlcy5SZWFkLkFsbCBEZXZpY2VNYW5hZ2VtZW50TWFuYWdlZERldmljZXMuUmVhZFdyaXRlLkFsbCBEZXZpY2VNYW5hZ2VtZW50UkJBQy5SZWFkLkFsbCBEZXZpY2VNYW5hZ2VtZW50UkJBQy5SZWFkV3JpdGUuQWxsIERldmljZU1hbmFnZW1lbnRTZXJ2aWNlQ29uZmlnLlJlYWQuQWxsIERldmljZU1hbmFnZW1lbnRTZXJ2aWNlQ29uZmlnLlJlYWRXcml0ZS5BbGwgRGlyZWN0b3J5LkFjY2Vzc0FzVXNlci5BbGwgRGlyZWN0b3J5LlJlYWRXcml0ZS5BbGwgRmlsZXMuUmVhZFdyaXRlLkFsbCBHcm91cC5SZWFkV3JpdGUuQWxsIElkZW50aXR5Umlza0V2ZW50LlJlYWQuQWxsIE1haWwuUmVhZFdyaXRlIE1haWxib3hTZXR0aW5ncy5SZWFkV3JpdGUgTm90ZXMuUmVhZFdyaXRlLkFsbCBOb3RpZmljYXRpb25zLlJlYWRXcml0ZS5DcmVhdGVkQnlBcHAgb3BlbmlkIFBlb3BsZS5SZWFkIHByb2ZpbGUgUmVwb3J0cy5SZWFkLkFsbCBTaXRlcy5SZWFkV3JpdGUuQWxsIFRhc2tzLlJlYWRXcml0ZSBVc2VyLlJlYWRCYXNpYy5BbGwgVXNlci5SZWFkV3JpdGUgVXNlci5SZWFkV3JpdGUuQWxsIGVtYWlsIiwic2lnbmluX3N0YXRlIjpbImlua25vd25udHdrIiwia21zaSJdLCJzdWIiOiJ2a1NhVzNWV0tLZnBydXR4NzVGWkFmMzI1TU92RmFlOWNBekFIbFl1a3JrIiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidW5pcXVlX25hbWUiOiJtbWFpbmVyQG1pY3Jvc29mdC5jb20iLCJ1cG4iOiJtbWFpbmVyQG1pY3Jvc29mdC5jb20iLCJ1dGkiOiJCaE42WmVMSlFVdUVaeFdoUHdVRkFBIiwidmVyIjoiMS4wIiwieG1zX3N0Ijp7InN1YiI6InVGYVZvNzBhaTVDNTNpbGFBV01fUGtGazhMSnBOR3R1aW5BNGpGZzQtVEkifSwieG1zX3RjZHQiOjEyODkyNDE1NDd9.egE-Od3eGNynVZ_M9b_D4W849484DT1A-g7eMvDRMNMtYK6vNxwLZqqM8ZC-lvQHD-Z3UeMXlpXT003cqdwB5WZsbj8puHohpwowV_-CP80wpGSS80VjxYbXD6nYLjpsm7Z2KkKY65VhiNQeVwz06zt984ujkagCCN6bduwnlKNO0FtVZuuZxv8Ap6OdjZKB-1BNI2_8FXNZSujIjQ0edDSenWUP9g5sPumIFlFdATjsLB-xMqcgBkS0ZXo4LGJn1wZNlzo0TnKS_xLMon4GE9ZWpiT0G2FLDwk7K7dT_Wv0lJgZyTCELDPJce-Kc_uIpCIVltJR6KG4yGezbi7o5A";
                                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

                            });

            HttpClient client = GraphClientFactory.Create(authProvider);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/messages");

            Func<MyMessage, bool> processEachMessage = (m) =>
            {
                bool shouldContinue = true;

                Debug.WriteLine(m.Id);
                Debug.WriteLine(m.Subject);
                return shouldContinue;
            };

            var httpPageIterator = HttpPageIterator<MyMessagePage, MyMessage>.CreateHttpPageIterator(request,
                                                                                                     client,
                                                                                                     processEachMessage);

            await httpPageIterator.IterateAsync();
        }
    }

    // TODO: Why does a customer need to do this? I think we can get rid of this.

    /// <summary>
    /// This is the customer defined page object. 
    /// </summary>
    public class MyMessagePage : CollectionPage<MyMessage>
    {
        public MyMessagePage()
        {
        }
    }

    /// <summary>
    /// Customer will need to define a model class. We should have a base class that they 
    /// should inherit from so that they get AdditionalData and we can expect that they
    /// have a known type.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MyMessage
    {
        /**
         * {
         *      "id": "22312323",
         *      "subject": "a message"
         * }
         * 
         * **/

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore,
                           PropertyName = "id", 
                               Required = Newtonsoft.Json.Required.Default)]
        public string Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, 
                           PropertyName = "subjects", 
                               Required = Newtonsoft.Json.Required.Default)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }

        //public MyMessage(string id, string subject)
        //{
        //    Id = id;
        //    Subject = subject;
        //}
    }
}
