using AdminManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace AdminManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly Database _database;

        public ProjectsController(Database database)
        {
            _database = database;
        }



        [HttpGet]
        public IActionResult GetProjects()
        {
            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "SELECT Name FROM Projects";
            string sql = "SELECT * FROM Projects";

            MySqlCommand command = new MySqlCommand(sql, connection);

            MySqlDataReader reader = command.ExecuteReader();

            List<object> projects = new List<object>();

            while (reader.Read())
            {
                //var p = reader["Name"].ToString();

                projects.Add(new
                {
                    Id = reader["Id"],
                    Name = reader["Name"],
                    Description = reader["Description"],
                    StartDate = reader["StartDate"],
                    EndDate = reader["EndDate"],
                    ManagerId = reader["ManagerId"]
                });
            }

            connection.Close();

            return Ok(projects);
        }



        [HttpGet("{id}")]
        public IActionResult GetProjectById(int id)
        {
            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            // string sql = "SELECT * FROM Projects";
            string sql = "SELECT * FROM Projects WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            MySqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                var project = new
                {
                    Id = reader["Id"],
                    Name = reader["Name"],
                    Description = reader["Description"],
                    StartDate = reader["StartDate"],
                    EndDate = reader["EndDate"],
                    ManagerId = reader["ManagerId"]
                };

                connection.Close();

                return Ok(project);
            }

            connection.Close();

            return NotFound("Project not found");
        }



        [HttpPost]
        public IActionResult CreateProject(string name, string description, DateTime startDate, DateTime endDate, int managerId)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            //if (role != "Admin")
            //{
            //    return Unauthorized();
            //}

            if (role != "Admin" && role != "Manager")
            {
                return Unauthorized("Only admin or manager can create project");
            }


            //if(name == "")
            //{
            //    return BadRequest();
            //}

            if (name == "")
            {
                return BadRequest("Name is required");
            }

            if (description == "")
            {
                return BadRequest("Description is required");
            }

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "INSERT INTO Projects VALUES (1, 'test', 'test', '2026-01-01', '2026-02-01', 1)";

            string sql = "INSERT INTO Projects (Name, Description, StartDate, EndDate, ManagerId) VALUES (@Name, @Description, @StartDate, @EndDate, @ManagerId)";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);
            command.Parameters.AddWithValue("@ManagerId", managerId);

            command.ExecuteNonQuery();

            connection.Close();

            return Ok("Project created successfully");
        }



        [HttpPut("{id}")]
        public IActionResult UpdateProject(int id, string name, string description, DateTime startDate, DateTime endDate, int managerId)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role != "Admin" && role != "Manager")
            {
                return Unauthorized("Only admin or manager can update project");
            }

            if (name == "")
            {
                return BadRequest("Name is required");
            }

            if (description == "")
            {
                return BadRequest("Description is required");
            }


            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "UPDATE Projects SET Name = @Name WHERE Id = @Id";

            string sql = "UPDATE Projects SET Name = @Name, Description = @Description, StartDate = @StartDate, EndDate = @EndDate, ManagerId = @ManagerId WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);
            command.Parameters.AddWithValue("@ManagerId", managerId);
            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("Project updated successfully");
            }

            return NotFound("Project not found");
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteProject(int id)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            //string idText = id.ToString();

            if (role != "Admin" && role != "Manager")
            {
                return Unauthorized("Only admin or manager can delete project");
            }

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "DELETE FROM Projects";
            string sql = "DELETE FROM Projects WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("Project deleted successfully");
            }
            else
            {
                return NotFound("Project not found");
            }
        }
    }
}