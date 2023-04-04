using Microsoft.AspNetCore.Mvc;
using BookingApi.Models;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using accountservice.Commons;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookingApi.Controllers
    {

        
        [Route("api/[controller]")]
        [ApiController]
        public class PurchaseOrderController : ControllerBase
            {
                // The primary key for the Azure Cosmos account.

                // The name of the database and container we will create

                private readonly IConfiguration _config;
                private SqlConnection _connection;

                public PurchaseOrderController(IConfiguration config)
                {
                    _config = config;
                    _connection = new SqlConnection(_config.GetConnectionString("connString"));

                }

                public async Task<Hashtable> fetchOrder (string userid)
                    {
                        Hashtable result = new Hashtable();
                        CosmosDbHandler<MPurchaseOrderItem> handler = CosmosDbHandler<MPurchaseOrderItem>.CreateCosmosHandlerInstance();
                        CosmosDbHandler<MPurchaseOrder> infoHandler = CosmosDbHandler<MPurchaseOrder>.CreateCosmosHandlerInstance("purchaseorderitems", "orderinformation");
                        string itemsQueryText = $"SELECT * FROM c WHERE c.createdBy = '{userid}'";
                        string infoQueryText = $"SELECT * FROM c WHERE c.id = '{userid}'";

                        try {
                            Hashtable order = new Hashtable();
                            List<MPurchaseOrderItem> orderitems = await handler.QuerySelector(itemsQueryText);
                            List<MPurchaseOrder> orderinformation = await infoHandler.QuerySelector(infoQueryText);
                            order.Add("orderItems", orderitems);
                            order.Add("orderInformation", orderinformation);
                            result.Add("code", "success");
                            result.Add("data", order);
                            return result;
                        }

                        catch(Exception X){
                            Console.WriteLine(X.Message);
                            result.Add("code", "failed");
                            return result;
                        }

                    }
                    

                [HttpGet]
                [Route("getorderitems")]
                public async Task<IActionResult> GetOrderItems ([FromQuery] string userid)
                    {
                        if (ModelState.IsValid){
                            CosmosDbHandler<MPurchaseOrderItem> handler = CosmosDbHandler<MPurchaseOrderItem>.CreateCosmosHandlerInstance();
                            CosmosDbHandler<MPurchaseOrder> infoHandler = CosmosDbHandler<MPurchaseOrder>.CreateCosmosHandlerInstance("purchaseorderitems", "orderinformation");
                            string itemsQueryText = $"SELECT * FROM c WHERE c.createdBy = '{userid}'";
                            string infoQueryText = $"SELECT * FROM c WHERE c.id = '{userid}'";

                            try {
                                Hashtable order = new Hashtable();
                                List<MPurchaseOrderItem> orderitems = await handler.QuerySelector(itemsQueryText);
                                List<MPurchaseOrder> orderinformation = await infoHandler.QuerySelector(infoQueryText);
                                order.Add("orderItems", orderitems);
                                order.Add("orderInformation", orderinformation);
                                return new OkObjectResult(order);
                            }

                            catch(Exception X){
                                Console.WriteLine(X.Message);
                                return new BadRequestResult();
                            }

                        }

                        return new BadRequestResult();
                    }


                [HttpPost]
                [Route("createpurchaseorder")]
                public async Task<IActionResult> insertPurchaseOrder(MPurchaseOrderUser? user)
                    {

                        string userid = user?.userid ?? "";
                        Hashtable response = new Hashtable();

                        Random rnd = new Random();
                        int num = rnd.Next(100000, 500000);

                        Hashtable order;
                        List<MPurchaseOrder> orderInformation;
                        List<MPurchaseOrderItem> orderItems;

                        try {
                            order = await fetchOrder(userid);
                            var itemsTable = order["data"] as Hashtable;
                            orderItems = itemsTable?["orderItems"] as List<MPurchaseOrderItem> ?? new List<MPurchaseOrderItem>();
                            orderInformation = itemsTable?["orderInformation"] as List<MPurchaseOrder> ?? new List<MPurchaseOrder>();
                            
                        }
                        catch(Exception Ex) {
                            return new BadRequestObjectResult(Ex.Message);
                        }

                    using (_connection)
                            {
                                _connection.OpenAsync().Wait();
                                
                                using (SqlCommand command = new SqlCommand("spInsertPurchaseOrder", _connection))
                                    {
                                        command.CommandType = CommandType.StoredProcedure;
                                        command.Parameters.AddWithValue("CostCenter", SqlDbType.NVarChar).Value = orderInformation[0].CostCenter;
                                        command.Parameters.AddWithValue("Supplier", SqlDbType.NVarChar).Value = orderInformation[0].Supplier;
                                        command.Parameters.AddWithValue("ShipsTo", SqlDbType.NVarChar).Value = orderInformation[0].ShipsTo;
                                        command.Parameters.AddWithValue("OrderAmount", SqlDbType.NVarChar).Value = orderInformation[0].OrderAmount;
                                        command.Parameters.AddWithValue("FirstDeliveryDate", SqlDbType.NVarChar).Value = orderInformation[0].FirstDeliveryDate;
                                        command.Parameters.AddWithValue("Narration", SqlDbType.NVarChar).Value = orderInformation[0].Narration;
                                        command.Parameters.AddWithValue("OrderDate", SqlDbType.NVarChar).Value = orderInformation[0].OrderDate;
                                        command.Parameters.AddWithValue("DeliveryPeriod", SqlDbType.NVarChar).Value = orderInformation[0].DeliveryPeriod;
                                        command.Parameters.AddWithValue("VehicleDetails", SqlDbType.NVarChar).Value = orderInformation[0].VehicleDetails;
                                        command.Parameters.AddWithValue("OrderNumber", SqlDbType.NVarChar).Value = num;                      

                                        
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
                                        foreach (var PurchaseItem in orderItems)
                                            {
                                                using (SqlCommand command = new SqlCommand("spInsertPurchaseOrderItem", _connection))
                                                    {
                                                        command.CommandType = CommandType.StoredProcedure;
                                                        command.Parameters.AddWithValue("item", SqlDbType.NVarChar).Value = PurchaseItem.item;
                                                        command.Parameters.AddWithValue("quantity", SqlDbType.NVarChar).Value = PurchaseItem.quantity;
                                                        command.Parameters.AddWithValue("unitCost", SqlDbType.NVarChar).Value = PurchaseItem.unitCost;
                                                        command.Parameters.AddWithValue("extendedCost", SqlDbType.NVarChar).Value = PurchaseItem.extendedCost;
                                                        command.Parameters.AddWithValue("taxAmount", SqlDbType.NVarChar).Value = PurchaseItem.taxAmount;
                                                        command.Parameters.AddWithValue("discountAmount", SqlDbType.NVarChar).Value = PurchaseItem.discountAmount;
                                                        command.Parameters.AddWithValue("lineTotal", SqlDbType.NVarChar).Value = PurchaseItem.lineTotal;
                                                        command.Parameters.AddWithValue("partitionKey", SqlDbType.NVarChar).Value = num;
                                                        command.Parameters.AddWithValue("id", SqlDbType.NVarChar).Value = PurchaseItem.id;
                                                        command.Parameters.AddWithValue("createdBy", SqlDbType.NVarChar).Value = PurchaseItem.createdBy;

                                                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                                                            {
                                                                continue;

                                                            }


                                                    }
                                                
                                            }
                                        response.Add("tableStatus", "Success");

                                    }
                                    catch (Exception Ex){
                                        Console.WriteLine(Ex.Message);
                                        response.Add("tableStatus", Ex.Message);

                                    }

                            }

                            try {
                                    CosmosDbHandler<MPurchaseOrder> handler = CosmosDbHandler<MPurchaseOrder>.CreateCosmosHandlerInstance("purchaseorderitems", "orderinformation");
                                    await handler.RemoveItem(orderInformation[0].id, orderInformation[0].id);
                                    CosmosDbHandler<MPurchaseOrderItem> itemHandler = CosmosDbHandler<MPurchaseOrderItem>.CreateCosmosHandlerInstance();

                                    foreach (var item in orderItems) {
                                        await itemHandler.RemoveItem(item.id, item.createdBy);
                                    }
                                    
                            } 
                            catch(Exception Ex) {
                                    Console.WriteLine(Ex.Message);
                            }

                            return new OkObjectResult(response);



                    }

                    // This controller adds order items to Azure CosmosDB
                    [HttpPost]
                    [Route("insertorderitems")]
                    public async Task<IActionResult> InsertOrderItems ([FromBody] MPurchaseOrderItem item) {
                        if (ModelState.IsValid){
                            CosmosDbHandler<MPurchaseOrderItem> handler = CosmosDbHandler<MPurchaseOrderItem>.CreateCosmosHandlerInstance();
                            try {
                                await handler.InsertItem(item, item.createdBy);
                                return new OkResult();
                            }
                            catch(Exception e) {
                                Console.WriteLine(e.Message);
                                return new BadRequestResult();

                            }
                        } 

                        return new BadRequestResult();
                        
                    }

                
                // This controller updates already existing order items in Azure CosmosDB

                [HttpPut]
                [Route("updateorderitem")]
                public async Task<IActionResult> UpdateOrderItem ([FromBody] MPurchaseOrderItem item) {
                    if (ModelState.IsValid){
                        CosmosDbHandler<MPurchaseOrderItem> handler = CosmosDbHandler<MPurchaseOrderItem>.CreateCosmosHandlerInstance();

                        try {
                            await handler.UpdateItem(item, item.id, item.createdBy);
                            return new OkResult();
                        }
                        catch(Exception e) {
                            Console.WriteLine(e.Message);
                            return new BadRequestResult();
                        }
                    } 

                    return new BadRequestResult();

                }

                [HttpPut]
                [Route("updateorderinfo")]
                public async Task<IActionResult> updateorderinfo([FromBody] MPurchaseOrder order)
                    {
                        if (order == null) {
                            return new OkResult();
                        }

                        CosmosDbHandler<MPurchaseOrder> handler = CosmosDbHandler<MPurchaseOrder>.CreateCosmosHandlerInstance("purchaseorderitems", "orderinformation"); 

                        try {
                            await handler.UpdateItem(order, order.id, order.id);
                            return new OkObjectResult(order);
                        }
                        catch(Exception Ex) {
                            return new BadRequestObjectResult(Ex.Message);
                        }
                    }


                    
                    
                [HttpDelete]
                [Route("removeorderitem")]

                public async Task<IActionResult> removeorderitem(MPurchaseOrderItem item) 
                    {
                        if (ModelState.IsValid){
                            CosmosDbHandler<MPurchaseOrderItem> handler = CosmosDbHandler<MPurchaseOrderItem>.CreateCosmosHandlerInstance();

                            try {
                                await handler.RemoveItem(item.id, item.createdBy);
                                return new OkResult();
                            }
                            catch (Exception X) {
                                Console.WriteLine(X.Message);
                                return new BadRequestResult();
                            }

                        }

                        return new BadRequestResult();
                    }

                [HttpGet]
                [Route("orders")]
                public async Task<IActionResult> GetAll()
                {

                    try
                        {

                            List<Hashtable> orders = new List<Hashtable>();

                            using (_connection)
                                {
                                    //Connect to database then read booking records
                                    _connection.OpenAsync().Wait();

                                    using (SqlCommand command = new SqlCommand("spGetAllOrders", _connection))
                                    {
                                        command.CommandType = CommandType.StoredProcedure;

                                        using (SqlDataReader reader = await command.ExecuteReaderAsync()){
                                            while (reader.Read())
                                            {
                                                Hashtable order = new Hashtable();
                                                order.Add("costCenter", reader.GetString(0));
                                                order.Add("supplier", reader.GetString(1));
                                                order.Add("shipsTo", reader.GetString(2));
                                                order.Add("orderDate", reader.GetDateTime(3).Date.ToString());
                                                order.Add("orderAmount", reader.GetInt32(4));
                                                order.Add("deliveryPeriod", reader.GetInt32(5));
                                                order.Add("orderNumber", reader.GetInt32(6));
                                                order.Add("firstDeliveryDate", reader.GetDateTime(7).Date.ToString());
                                                order.Add("vehicleDetails", reader.GetString(8));
                                                orders.Add(order);
                                        

                                            }
                                        }

                                    }

                                }

                            return new OkObjectResult(orders);

                        }
                    catch(Exception ex)
                        {
                            return new BadRequestObjectResult(ex.Message);
                        }

                }

                [HttpPost]
                [Route("addorderinformation")]
                public async Task<IActionResult> addorderinformation([FromBody] MPurchaseOrder order)
                    {
                        CosmosDbHandler<MPurchaseOrder> handler = CosmosDbHandler<MPurchaseOrder>.CreateCosmosHandlerInstance("purchaseorderitems", "orderinformation");
                        try {
                            await handler.InsertItem(order, order.id);
                            return new OkResult();
                        }
                        catch (Exception Ex){
                            return new BadRequestObjectResult(Ex.Message);
                        }
                    }


                [HttpGet]
                [Route("getorder")]
                public async Task<IActionResult> getorder([FromQuery] int orderNumber)
                    {
                        Hashtable order = new Hashtable();

                        try {
                            using(_connection) {

                                _connection.OpenAsync().Wait();

                                using (SqlCommand command = new SqlCommand("spGetPurchaseOrderInfo", _connection))
                                    {
                                        command.CommandType = CommandType.StoredProcedure;
                                        command.Parameters.AddWithValue("orderNumber", SqlDbType.Int).Value = orderNumber;
                                        using (SqlDataReader reader = await command.ExecuteReaderAsync()){
                                            while (reader.Read()) {
                                                Hashtable formInfo = new Hashtable();
                                                formInfo.Add("costCenter", reader.GetString(0));
                                                formInfo.Add("supplier", reader.GetString(1));
                                                formInfo.Add("shipsTo", reader.GetString(2));
                                                formInfo.Add("orderDate", reader.GetDateTime(3).Date.ToString());
                                                formInfo.Add("orderAmount", reader.GetInt32(4));
                                                formInfo.Add("deliveryPeriod", reader.GetInt32(5));
                                                formInfo.Add("orderNumber", reader.GetInt32(6));
                                                formInfo.Add("firstDeliveryDate", reader.GetDateTime(7).Date.ToString());
                                                formInfo.Add("vehicleDetails", reader.GetString(8));
                                                formInfo.Add("narration", reader.GetString(9));
                                                order.Add("formInfo", formInfo);
                                            }
                                        }
                                    }

                                using (SqlCommand command = new SqlCommand("spGetPurchaseOrderItems", _connection)){
                                    List<Hashtable> orderitems = new List<Hashtable>();
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("orderNumber", SqlDbType.Int).Value = orderNumber;
                                    using (SqlDataReader reader = await command.ExecuteReaderAsync()){
                                        while (reader.Read()) {
                                            Hashtable orderitem = new Hashtable();
                                            orderitem.Add("item", reader.GetString(0));
                                            orderitem.Add("quantity", reader.GetInt32(1));
                                            orderitem.Add("unitCost", reader.GetDouble(2));
                                            orderitem.Add("extendedCost", reader.GetDouble(3));
                                            orderitem.Add("taxAmount", reader.GetDouble(4));
                                            orderitem.Add("discountAmount", reader.GetDouble(5));
                                            orderitem.Add("lineTotal", reader.GetDouble(6));
                                            orderitem.Add("id", reader.GetString(7));
                                            orderitem.Add("partitionKey", reader.GetInt32(8));
                                            orderitem.Add("createdBy", reader.GetString(9));
                                            orderitems.Add(orderitem);
                                        }
                                    }
                                    order.Add("tableData", orderitems);
                                }
                            }
                            return new OkObjectResult(order);
                        }
                        catch (Exception Ex) {
                            Console.WriteLine(Ex.Message);
                            return new BadRequestResult();
                        }
                    }

                [HttpPut]
                [Route("updateorder")]
                public async Task<IActionResult> updateOrder(CompletePurchaseOrder order)
                    {
                        MPurchaseOrder orderInformation = order.FormData ?? new MPurchaseOrder();
                        List<MPurchaseOrderItem> tableData = order.TableData ?? new List<MPurchaseOrderItem>();
                        Hashtable response = new Hashtable();

                        using (_connection)
                            {
                                _connection.OpenAsync().Wait();
                                
                                using (SqlCommand command = new SqlCommand("spInsertPurchaseOrder", _connection))
                                    {
                                        command.CommandType = CommandType.StoredProcedure;
                                        command.Parameters.AddWithValue("CostCenter", SqlDbType.NVarChar).Value = orderInformation.CostCenter;
                                        command.Parameters.AddWithValue("Supplier", SqlDbType.NVarChar).Value = orderInformation.Supplier;
                                        command.Parameters.AddWithValue("ShipsTo", SqlDbType.NVarChar).Value = orderInformation.ShipsTo;
                                        command.Parameters.AddWithValue("OrderAmount", SqlDbType.NVarChar).Value = orderInformation.OrderAmount;
                                        command.Parameters.AddWithValue("FirstDeliveryDate", SqlDbType.NVarChar).Value = orderInformation.FirstDeliveryDate;
                                        command.Parameters.AddWithValue("Narration", SqlDbType.NVarChar).Value = orderInformation.Narration;
                                        command.Parameters.AddWithValue("OrderDate", SqlDbType.NVarChar).Value = orderInformation.OrderDate;
                                        command.Parameters.AddWithValue("DeliveryPeriod", SqlDbType.NVarChar).Value = orderInformation.DeliveryPeriod;
                                        command.Parameters.AddWithValue("VehicleDetails", SqlDbType.NVarChar).Value = orderInformation.VehicleDetails;
                                        command.Parameters.AddWithValue("OrderNumber", SqlDbType.NVarChar).Value = orderInformation.OrderNumber;                      

                                    
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
                                        foreach (var PurchaseItem in tableData)
                                            {
                                                using (SqlCommand command = new SqlCommand("spInsertPurchaseOrderItem", _connection))
                                                    {
                                                        command.CommandType = CommandType.StoredProcedure;
                                                        command.Parameters.AddWithValue("item", SqlDbType.NVarChar).Value = PurchaseItem.item;
                                                        command.Parameters.AddWithValue("quantity", SqlDbType.Int).Value = PurchaseItem.quantity;
                                                        command.Parameters.AddWithValue("unitCost", SqlDbType.Float).Value = PurchaseItem.unitCost;
                                                        command.Parameters.AddWithValue("extendedCost", SqlDbType.Float).Value = PurchaseItem.extendedCost;
                                                        command.Parameters.AddWithValue("taxAmount", SqlDbType.Float).Value = PurchaseItem.taxAmount;
                                                        command.Parameters.AddWithValue("discountAmount", SqlDbType.Float).Value = PurchaseItem.discountAmount;
                                                        command.Parameters.AddWithValue("lineTotal", SqlDbType.Float).Value = PurchaseItem.lineTotal;
                                                        command.Parameters.AddWithValue("partitionKey", SqlDbType.Int).Value = orderInformation.OrderNumber;
                                                        command.Parameters.AddWithValue("id", SqlDbType.NVarChar).Value = PurchaseItem.id;
                                                        command.Parameters.AddWithValue("createdBy", SqlDbType.NVarChar).Value = PurchaseItem.createdBy;

                                                        using(SqlDataReader reader = await command.ExecuteReaderAsync()){
                                                            continue;
                                                        };

                                                    }
                                                
                                            }
                                        response.Add("tableStatus", 1);

                                    }
                                    catch (Exception Ex){

                                        Console.WriteLine(Ex.Message);
                                        response.Add("tableStatus", Ex.Message);

                                    }

                                return new OkObjectResult(response);

                            }
                    }


            }


    }

