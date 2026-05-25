using AdminManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace AdminManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly Database _database;

        public UsersController(Database database)
        {
            _database = database;
        }



        [HttpGet]
        public IActionResult GetAllUsers()
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            //if (role == "Admin")
            //{
            //    return Ok();
            //}

            if (role != "Admin")
            {
                return Unauthorized("Only admin can see all users");
            }

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "SELECT * FROM Users";
            string sql = "SELECT Id, FullName, Email, Role FROM Users";

            MySqlCommand command = new MySqlCommand(sql, connection);

            MySqlDataReader reader = command.ExecuteReader();

            List<object> users = new List<object>();

            while (reader.Read())
            {
                //string name = reader["FullName"].ToString();

                users.Add(new
                {
                    Id = reader["Id"],
                    FullName = reader["FullName"],
                    Email = reader["Email"],
                    Role = reader["Role"]
                });
            }

            connection.Close();

            return Ok(users);
        }



        [HttpGet("me")]
        public IActionResult GetMyInfo()
        {
            string id = User.FindFirst("Id")?.Value;

            //string id = "1";

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "SELECT * FROM Users WHERE Id = @Id";
            string sql = "SELECT Id, FullName, Email, Role FROM Users WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            MySqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                var user = new
                {
                    Id = reader["Id"],
                    FullName = reader["FullName"],
                    Email = reader["Email"],
                    Role = reader["Role"]
                };

                connection.Close();

                return Ok(user);
            }

            connection.Close();

            return NotFound("User not found");
        }



        [HttpPut("change-role/{id}")]
        public IActionResult ChangeRole(int id, string newRole)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            //if(newRole == "admin")
            //{
            //    newRole = "Admin";
            //}

            if (role != "Admin")
            {
                return Unauthorized("Only admin can change roles");
            }

            //if (newRole == "")
            //{
            //    return BadRequest();
            //}

            if (newRole != "Admin" && newRole != "Manager" && newRole != "Employee")
            {
                return BadRequest("Role must be Admin, Manager or Employee");
            }


            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "UPDATE Users SET Role = 'Admin' WHERE Id = @Id";
            string sql = "UPDATE Users SET Role = @Role WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Role", newRole);
            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("Role changed successfully");
            }
            else
            {
                return NotFound("User not found");
            }
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Unauthorized("Only admin can delete users");
            }

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "DELETE FROM Users";
            string sql = "DELETE FROM Users WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("User deleted successfully");
            }

            return NotFound("User not found");
        }
    }
}