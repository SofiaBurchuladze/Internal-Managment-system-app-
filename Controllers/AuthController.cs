using AdminManager.Data;
using AdminManager.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdminManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Database _database;
        private readonly IConfiguration _configuration;

        public AuthController(Database database, IConfiguration configuration)
        {
            _database = database;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public IActionResult Register(RegisterDto user)
        {
            //if(user == null)
            //{
            //    return BadRequest("error");
            //}

            // if (user.FullName == "" || user.Email == "" || user.Password == "")
            // {
            //     return BadRequest("fill fields");
            // }

            if (user.FullName == "")
            {
                return BadRequest("Full name is required");
            }

            if (user.Email == "")
            {
                return BadRequest("Email is required");
            }

            if (user.Password == "")
            {
                return BadRequest("Password is required");
            }

            if (user.Role == "")
            {
                return BadRequest("Role is required");
            }


            //if (user.Role == "Admin")
            //{
            //    return Ok();
            //}

            if (user.Role != "Admin" && user.Role != "Manager" && user.Role != "Employee")
            {
                return BadRequest("Role must be Admin, Manager or Employee");
            }


            MySqlConnection connection = _database.GetConnection();
            connection.Open();


            string checkSql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

            MySqlCommand checkCommand = new MySqlCommand(checkSql, connection);
            checkCommand.Parameters.AddWithValue("@Email", user.Email);

            int count = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (count > 0)
            {
                connection.Close();
                return BadRequest("Email already exists");
            }


            //string sql = "INSERT INTO Users VALUES (1, 'test', 'test@gmail.com', '123', 'Admin')";
            //MySqlCommand command = new MySqlCommand(sql, connection);
            //command.ExecuteNonQuery();

            string sql = "INSERT INTO Users (FullName, Email, Password, Role) VALUES (@FullName, @Email, @Password, @Role)";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@Role", user.Role);

            command.ExecuteNonQuery();

            connection.Close();

            return Ok("User registered successfully");
        }



        [HttpPost("login")]
        public IActionResult Login(LoginDto login)
        {
            //if(login == null)
            //{
            //    return BadRequest();
            //}

            if (login.Email == "")
            {
                return BadRequest("Email is required");
            }

            if (login.Password == "")
            {
                return BadRequest("Password is required");
            }


            MySqlConnection connection = _database.GetConnection();

            connection.Open();

            //string sql = "SELECT * FROM Users WHERE Email = '" + login.Email + "' AND Password = '" + login.Password + "'";

            string sql = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";

            MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Email", login.Email);
            command.Parameters.AddWithValue("@Password", login.Password);

            MySqlDataReader reader = command.ExecuteReader();


            if (reader.Read())
            {
                string id = reader["Id"].ToString();
                string fullName = reader["FullName"].ToString();
                string email = reader["Email"].ToString();
                string role = reader["Role"].ToString();

                // string token = "token";
                // string token = CreateToken(email, role);

                string token = CreateToken(id, fullName, email, role);

                connection.Close();

                return Ok(new
                {
                    message = "Login successful",
                    token = token,
                    fullName = fullName,
                    email = email,
                    role = role
                });
            }
            else
            {
                connection.Close();

                return BadRequest("Email or password is incorrect");
            }
        }



        private string CreateToken(string id, string fullName, string email, string role)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim("Id", id));
            claims.Add(new Claim(ClaimTypes.Name, fullName));
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Role, role));

            //claims.Add(new Claim("role", role));
            //claims.Add(new Claim("email", email));


            string keyText = _configuration["Jwt:Key"];

            //string keyText = "secret key for token";

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials
            );

            string tokenText = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenText;
        }
    }
}