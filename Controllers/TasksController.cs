using AdminManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace AdminManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly Database _database;

        public TasksController(Database database)
        {
            _database = database;
        }



        [HttpGet]
        public IActionResult GetTasks()
        {
            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            // string sql = "SELECT Title FROM WorkTasks";
            string sql = "SELECT * FROM WorkTasks";

            MySqlCommand command = new MySqlCommand(sql, connection);

            MySqlDataReader reader = command.ExecuteReader();

            List<object> tasks = new List<object>();

            while (reader.Read())
            {
                //string title = reader["Title"].ToString();

                tasks.Add(new
                {
                    Id = reader["Id"],
                    Title = reader["Title"],
                    Description = reader["Description"],
                    Status = reader["Status"],
                    Deadline = reader["Deadline"],
                    ProjectId = reader["ProjectId"],
                    AssignedUserId = reader["AssignedUserId"]
                });
            }

            connection.Close();

            return Ok(tasks);
        }



        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "SELECT * FROM WorkTasks";
            string sql = "SELECT * FROM WorkTasks WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            MySqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                var task = new
                {
                    Id = reader["Id"],
                    Title = reader["Title"],
                    Description = reader["Description"],
                    Status = reader["Status"],
                    Deadline = reader["Deadline"],
                    ProjectId = reader["ProjectId"],
                    AssignedUserId = reader["AssignedUserId"]
                };

                connection.Close();

                return Ok(task);
            }

            connection.Close();

            return NotFound("Task not found");
        }



        [HttpPost]
        public IActionResult CreateTask(string title, string description, string status, DateTime deadline, int projectId, int assignedUserId)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            //if(role != "Admin")
            //{
            //    return Unauthorized();
            //}

            if (role != "Admin" && role != "Manager")
            {
                return Unauthorized("Only admin or manager can create task");
            }


            //if(title == "")
            //{
            //    return BadRequest("error");
            //}

            if (title == "")
            {
                return BadRequest("Title is required");
            }

            if (description == "")
            {
                return BadRequest("Description is required");
            }


            //if(status == "")
            //{
            //    status = "Pending";
            //}

            if (status != "Pending" && status != "InProgress" && status != "Completed")
            {
                return BadRequest("Status must be Pending, InProgress or Completed");
            }


            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "INSERT INTO WorkTasks VALUES (1, 'test', 'test', 'Pending', '2026-01-01', 1, 1)";

            string sql = "INSERT INTO WorkTasks (Title, Description, Status, Deadline, ProjectId, AssignedUserId) VALUES (@Title, @Description, @Status, @Deadline, @ProjectId, @AssignedUserId)";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@Deadline", deadline);
            command.Parameters.AddWithValue("@ProjectId", projectId);
            command.Parameters.AddWithValue("@AssignedUserId", assignedUserId);

            command.ExecuteNonQuery();

            connection.Close();

            return Ok("Task created successfully");
        }



        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, string title, string description, string status, DateTime deadline, int projectId, int assignedUserId)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role != "Admin" && role != "Manager")
            {
                return Unauthorized("Only admin or manager can update task");
            }

            if (title == "")
            {
                return BadRequest("Title is required");
            }

            if (description == "")
            {
                return BadRequest("Description is required");
            }

            //if (status == "Done")
            //{
            //    status = "Completed";
            //}

            if (status != "Pending" && status != "InProgress" && status != "Completed")
            {
                return BadRequest("Status must be Pending, InProgress or Completed");
            }


            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "UPDATE WorkTasks SET Status = @Status WHERE Id = @Id";

            string sql = "UPDATE WorkTasks SET Title = @Title, Description = @Description, Status = @Status, Deadline = @Deadline, ProjectId = @ProjectId, AssignedUserId = @AssignedUserId WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@Deadline", deadline);
            command.Parameters.AddWithValue("@ProjectId", projectId);
            command.Parameters.AddWithValue("@AssignedUserId", assignedUserId);
            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("Task updated successfully");
            }

            return NotFound("Task not found");
        }



        [HttpPut("change-status/{id}")]
        public IActionResult ChangeTaskStatus(int id, string status)
        {
            //string oldStatus = "Pending";

            if (status != "Pending" && status != "InProgress" && status != "Completed")
            {
                return BadRequest("Status must be Pending, InProgress or Completed");
            }

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            string sql = "UPDATE WorkTasks SET Status = @Status WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("Task status changed successfully");
            }
            else
            {
                return NotFound("Task not found");
            }
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role != "Admin" && role != "Manager")
            {
                return Unauthorized("Only admin or manager can delete task");
            }

            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "DELETE FROM WorkTasks";
            string sql = "DELETE FROM WorkTasks WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            int result = command.ExecuteNonQuery();

            connection.Close();

            if (result > 0)
            {
                return Ok("Task deleted successfully");
            }
            else
            {
                return NotFound("Task not found");
            }
        }
    }
}