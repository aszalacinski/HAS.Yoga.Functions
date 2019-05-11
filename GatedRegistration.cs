using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace HAS.Yoga.Functions
{
    public static class GatedEntry
    {
        [FunctionName("GatedRegistration")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "gate")] HttpRequest req,
            ILogger log)
        {
            var str = Environment.GetEnvironmentVariable("GatedEntryDB");

            string emailAddress = req.Query["emailAddress"];
            string entryCode = req.Query["entryCode"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            emailAddress = emailAddress ?? data?.emailAddress;
            entryCode = entryCode ?? data?.entryCode;
            
            var registered = false;

            using (SqlConnection conn = new SqlConnection(str))
            {
                var cont = false;

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.HAS_GatedRegistration_ValidateEntryCode", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter entryCodeParam = cmd.Parameters.Add("@entryCode", SqlDbType.NChar, 10);
                        entryCodeParam.Direction = ParameterDirection.Input;
                        entryCodeParam.Value = entryCode;

                        SqlParameter emailAddressParam = cmd.Parameters.Add("@emailAddress", SqlDbType.NVarChar, 50);
                        emailAddressParam.Direction = ParameterDirection.Input;
                        emailAddressParam.Value = emailAddress;

                        cmd.Parameters.Add("@result", SqlDbType.Bit).Direction = ParameterDirection.Output;

                        await cmd.ExecuteScalarAsync();

                        cont = Convert.ToBoolean(cmd.Parameters["@result"].Value);
                    }
                    
                    if (cont)
                    {
                        using (SqlCommand cmd = new SqlCommand("dbo.HAS_GatedRegistration_UpdateEntryCode",conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            SqlParameter entryCodeParam = cmd.Parameters.Add("@entryCode", SqlDbType.NChar, 10);
                            entryCodeParam.Direction = ParameterDirection.Input;
                            entryCodeParam.Value = entryCode;

                            SqlParameter emailAddressParam = cmd.Parameters.Add("@emailAddress", SqlDbType.NVarChar, 50);
                            emailAddressParam.Direction = ParameterDirection.Input;
                            emailAddressParam.Value = emailAddress;

                            await cmd.ExecuteNonQueryAsync();

                            registered = true;
                            log.LogInformation($"Gated Registration successful for entrycode: {entryCode}");
                        }
                    }
                    else
                    {
                        log.LogInformation($"Unauthorized attempt to access Gated Regisration for entrycode: {entryCode}");
                    }
                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                }

            }

            return registered == true ? (ActionResult)new OkObjectResult($"true") : new UnauthorizedResult();
        }
    }
}
