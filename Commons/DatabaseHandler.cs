using System.Data;
using System.Data.SqlClient;

namespace accountservice.Commons
{
    //This namespace alongside its classes handles the boilerplate associated with database connection and value processing
    public class DatabaseHandler
    {
        SqlConnection? _connection;
        SqlCommand? command;

        private readonly List<Parameter> _parameters;

        //Enhance singleton capabilities
        private  static readonly DatabaseHandler _databaseHandler;

        static DatabaseHandler()
        {
            _databaseHandler = new DatabaseHandler();
        }
        private DatabaseHandler() 
        {
            _parameters = new List<Parameter>();
            
        }

        public static DatabaseHandler GetDAtabaseHandlerInstance()
        {
            //Clear parameter list
            _databaseHandler.Parameters.Clear();
            return _databaseHandler;
        }

        public List<Parameter> Parameters { get { return _parameters; } }

        public void CloseResources()
        {
            command?.DisposeAsync();
            _connection?.DisposeAsync();

            
        }

        public async Task<SqlDataReader> ExecuteProcedure(string connectionstring, string procedureQuery)
        {
            try
            {
                _connection = new SqlConnection(connectionstring);
                {
                    //Connect to database then read booking records
                    await _connection.OpenAsync();

                    command = new SqlCommand(procedureQuery, _connection);
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        foreach(Parameter parameter in _parameters)
                        {
                            switch (parameter.Type)
                            {
                                case SqlDbType.Int:
                                    command.Parameters.AddWithValue(parameter.Name, parameter.Type).Value = int.Parse(parameter.Value??"-1");

                                    break;

                                case SqlDbType.TinyInt:
                                    command.Parameters.AddWithValue(parameter.Name, parameter.Type).Value = short.Parse(parameter.Value ?? "-1");
                                    break;

                                case SqlDbType.BigInt:
                                    command.Parameters.AddWithValue(parameter.Name, parameter.Type).Value = long.Parse(parameter.Value ?? "-1");
                                    break;

                                case SqlDbType.Money:
                                    command.Parameters.AddWithValue(parameter.Name, parameter.Type).Value = decimal.Parse(parameter.Value ?? "-1");
                                    break;

                                default:
                                    command.Parameters.AddWithValue(parameter.Name, parameter.Type).Value = parameter.Value;
                                    break;
                            }
                           
                        }

                        SqlDataReader reader = await command.ExecuteReaderAsync();

                        return reader;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
           
        }


    }


    //Holds a parameter value, name and sql type
    public class Parameter
    {
        public string? Name { get; set; }

        public string? Value { get; set; }

        public SqlDbType Type { get; set; }

    }
}
