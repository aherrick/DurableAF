using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
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
            bool async = false;

            // 1)
            // transfrom xml pulled from request

            string xml = await req.Content.ReadAsAsync<string>(); // xml
            var newXml = TransformXML(xml);

            // move this code to a base helper function
            // 2) call durable
            string instanceId = await client.StartNewAsync("DieselTransDurable", newXml);

            if (async)
            {
                return client.CreateCheckStatusResponse(req, instanceId);
            }
            else
            {
                return await client.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout: new TimeSpan(0, 0, 120), retryInterval: new TimeSpan(0, 0, 1));
                /*
                for (int i = 0; i < 120; i++)
                {
                    var status = await client.GetStatusAsync(instanceId);
                    if (status?.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                    {
                        var result = status.Output.ToObject<DurableResponse>();

                        return req.CreateResponse(HttpStatusCode.OK, result);
                    }
                    else if (status?.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
                    {
                        // return error message
                        var data = status.Output;
                        return req.CreateResponse(HttpStatusCode.OK, data.ToString());
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }*/
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, "Bad");
        }

        public static string TransformXML(string xml)
        {
            // do the XML transform here, and add the PUR Code
            // each function will have one of these methods, except generic xml endpoint, assume already has TransCode

            return xml;
        }
    }
}