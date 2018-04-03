using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DurableAF
{
    public class D365POCreateFn
    {
        [FunctionName("D365POCreate")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, methods: "post")] HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient client,
          TraceWriter log)
        {
            // 0)
            // check if async, for now hard cocde
            bool async = true;

            // 1)
            // transfrom xml pulled from request

            string xml = await req.Content.ReadAsAsync<string>(); // xml
            var newXml = TransformXML(xml);

            // move this code to a base helper function
            // 2) call durable
            string instanceId = await client.StartNewAsync("DieselTransDurable", newXml);

            if (async)
            {
                var res = client.CreateCheckStatusResponse(req, instanceId);
                // tells calling system to check again after 10 seconds
                res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
                return res;
            }
            else
            {
                return await client.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout: new TimeSpan(0, 0, 120), retryInterval: new TimeSpan(0, 0, 5));
            }
        }

        public static string TransformXML(string xml)
        {
            // do the XML transform here, and add the PUR Code
            // each function will have one of these methods, except generic xml endpoint, assume already has TransCode

            return xml;
        }
    }
}