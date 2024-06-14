using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Mixingdata : ControllerBase
    {
        
        private readonly string connectionString = "Data Source=192.168.20.70,1433;Initial Catalog=Mixing;User ID=admin;Password=Fores@123;";

        [HttpGet]
        [Route("getBatchdata")]
        public async Task<IActionResult> GetBatchData(string sapcode, string batchno)
        {
            string status = "OK";
            // Validate inputs
            if (string.IsNullOrEmpty(sapcode) || string.IsNullOrEmpty(batchno))
            {
                return BadRequest("Invalid input");
            }

            string query = $"SELECT R_ml, R_mh, R_ts2, R_tc50, R_tc90,Hardness_1,Hardness_2,Hardness_3,Hardness_4,SpecificGravity,Reho_Date_Time FROM {sapcode} WHERE Batch_No = @batchno AND status LIKE @status";

            var result = new List<Dictionary<string, object>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parameterize the query to prevent SQL injection
                        command.Parameters.AddWithValue("@batchno", batchno);
                        command.Parameters.AddWithValue("@status", status);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                double meanhrd1 = reader.GetDouble(5);
                                double meanhrd2 = reader.GetDouble(6);
                                double meanhrd3 = reader.GetDouble(7);
                                double meanhrd4 = reader.GetDouble(8);

                                double Averagehrd = (meanhrd1 + meanhrd2 + meanhrd3 + meanhrd4)/4;
                                var data = new Dictionary<string, object>
                                {
                                    { "ML", reader["R_ml"] },
                                    { "MH", reader["R_mh"] },
                                    { "TS2", reader["R_ts2"] },
                                    { "Tc50", reader["R_tc50"] },
                                   
                                    { "Tc90", reader["R_tc90"] },
                                    { "Hardness", Averagehrd },
                                    { "Gravity", reader["SpecificGravity"] },
                                    { "Rheodate", reader["Reho_Date_Time"] },


                                };
                                result.Add(data);
                            }
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                // Log the exception (in a real-world application, consider using a logging framework)
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
                
            }
        }
    }
}
