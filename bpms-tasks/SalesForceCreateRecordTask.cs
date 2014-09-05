namespace Simple.Bpms.Tasks.SystemTasks
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;
    using Typesafe.Mailgun;

    /// <summary>
    /// INPUT:
    ///     last_name
    ///     description
    ///     
    /// OUTPUT:
    ///     <None>
    /// 
    /// </summary>
    public class SalesForceCreateRecordTask : BpmsTask
    {
        const string LastNameInputKey = "last_name";
        const string DescriptionInputKey = "description";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, 
            IDictionary<string, string> input)
        {
            string lastName = input[LastNameInputKey];
            string description = input[DescriptionInputKey];

            if(string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(description))
            {
                throw new BpmsTaskException("Invalid parameters");
            }

            SalesForceAddRecordRequest req = new SalesForceAddRecordRequest()
            {
                RowKey = Guid.NewGuid().ToString(),
                Company = "BPMS-System",
                LastName = lastName,
                Description = description
            };

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-zumo-auth", 
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ1cm46bWljcm9zb2Z0OndpbmRvd3MtYXp1cmU6enVtbyIsImF1ZCI6InVybjptaWNyb3NvZnQ6d2luZG93cy1henVyZTp6dW1vIiwibmJmIjoxNDA5ODcwNzE4LCJleHAiOjE0MTI0NjI3MTgsInVybjptaWNyb3NvZnQ6Y3JlZGVudGlhbHMiOiJ7XCJhY2Nlc3NUb2tlblwiOlwiQ0FBSlYzbE04cHZ3QkFQWkFrRVJEVWdHazh5NmdkV3h0QXBsSUsybkVVeVFrb1pBRW5JbDF2aEh1d1MwZ1MzNlhGeTYyMDZIdHRrUkl4UVpBSGVyWkJVNUoxWkN4SjVNSjFNUnA4UGZLRE9OeGMxU3lWSkVTQkdWeVpCWkFwbzgxZFdzYTZzWVpCaTNTNUF5WkNYdWxCaTJuaXVLYmxUZzhTNW4zaWtlSmhzUWVMbjF4eUdsNldRRG1aQ2E3ZHlmUnVlWkFybnlnUHlzaWh0QnlHUlBUellUWkNrVXpcIn0iLCJ1aWQiOiJGYWNlYm9vazoxMDE1Mjk2NDk0NjY3NzU3MSIsInZlciI6IjIifQ.t7P1NKTN_3G1GEu6UbTTbAgm2Mm9NCmu1vOtyQ8qw0g");
            var resp = client.PostAsJsonAsync<SalesForceAddRecordRequest>("http://henrikntest13.azure-mobile.net/mobile/appsync/gid", req).Result;
            
            string outcome = "failure";

            if (resp.IsSuccessStatusCode)
            {

                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "http://henrikntest13.azure-mobile.net/jobs/sync");

                request.Headers.Add("x-zumo-master", "HlTPStmcpIwGJlkElcpBuRIymnmFDl72");
                resp = new HttpClient().SendAsync(request).Result;
                if (resp.IsSuccessStatusCode)
                {
                    outcome = "success";
                }
            }
            return new Dictionary<string, string>() { { "salesforce_create_record_outcome", outcome } };
        }

        protected override string[] RequiredInputKeys
        {
            get 
            {
                return new string[] { LastNameInputKey, DescriptionInputKey };
            } 
        }

        class SalesForceAddRecordRequest
        {
            public string RowKey;
            public string Company;
            public string LastName;
            public string Description;
        }
    }
}
