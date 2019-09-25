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

    //
    ///
    //public partial class HttpPageIterator<ICollectionPage<TPageEntity>> where TPageEntity:object
    public partial class HttpPageIterator<TPage, TPageEntity> where TPage : ICollectionPage<TPageEntity>
    {
        private HttpRequestMessage _request;
        private HttpClient _client;
        private Func<TPageEntity, bool> _processPageItemCallback;
        private Queue<TPageEntity> _pageItemQueue;
        //private ResponseHandler _responseHandler;

        /// <summary>
        /// The @odata.deltaLink returned from a delta query.
        /// </summary>
        public string Deltalink { get; private set; }
        /// <summary>
        /// The @odata.nextLink returned in a paged result.
        /// </summary>
        public string Nextlink { get; private set; }

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
                _processPageItemCallback = callback ?? throw new ArgumentNullException(nameof(callback)),
                _pageItemQueue = new Queue<TPageEntity>()
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

            // TODO: Handle resume iteration, for both the TPageEntity in queue, and the 
            while (shouldContinueInterpageIteration)
            {
                shouldContinueInterpageIteration = await InterpageIterateAsync(cancellationToken).ConfigureAwait(false);

                
                // TODO: IntrapageIterateAsync 
            }

            // TODO: Final IntrapageIterateAsync in case the iteration 
        }

        private async Task<bool> InterpageIterateAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage hrm = null;

            try
            {
                hrm = await _client.SendAsync(_request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // TODO: Handle both server and client errors. Pass through service errors.
            }

            var responseHander = new ResponseHandler();

            var pageObject = await responseHander.HandleResponse <MyMessagePageResponse<TPage>>(hrm);

            // Add all of the items returned in the response to the queue.
            // for processing by the Func<TPageEntity, bool>
            if (pageObject.Value.Count > 0)
            {
                foreach (TPageEntity entity in pageObject.Value)
                {
                    _pageItemQueue.Enqueue(entity);
                }
            }

            bool hasMorePagesOfData = pageObject.AdditionalData.TryGetValue("@odata.nextLink", out object nextPageLink);

            // TODO: Make this pattern match what we have in the service library.
            // TODO: Iterate over the collection according to the customer's delegate.
            // TODO: Continue calling IterpageIterateAsync until nextLink is not present.
            // TODO: Support DeltaLink.
            // TODO: Make this resumeable.

            return hasMorePagesOfData;
        }
    }
    /// <summary>
    /// This  wraps the ICollectionPage&lt;PageEntity&gt; created by the customer.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class MyMessagePageResponse<TPage>
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
