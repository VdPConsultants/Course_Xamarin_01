using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
//using Microsoft.WindowsAzure.Storage.Table;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Microsoft.Azure.WebJobs.Host;

namespace VDPC.Function
{
    public static class TripLog
    {
        [FunctionName("TripLog")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("EntryTable")] CloudTable gettable,
            [Table("EntryTable")] IAsyncCollector<Entry> addtable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (req.Method == "GET")
            {
                var querySegment = gettable.ExecuteQuerySegmentedAsync(new TableQuery<Entry>(), null);
                String test = JsonConvert.SerializeObject(querySegment.Result);
                StringContent responseContent = null;
                responseContent = new StringContent(test, Encoding.UTF8, "application/json");
                foreach (Entry item in querySegment.Result)
                {
                    log.LogInformation($"Data loaded: '{item.Date}' | '{item.Latitude}' | '{item.Longitude}' | '{item.Notes}'");
                }
                return (ActionResult) new OkObjectResult(test);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Entry entry = JsonConvert.DeserializeObject<Entry>(requestBody);
 
            if (entry != null)
            {
                await addtable.AddAsync(entry);
                return (ActionResult) new OkObjectResult(entry);
            }

            return new BadRequestObjectResult("Invalid entry request.");
       }
    }
    public class Entry : TableEntity
    {
        public string Id => Guid.NewGuid().ToString("n");
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Date { get; set; }
        public int Rating { get; set; }
        public string Notes { get; set; }
        // Required for Table Storage entities
        public string PartitionKey => "ENTRY";
        public string RowKey => Id;
    }
}
