using Microsoft.AspNetCore.Mvc;
using BookingApi.Models;
using System.Collections;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using System;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {

        private readonly IConfiguration _config;
        private SqlConnection _connection;

        public PurchaseOrderController(IConfiguration config)
        {
            _config = config;
            _connection = new SqlConnection(_config.GetConnectionString("connString"));

        }

        [HttpPost]
        [Route("/createpurchaseorder")]
        public async Task<IActionResult> insertPurchaseOrder(CompletePurchaseOrder request)
            {

            Hashtable response = new Hashtable();

            if (ModelState.IsValid)
            {


               using (_connection)
               {
                   _connection.OpenAsync().Wait();
                   
                   using (SqlCommand command = new SqlCommand("spInsertPurchaseOrder", _connection))
                   {
                       command.CommandType = CommandType.StoredProcedure;
                       command.Parameters.AddWithValue("CostCenter", SqlDbType.NVarChar).Value = request.FormData.CostCenter;
                       command.Parameters.AddWithValue("Supplier", SqlDbType.NVarChar).Value = request.FormData.Supplier;
                       command.Parameters.AddWithValue("ShipsTo", SqlDbType.NVarChar).Value = request.FormData.ShipsTo;
                       command.Parameters.AddWithValue("OrderAmount", SqlDbType.NVarChar).Value = request.FormData.OrderAmount;
                       command.Parameters.AddWithValue("FirstDeliveryDate", SqlDbType.NVarChar).Value = request.FormData.FirstDeliveryDate;
                       command.Parameters.AddWithValue("Narration", SqlDbType.NVarChar).Value = request.FormData.Narration;
                       command.Parameters.AddWithValue("OrderDate", SqlDbType.NVarChar).Value = request.FormData.OrderDate;
                       command.Parameters.AddWithValue("DeliveryPeriod", SqlDbType.NVarChar).Value = request.FormData.DeliveryPeriod;
                       command.Parameters.AddWithValue("VehicleDetails", SqlDbType.NVarChar).Value = request.FormData.VehicleDetails;

                       
                       using(SqlDataReader reader = await command.ExecuteReaderAsync())
                       {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                response.Add("formStatus", reader.GetInt32(0));
                                                               
                            }

                       }

                   }

                    try {
                        foreach (var PurchaseItem in request.TableData)
                        {
                            using (SqlCommand command = new SqlCommand("spInsertPurchaseOrderItem", _connection))
                                {
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("itemName", SqlDbType.NVarChar).Value = PurchaseItem.item;
                                    command.Parameters.AddWithValue("quantity", SqlDbType.NVarChar).Value = PurchaseItem.quantity;
                                    command.Parameters.AddWithValue("unitCost", SqlDbType.NVarChar).Value = PurchaseItem.unitCost;
                                    command.Parameters.AddWithValue("extendedCost", SqlDbType.NVarChar).Value = PurchaseItem.extendedCost;
                                    command.Parameters.AddWithValue("taxAmount", SqlDbType.NVarChar).Value = PurchaseItem.taxAmount;
                                    command.Parameters.AddWithValue("discountAmount", SqlDbType.NVarChar).Value = PurchaseItem.discountAmount;
                                    command.Parameters.AddWithValue("lineTotal", SqlDbType.NVarChar).Value = PurchaseItem.lineTotal;

                                    using(SqlDataReader reader = await command.ExecuteReaderAsync()){

                                        if (reader.HasRows)
                                        {
                                            reader.Read();
                                            response.Add("tableStatus", reader.GetInt32(0));

                                        }

                                    }

                                }
                            
                        }
                        response.Add("tableStatus", "Success");

                    }
                    catch (Exception Ex){
                        Console.WriteLine(Ex.Message);
                        response.Add("tableStatus", "Success");

                    }

               }

               return new OkObjectResult(response);






            }
            else
            {
               response.Add("Status", "Failed");
               response.Add("Message", "Invalid Purchase Order");
               return new JsonResult(request);
            }

            }
        }




    }

