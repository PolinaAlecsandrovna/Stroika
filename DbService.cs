using Stroika.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
namespace Stroika
{
    public class DbService
    {
        private const string ConnString = "Host=localhost;Port=5432;Database=stroika;Username=postgres;Password=Password";

        public List<OrderViewModel> GetOrders()
        {
            var list = new List<OrderViewModel>();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        o.id, 
                        pt.name as p_type, 
                        p.company_name, 
                        c.name as city, 
                        p.street, 
                        p.building, 
                        p.phone, 
                        p.rating, 
                        COALESCE(SUM(oi.quantity * oi.unit_price), 0) as total_cost
                    FROM partner_orders o
                    JOIN partners p ON o.partner_id = p.id
                    JOIN partner_types pt ON p.partner_type_id = pt.id
                    JOIN citys c ON p.city_id = c.id
                    LEFT JOIN order_items oi ON o.id = oi.order_id
                    GROUP BY o.id, pt.name, p.company_name, c.name, p.street, p.building, p.phone, p.rating
                    ORDER BY o.id DESC";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new OrderViewModel
                        {
                            Id = reader.GetInt32(0),
                            PartnerType = reader.GetString(1),
                            PartnerName = reader.GetString(2),
                            Address = $"{reader.GetString(3)}, ул. {reader.GetString(4)}, д. {reader.GetString(5)}",
                            Phone = reader.GetString(6),
                            Rating = reader.GetInt32(7),
                            TotalCost = reader.GetDecimal(8)
                        });
                    }
                }
            }
            return list;
        }

        public List<PartnerShortModel> GetAllPartners()
        {
            var list = new List<PartnerShortModel>();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, company_name FROM partners ORDER BY company_name", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PartnerShortModel { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                    }
                }
            }
            return list;
        }

        public List<ProductModel> GetAllProducts()
        {
            var list = new List<ProductModel>();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, name, COALESCE(min_partner_price, 0) FROM products ORDER BY name", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ProductModel
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            MinPartnerPrice = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return list;
        }

        public void CreateOrder(int partnerId, List<OrderItemModel> items)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlOrder = @"INSERT INTO partner_orders (partner_id, order_status_id, order_date) 
                                            VALUES (@pid, 1, CURRENT_DATE) RETURNING id";

                        int orderId;
                        using (var cmd = new NpgsqlCommand(sqlOrder, conn))
                        {
                            cmd.Parameters.AddWithValue("pid", partnerId);
                            orderId = (int)cmd.ExecuteScalar();
                        }

                        foreach (var item in items)
                        {
                            string sqlItem = @"INSERT INTO order_items (order_id, product_id, quantity, unit_price) 
                                               VALUES (@oid, @prodId, @qty, @price)";
                            using (var cmd = new NpgsqlCommand(sqlItem, conn))
                            {
                                cmd.Parameters.AddWithValue("oid", orderId);
                                cmd.Parameters.AddWithValue("prodId", item.ProductId);
                                cmd.Parameters.AddWithValue("qty", item.Quantity);
                                cmd.Parameters.AddWithValue("price", item.Price);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public int GetPartnerIdByOrderId(int orderId)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT partner_id FROM partner_orders WHERE id = @oid", conn))
                {
                    cmd.Parameters.AddWithValue("oid", orderId);
                    var result = cmd.ExecuteScalar();
                    return result != null ? (int)result : 0;
                }
            }
        }

        public List<OrderItemModel> GetOrderItems(int orderId)
        {
            var list = new List<OrderItemModel>();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                string sql = @"
                    SELECT oi.product_id, p.name, oi.unit_price, oi.quantity
                    FROM order_items oi
                    JOIN products p ON oi.product_id = p.id
                    WHERE oi.order_id = @oid";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("oid", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new OrderItemModel
                            {
                                ProductId = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                Quantity = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return list;
        }
        public void UpdateOrder(int orderId, int partnerId, List<OrderItemModel> items)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlUpdate = "UPDATE partner_orders SET partner_id = @pid WHERE id = @oid";
                        using (var cmd = new NpgsqlCommand(sqlUpdate, conn))
                        {
                            cmd.Parameters.AddWithValue("pid", partnerId);
                            cmd.Parameters.AddWithValue("oid", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        string sqlDelete = "DELETE FROM order_items WHERE order_id = @oid";
                        using (var cmd = new NpgsqlCommand(sqlDelete, conn))
                        {
                            cmd.Parameters.AddWithValue("oid", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var item in items)
                        {
                            string sqlItem = @"INSERT INTO order_items (order_id, product_id, quantity, unit_price) 
                                               VALUES (@oid, @prodId, @qty, @price)";
                            using (var cmd = new NpgsqlCommand(sqlItem, conn))
                            {
                                cmd.Parameters.AddWithValue("oid", orderId);
                                cmd.Parameters.AddWithValue("prodId", item.ProductId);
                                cmd.Parameters.AddWithValue("qty", item.Quantity);
                                cmd.Parameters.AddWithValue("price", item.Price);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

}
