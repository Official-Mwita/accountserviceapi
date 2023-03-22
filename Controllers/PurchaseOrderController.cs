using Microsoft.AspNetCore.Mvc;
using BookingApi.Models;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Azure.Cosmos;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookingApi.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private static readonly string EndpointUri = "https://purchaseorderitems.documents.azure.com:443/";

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "UEyhDWw0UF9CweujkD8xlhtnhWpucIJHiElDrLa47gL77EwBfCMueYfeDcwiZPwvB3VyX6uignNBACDbPg1ohQ==";

        // The name of the database and container we will create
        private string databaseId = "purchaseorderitems";
        private string containerId = "orderitems";


        private readonly IConfiguration _config;
        private SqlConnection _connection;

        public PurchaseOrderController(IConfiguration config)
        {
            _config = config;
            _connection = new SqlConnection(_config.GetConnectionString("connString"));

        }

        [HttpPost]
        [Route("createpurchaseorder")]
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

            [HttpPost]
            [Route("insertorderitems")]
            public async Task<IActionResult> InsertOrderItems ([FromBody] MPurchaseOrderItem item) {
                if (ModelState.IsValid){
                    CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                    Container container = cosmosClient.GetContainer(databaseId, containerId);
                    try {
                        ItemResponse<MPurchaseOrderItem> response = await container.CreateItemAsync<MPurchaseOrderItem>(item, new PartitionKey(item.partitionKey));
                        return new OkResult();
                    } catch(Exception e) {
                        Console.WriteLine(e.Message);
                        return new BadRequestResult();


                    }
                } 

                return new BadRequestResult();

                
            }

            [HttpPut]
            [Route("updateorderitem")]
            public async Task<IActionResult> UpdateOrderItem ([FromBody] MPurchaseOrderItem item) {
                if (ModelState.IsValid){
                    CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                    Container container = cosmosClient.GetContainer(databaseId, containerId);
                    ItemResponse<MPurchaseOrderItem> OrderItemResponse = await container.ReadItemAsync<MPurchaseOrderItem>(item.id, new PartitionKey(item.partitionKey));
                    Console.WriteLine(OrderItemResponse.Resource.id);

                    try {
                        OrderItemResponse = await container.ReplaceItemAsync<MPurchaseOrderItem>(item, item.id, new PartitionKey(item.partitionKey));
                        return new OkResult();
                    } catch(Exception e) {
                        Console.WriteLine(e.Message);
                        return new BadRequestResult();

                    }
                } 

                return new BadRequestResult();

                
            }

            [HttpPost]
            [Route("getorderitems")]

            public async Task<IActionResult> GetOrderItems (MPurchaseOrderUser user)
                {
                    
                    CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                    Container container = cosmosClient.GetContainer(databaseId, containerId);
                    string sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = '{user.userid}'";
                    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                    FeedIterator<MPurchaseOrderItem> queryResultSetIterator = container.GetItemQueryIterator<MPurchaseOrderItem>(queryDefinition);

                    List<MPurchaseOrderItem> orderitems = new List<MPurchaseOrderItem>();

                    while (queryResultSetIterator.HasMoreResults)
                        {
                            try {
                            FeedResponse<MPurchaseOrderItem> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                            foreach (MPurchaseOrderItem orderitem in currentResultSet)
                                {
                                    orderitems.Add(orderitem);
                                    Console.WriteLine("\tRead {0}\n", orderitem.item);
                                }
                            } catch(Exception X){
                                Console.WriteLine(X.Message);
                                return new BadRequestResult();
                            }
                        }
                    
                    return new OkObjectResult(orderitems);
                
                }
            
            [HttpDelete]
            [Route("removeorderitem")]

            public async Task<IActionResult> removeorderitem(MPurchaseOrderItem item) 
                {
                    CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                    Container container = cosmosClient.GetContainer(databaseId, containerId);
                    var partitionKeyValue = item.partitionKey;
                    var itemid = item.id;
                    // Delete an item. Note we must provide the partition key value and id of the item to delete
                    try {
                    ItemResponse<MPurchaseOrderItem> itemResponse = await container.DeleteItemAsync<MPurchaseOrderItem>(itemid, new PartitionKey(partitionKeyValue));
                    Console.WriteLine("Deleted item [{0},{1}]\n", partitionKeyValue, itemid);
                    return new OkResult();

                    } catch (Exception X) {
                        Console.WriteLine(X.Message);
                        return new BadRequestResult();
                    }

                }


        }




    }

