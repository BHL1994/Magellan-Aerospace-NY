using System;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Port=5342;Username=postgres;Password=brienh;Database=Part";

        [HttpPost]
        public IActionResult CreateItem([FromBody] dynamic request)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO item (item_name, parent_item, cost, req_date) VALUES (@item_name, @parent_item, @cost, @req_date) RETURNING id";
                        cmd.Parameters.AddWithValue("item_name", request.item_name);
                        cmd.Parameters.AddWithValue("parent_item", request.parent_item);
                        cmd.Parameters.AddWithValue("cost", request.cost);
                        cmd.Parameters.AddWithValue("req_date", request.req_date);
                        int id = (int)cmd.ExecuteScalar();
                        return Ok(new { id });
                    }
                }
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetItem(int id)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT id, item_name, parent_item, cost, req_date FROM item WHERE id = @id";
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int? parentItem = null;
                            if (!reader.IsDBNull(2))
                            {
                                parentItem = reader.GetInt32(2);
                            }

                            return Ok(new
                            {
                                id = reader.GetInt32(0),
                                item_name = reader.GetString(1),
                                parent_item = parentItem,
                                cost = reader.GetInt32(3),
                                req_date = reader.GetDateTime(4)
                            });
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }

        [HttpGet("TotalCost/{item_name}")]
        public IActionResult GetTotalCost(string item_name)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT Get_Total_Cost(@item_name)";
                        cmd.Parameters.AddWithValue("item_name", item_name);
                        int? total_cost = cmd.ExecuteScalar() as int?;
                        return Ok(new { total_cost });
                    }
                }
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}