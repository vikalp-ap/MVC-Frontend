using Npgsql;
using Microsoft.AspNetCore.Mvc;
using MVCFrontend.Models;

namespace MVCFrontend.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly string? _connectionString;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("UsersCon");
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AddUser()
    {
        return View();
    }

    [HttpPost]
    public IActionResult AddUser(UsersModel user)
    {
        try
        {
            string query = @"INSERT INTO users (name, email, age) VALUES (@Name, @Email, @Age);";
            using (NpgsqlConnection myCon = new NpgsqlConnection(_connectionString))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Name", user.Name ?? (object)DBNull.Value);
                    myCommand.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    myCommand.Parameters.AddWithValue("@Age", user.Age);
                    myCommand.ExecuteNonQuery();
                }
            }
            return RedirectToAction("GetAllData");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    public IActionResult GetAllData()
    {
        try
        {
            string query = @"SELECT * FROM users ORDER BY id ASC";
            List<UsersModel> users = new List<UsersModel>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                {
                    NpgsqlDataReader myReader = command.ExecuteReader();
                    while (myReader.Read())
                    {
                        UsersModel userInfo = new UsersModel()
                        {
                            Id = myReader.GetInt32(3),
                            Name = myReader.GetString(0),
                            Email = myReader.GetString(1),
                            Age = myReader.GetInt32(2),
                        };
                        users.Add(userInfo);
                    }
                    myReader.Close();
                    conn.Close();
                }
            }
            return View(users);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    public IActionResult Delete(int id)
    {
        try
        {
            string query = "DELETE FROM users WHERE id = @Id;";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    int nrows = command.ExecuteNonQuery();
                    System.Console.WriteLine(nrows + " rows deleted");
                    conn.Close();
                }
                return RedirectToAction("GetAllData");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("Error occured");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    public IActionResult ViewDetails(int id)
    {
        try
        {
            string query = "SELECT * FROM users where id = @id";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                UsersModel userInfo = new UsersModel();
                using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("id", id);
                    NpgsqlDataReader myReader = command.ExecuteReader();
                    while (myReader.Read())
                    {
                        userInfo = new UsersModel()
                        {
                            Id = myReader.GetInt32(3),
                            Name = myReader.GetString(0),
                            Email = myReader.GetString(1),
                            Age = myReader.GetInt32(2),
                        };
                    }
                    myReader.Close();
                    conn.Close();
                }
                return View(userInfo);
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("Error occured");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

    }
}
