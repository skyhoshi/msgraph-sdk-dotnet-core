using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using OpenCensus;
using OpenCensus.Trace;
using OpenCensus.Trace.Config;
using OpenCensus.Trace.Sampler;

namespace LoggingTestRunner
{
    class Program
    {
        static void Main(string[] args)
        { 

            AuthProviderConstructorAsync().GetAwaiter().GetResult();
        }

        public static async System.Threading.Tasks.Task AuthProviderConstructorAsync()
        {

            // tracer
            ITraceConfig traceConfig = Tracing.TraceConfig;
            ITraceParams currentConfig = traceConfig.ActiveTraceParams;
            var newConfig = currentConfig.ToBuilder()
                .SetSampler(Samplers.AlwaysSample)
                .Build();
            traceConfig.UpdateActiveTraceParams(newConfig);

            var tracer = Tracing.Tracer;
       
            IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                .Create("4d9a208f-b479-445a-8b28-256c6613d14f")
                .Build();

            using (AuthenticationHandler auth = new AuthenticationHandler(new DeviceCodeProvider(publicClientApplication, new List<string> { "user.read" }), tracer))
            {
                auth.InnerHandler = new HttpClientHandler();
                try
                {
                    HttpMessageInvoker m = new HttpMessageInvoker(auth);
                    HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");

                    var repsonse = await m.SendAsync(msg, CancellationToken.None);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }

                Console.Write("Access token time span: " + auth.Trace + "\n");
                //Console.Write("Call to graph service time span: " + auth.Trace. "\n");



            }
        }
    }
}
