using MySql.Data.MySqlClient;

namespace AdminManager.Data
{
    public class Database
    {
        private readonly IConfiguration _configuration;

        public Database(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MySqlConnection GetConnection()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            MySqlConnection connection = new MySqlConnection(connectionString);

            return connection;
        }
    }
}