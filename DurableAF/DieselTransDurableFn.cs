using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DurableAF
{
    public static class DieselTransDurableFn
    {
        [FunctionName("DieselTransDurable")]
        public static async Task<DurableResponse> Run(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            try
            {
                outputs.Add(await context.CallActivityAsync<string>("DieselQueueVal", "Tokyo"));
                outputs.Add(await context.CallActivityAsync<string>("DieselQueueProc", "Seattle"));
                outputs.Add(await context.CallActivityAsync<string>("DieselQueueChannel", "Seattle"));
                outputs.Add(await context.CallActivityAsync<string>("DieselQueueAlert", "Seattle"));
            }
            catch (Exception ex)
            {
                return new DurableResponse()
                {
                    ErrorMsg = ex.ToString()
                };
            }

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return new DurableResponse()
            {
                XMLResult = XmlHelpers.Serialize(outputs)
            };
        }

        [FunctionName("DieselQueueVal")]
        public static string DieselQueueVal([ActivityTrigger] string name)
        {
            // Set Trans Req status to INVALIDATION
            // Fire off validation azure function
            // Set Trans Req status to READFORPROCESS if success or FAILEDVALIDATION if fail
            Thread.Sleep(10000);
            return $"Hello {name}!";
        }

        [FunctionName("DieselQueueProc")]
        public static string DieselQueueProc([ActivityTrigger] string name)
        {
            // Set Trans Req Status to INPROCESS
            // Fire Off Process azure function
            // if Success trans req status = READYFORCHANNEL else ERROR if fail
            Thread.Sleep(10000);

            return $"Hello {name}!";
        }

        [FunctionName("DieselQueueChannel")]
        public static string DieselQueueChannel([ActivityTrigger] string name)
        {
            // Set Trans req status to INCHANNEL
            // Fire off channel code
            // If success trans req status = READYFORALERTS else ERROR if fail
            // throw new Exception("OH NO");

            return $"Hello {name}!";
        }

        [FunctionName("DieselQueueAlert")]
        public static string DieselQueueAlert([ActivityTrigger] string name)
        {
            // Set trans req status to PROCESSINGALERTS
            // fire off alerts code
            // if success Trans Req status = COMPLETE else ERROR if fail
            // Thread.Sleep(10000);

            return $"Hello {name}!";
        }
    }
}