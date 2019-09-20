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
                                var token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6ImQ2bm1iamtxWlpLZlFfQjEwWVJJZ0gyWGJ5amZoZTkxMGxNc05aYWdQXzAiLCJhbGciOiJSUzI1NiIsIng1dCI6ImllX3FXQ1hoWHh0MXpJRXN1NGM3YWNRVkduNCIsImtpZCI6ImllX3FXQ1hoWHh0MXpJRXN1NGM3YWNRVkduNCJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lZTk5MWQ4OC0wZWJlLTQ2OWMtOWMzMS00NmFkNTgwMmQxMzAvIiwiaWF0IjoxNTY5MDE4NTU5LCJuYmYiOjE1NjkwMTg1NTksImV4cCI6MTU2OTAyMjQ1OSwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFTUUEyLzhNQUFBQXZES2NFSTFGcUVBUkFkLzRWT0NhcDMzNFVVWnBxZlloblFLd1dQcnlnbjA9IiwiYW1yIjpbInB3ZCJdLCJhcHBfZGlzcGxheW5hbWUiOiJHcmFwaCBleHBsb3JlciIsImFwcGlkIjoiZGU4YmM4YjUtZDlmOS00OGIxLWE4YWQtYjc0OGRhNzI1MDY0IiwiYXBwaWRhY3IiOiIwIiwiZmFtaWx5X25hbWUiOiJWYW5jZSIsImdpdmVuX25hbWUiOiJBZGVsZSIsImlwYWRkciI6IjEzMS4xMDcuMTc0LjE3NiIsIm5hbWUiOiJVcGRhdGVkIEFkZWxlIFZhbmNlIiwib2lkIjoiNjBlYTY1ZDQtY2ExMy00OTNkLThkYjgtOTRjZTE2NGNiNTVhIiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMzRkZGQTUwMTkyMjUiLCJyaCI6IkkiLCJzY3AiOiJDYWxlbmRhcnMuUmVhZFdyaXRlIENvbnRhY3RzLlJlYWRXcml0ZSBGaWxlcy5SZWFkV3JpdGUuQWxsIEdyb3VwLlJlYWQuQWxsIE1haWwuUmVhZFdyaXRlIE5vdGVzLlJlYWRXcml0ZS5BbGwgb3BlbmlkIFBlb3BsZS5SZWFkIHByb2ZpbGUgU2l0ZXMuUmVhZFdyaXRlLkFsbCBUYXNrcy5SZWFkV3JpdGUgVXNlci5SZWFkQmFzaWMuQWxsIFVzZXIuUmVhZFdyaXRlIFVzZXIuUmVhZFdyaXRlLkFsbCBlbWFpbCIsInN1YiI6Inp3ZkpLajRCTHdYLXBqLUhzZFRqWDQ5RDU2S05vNDlsSTVramQxQll1eTQiLCJ0aWQiOiJlZTk5MWQ4OC0wZWJlLTQ2OWMtOWMzMS00NmFkNTgwMmQxMzAiLCJ1bmlxdWVfbmFtZSI6IkFkZWxlVkBNMzY1eDQ2Mjg5Ni5vbm1pY3Jvc29mdC5jb20iLCJ1cG4iOiJBZGVsZVZATTM2NXg0NjI4OTYub25taWNyb3NvZnQuY29tIiwidXRpIjoiVEpnakRuLTdhRXVrSnRyTFVlMERBQSIsInZlciI6IjEuMCIsIndpZHMiOlsiNjJlOTAzOTQtNjlmNS00MjM3LTkxOTAtMDEyMTc3MTQ1ZTEwIl0sInhtc19zdCI6eyJzdWIiOiJ6V3ltQk1XbVczNElRX0czNjJ1SGVZVHM5UHVTZ2pEd1p4MFVESXhxeVdzIn0sInhtc190Y2R0IjoxNTA2MDk3OTYxfQ.DYhZJ5OhacXThSBkoC4Ifuxga6tW6-EDNkmeWOURmq6QFLmsqZYPlG4QFej-UzoXpd8xID7kLg53b_N_lVld3MO6WurmADDcMSwxUKrDTcMI_TXkuOmL3wJgU9K3T_KqZVN4HGhnR_iLcBtQUnHTHiSlJ59hIVyv6lvF8WUfy0jV4NVGi_jWhOKTiwyRNTP0NzSTENks-TgsKRkwaNvyfBCa1vC6jA2VYKZPFJyNepXncLqG61-96Iy6OtCJAslCxMQ9voCCo6yMtYMbebkrvkXE0M6cXi54gWFljYOAt0EbFaYLAypfinjKDrE4gm8F630Mq-qfbo_uHHGrtB4lxA";
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

    /// <summary>
    /// Customer will need this as it wraps the entire response body.
    /// We can probably hide this from the customer. 
    /// </summary>
    //[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    //public class MyMessagePageResponse<TPage, TPageEntity>
    //{
       
    //    /// <summary>
    //    /// Contains the actual page of responses
    //    /// </summary>
    //    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "value", Required = Newtonsoft.Json.Required.Default)]
    //    public TPage Value { get; set; }

    //    /// <summary>
    //    /// Contains nextlink and deltalink
    //    /// </summary>
    //    [JsonExtensionData(ReadData = true)]
    //    public IDictionary<string, object> AdditionalData { get; set; }
    //}

    /// <summary>
    /// This is the customer defined page object. 
    /// </summary>
    
    public class MyMessagePage : CollectionPage<MyMessage>
    {
        public MyMessagePage()
        {
        }
    }

    // TODO: Create a base class for customer to use.

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
