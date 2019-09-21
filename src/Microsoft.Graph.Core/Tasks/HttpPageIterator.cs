using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Graph.Core.Tasks
{
    /// <summary>
    /// Use HttpPageIterator&lt;TEntity&gt; to automatically page through result sets across multiple calls 
    /// and process each item in the result set.
    /// </summary>
    /// <typeparam name="TResponse">A user defined page returned in the result set.</typeparam>
    /// <typeparam name="TPage">A user defined page returned in the result set.</typeparam>
    /// <typeparam name="TPageEntity">A user defined page entity returned in the result set.</typeparam>
    public partial class HttpPageIterator<TPage, TPageEntity> where TPage:ICollectionPage<TPageEntity>
    {
        private HttpRequestMessage _request;
        private HttpClient _client;
        private Func<TPageEntity, bool> _processPageItemCallback;
        //private ResponseHandler _responseHandler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static HttpPageIterator<TPage, TPageEntity> CreateHttpPageIterator(HttpRequestMessage request, 
                                                                       HttpClient client, 
                                                                       Func<TPageEntity, bool> callback)

        {
            return new HttpPageIterator<TPage, TPageEntity>()
            {
                _request = request ?? throw new ArgumentNullException(nameof(request)),
                _client = client ?? throw new ArgumentNullException(nameof(client)),
                _processPageItemCallback = callback ?? throw new ArgumentNullException(nameof(callback))
                //_responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler))
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task IterateAsync()
        {
            await IterateAsync(new CancellationToken());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task IterateAsync(CancellationToken cancellationToken)
        {
            bool shouldContinueInterpageIteration = true;
            shouldContinueInterpageIteration = await InterpageIterateAsync(cancellationToken);
        }

        private async Task<bool> InterpageIterateAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage hrm = await _client.SendAsync(_request);
            
            // TODO: Handle both server and client errors. Pass through service errors.

            var responseHander = new ResponseHandler();

            var pageObject = await responseHander.HandleResponse<MyMessagePageResponse<TPage, TPageEntity>>(hrm);

            bool hasMorePagesOfData = pageObject.AdditionalData.TryGetValue("@odata.nextLink", out object nextPageLink);

            // TODO: Make this pattern match what we have in the service library.
            // TODO: Iterate over the collection according to the customer's delegate.
            // TODO: Continue calling IterpageIterateAsync until nextLink is not present.
            // TODO: Support DeltaLink.
            // TODO: Make this resumeable.

            return false;
        }
    }
    /// <summary>
    /// Customer will need this as it wraps the entire response body.
    /// We can probably hide this from the customer. 
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MyMessagePageResponse<TPage, TPageEntity>
    {

        /// <summary>
        /// Contains the actual page of responses
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "value", Required = Newtonsoft.Json.Required.Default)]
        public TPage Value { get; set; }

        /// <summary>
        /// Contains nextlink and deltalink
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
