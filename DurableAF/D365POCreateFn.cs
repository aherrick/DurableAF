using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DurableAF
{
    /// <summary>
    /// test
    /// </summary>
    public class D365POCreateFn
    {
        //[FunctionName("HttpStart")]
        //public static async Task<HttpResponseMessage> Run(
        //  [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "orchestrators/{functionName}")] HttpRequestMessage req,
        //  [OrchestrationClient] DurableOrchestrationClient starter,
        //  string functionName,
        //  TraceWriter log)
        //{
        //    // Function input comes from the request content.
        //    dynamic eventData = await req.Content.ReadAsAsync<object>();
        //    string instanceId = await starter.StartNewAsync(functionName, eventData);

        //    log.Info($"Started orchestration with ID = '{instanceId}'.");

        //    var res = starter.CreateCheckStatusResponse(req, instanceId);
        //    res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
        //    return res;
        //}

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
                return client.CreateCheckStatusResponse(req, instanceId);
            }
            else
            {
                for (int i = 0; i < 120; i++)
                {
                    var status = await client.GetStatusAsync(instanceId);
                    if (status?.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                    {
                        var data = status.Output;
                        return req.CreateResponse(HttpStatusCode.OK, data.ToString());
                    }
                    else if (status?.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
                    {
                        var data = status.Output;
                        return req.CreateResponse(HttpStatusCode.OK, data.ToString());
                    }

                    // handle other status conditions, like failure

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, "Bad");
        }

        public static string TransformXML(string xml)
        {
            // do the XML transform here, and add the PUR Code
            // each function will have one of these methods, except generic xml endpoint, assume already has TransCode

            return xml;
        }

        ///// <summary>
        ///// transform
        ///// </summary>
        ///// <param name="xml"></param>
        //public static override string TransformXML(string xml)
        //{
        //    return xml;
        //}

        //[FunctionName("D365POCreate")]
        //public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        //{
        //    log.Info("C# HTTP trigger function processed a request.");

        //    // parse query parameter
        //    string name = req.GetQueryNameValuePairs()
        //        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        //        .Value;

        //    // Get request body
        //    dynamic data = await req.Content.ReadAsAsync<object>();

        //    // Set name to query string or body data
        //    name = name ?? data?.name;

        //    return name == null
        //        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        //        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        //}
    }
}