using Microsoft.Azure.Cosmos;

namespace accountservice.Commons
{
    /// <summary>
    /// This cosmos database class as a central point to handle all cosmos db operations from a single point
    /// Create a cosmos db single class instance to handle particular operations
    /// </summary>
    public class CosmosDbHandler<Data>
    {

        private static string EndpointUri = "https://purchaseorderitems.documents.azure.com:443/";

        // The primary key for the Azure Cosmos account.
        private static string PrimaryKey = "UEyhDWw0UF9CweujkD8xlhtnhWpucIJHiElDrLa47gL77EwBfCMueYfeDcwiZPwvB3VyX6uignNBACDbPg1ohQ==";

        private static readonly string defaultDbId = "purchaseorderitems";
        private static readonly string defaultContainId = "orderitems";

        // The name of the database and container we will create
        private string _databaseId;
        private string _containerId;

        private Container _container;

        //Enhance singleton capabilities
        private static CosmosDbHandler<Data> handler;

        static CosmosDbHandler()
        {
            handler = new CosmosDbHandler<Data>();
        }


        private CosmosDbHandler(string databaseId, string containerId)
        {
            _containerId = containerId;
            _databaseId = databaseId;
        }

        public string SetEndPointUrl { set { EndpointUri= value; } }

        public string SetPrimaryKey { set { PrimaryKey = value; } }

        public CosmosDbHandler():this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Create an instance to handle Cosmos database operation for the specified database id and container
        /// If values are specified. Default values are used
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="containerId"></param>
        /// <returns></returns>
        public static CosmosDbHandler<Data> CreateCosmosHandlerInstance(string databaseId, string containerId)
        {

            if (handler != null)
            {
                handler._containerId = containerId;
                handler._databaseId = databaseId;

                handler.initializeDatabase();

                return handler;
            }
            else
            {
                handler = new CosmosDbHandler<Data>(databaseId, containerId);
                handler.initializeDatabase();
                return handler;
            }
        }

        public static CosmosDbHandler<Data> CreateCosmosHandlerInstance()
        {
           return CreateCosmosHandlerInstance(defaultDbId, defaultContainId);
        }

        public Container Container { get { return _container; } }

        public async Task<List<Data>> QuerySelector(string query)
        {
            List<Data> items = new List<Data>();
            
            QueryDefinition queryDefinition = new QueryDefinition(query);

            FeedIterator<Data> queryResultSetIterator = Container.GetItemQueryIterator<Data>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Data> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Data item in currentResultSet)
                {
                    items.Add(item); 
                }
            }

            return items;
        }

        /// <summary>
        /// Inserts an new item into your cosmos db database
        /// Throws items exists (409) conflict HTTP response and other related errors
        /// </summary>
        /// <param name="item">Specified generic item to be inserted</param>
        /// <param name="partitionKey">Partition key valaue as specified</param>
        /// <returns>True if the item is succefully inserted</returns>
        public async Task<bool> InsertItem(Data item, string partitionKey)
        {
            bool result;

            ItemResponse<Data> createdItem = await Container.CreateItemAsync(item, new PartitionKey(partitionKey));

            result = createdItem != null;

            return result;
        }

        public async Task<bool> UpdateItem(Data item, string itemId, string partitionKey)
        {
            bool result;

            ItemResponse<Data> updatedItem = await Container.ReadItemAsync<Data>(itemId, new PartitionKey(partitionKey));

            Console.WriteLine(updatedItem.Resource);

            updatedItem = await Container.ReplaceItemAsync<Data>(item, itemId, new PartitionKey(partitionKey));

            result = updatedItem != null;

            return result;

        }


        /// <summary>
        /// Removes/Deletes a specified item from your Cosmos db database
        /// </summary>
        /// <param name="itemId">string value representing itemid of the item to delete</param>
        /// <param name="partitionKey">Partition key used to add the item to the database</param>
        /// <returns>Response item that was deleted</returns>
        public async Task<Data> RemoveItem(string itemId, string partitionKey)
        {
            ItemResponse<Data> itemResponse = await Container.DeleteItemAsync<Data>(itemId, new PartitionKey(partitionKey));

            return itemResponse;
        }


        private void initializeDatabase()
        {
            CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            _container = cosmosClient.GetContainer(_databaseId, _containerId);
        }

        override public string ToString()
        {
            return _databaseId + ":" + _containerId;
        }
        
    }

}
