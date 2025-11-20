using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Data.SqlClient;
using ShopOnline.Helpers.Enums;
using ShopOnline.Models;
using System.Data;

namespace ShopOnline.Helpers
{
    public static class DbHelper
    {
        private const string _masterConnectionString = "Server=PC1\\SQLEXPRESS;User Id=sa;Password=sa;Database=master;TrustServerCertificate=True;Trusted_Connection=True";

        private const string _shopConnectionString = "Server=PC1\\SQLEXPRESS;User Id=sa;Password=sa;Database=ShopOnline;TrustServerCertificate=True;Trusted_Connection=True";

        public static void InitializeDb()
        {
            CreateDb();
            CreateProductsTable();
        }

        private static void CreateDb()
        {
            using var connection = new SqlConnection(_masterConnectionString);

            connection.Open();

            //var command = new SqlCommand("CREATE DATABASE ...",connection);

            var commandText = """
                CREATE DATABASE ShopOnline;
                """;

            var command = connection.CreateCommand();

            command.CommandText = commandText;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == (int)ECodiciDb.DatabaseEsistente)
                {
                    Console.WriteLine("Il database è già stato creato.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        private static void CreateProductsTable()
        {
            using var connection = new SqlConnection(_shopConnectionString);

            connection.Open();

            var commandText = """
                CREATE TABLE Products (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    Name NVARCHAR(25) NOT NULL,
                    Description NVARCHAR(2000) NOT NULL,
                    Price DECIMAL(6,2)
                );
                """;

            var command = connection.CreateCommand();

            command.CommandText = commandText;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == (int)ECodiciDb.TabellaEsistente)
                {
                    Console.WriteLine("La tabella è già stata creata.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        public static List<Product> GetProducts()
        {
            using var connection = new SqlConnection(_shopConnectionString);
            connection.Open();

            var commandText = """
                SELECT * FROM Products;
                """;

            var command = connection.CreateCommand();

            command.CommandText = commandText;

            using var reader = command.ExecuteReader();

            var products = new List<Product>();

            while (reader.Read())
            {
                var id = reader.GetGuid(0);
                var name = reader.GetString(1);
                var description = reader.GetString(2);
                var price = reader.GetDecimal(3);

                var product = new Product()
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    Price = price,
                };

                products.Add(product);
            }

            return products;
        }

        public static bool AddProduct(Product product)
        {
            bool result = false;

            using var connection = new SqlConnection(_shopConnectionString);

            connection.Open();

            var commandText = """
                INSERT INTO Products VALUES (
                    @Id,
                    @Name,
                    @Description,
                    @Price
                );
                """;

            var command = connection.CreateCommand();

            command.CommandText = commandText;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 25);
            command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000);
            command.Parameters.Add("@Price", SqlDbType.Decimal).Precision = 6;
            command.Parameters["@Price"].Scale = 2;

            command.Prepare();

            command.Parameters["@Id"].Value = product.Id;
            command.Parameters["@Name"].Value = product.Name;
            command.Parameters["@Description"].Value = product.Description;
            command.Parameters["@Price"].Value = product.Price;

            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public static Product? GetProductById(Guid id)
        {
            using var connection = new SqlConnection(_shopConnectionString);

            connection.Open();

            var commandText = """
                SELECT * FROM Products WHERE Id = @Id;
                """;

            var command = connection.CreateCommand();

            command.CommandText = commandText;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);

            command.Prepare();

            command.Parameters["@Id"].Value = id;

            using var reader = command.ExecuteReader();

            Product? product = null;

            while (reader.Read())
            {
                var _id = reader.GetGuid(0);
                var name = reader.GetString(1);
                var description = reader.GetString(2);
                var price = reader.GetDecimal(3);

                product = new Product()
                {
                    Id = _id,
                    Name = name,
                    Description = description,
                    Price = price,
                };
            }

            return product;
        }

        public static bool DeleteProductById(Guid id)
        {
            var result = false;

            using var connection = new SqlConnection(_shopConnectionString);
            connection.Open();

            var commandText = """
                DELETE FROM Products WHERE Id = @Id;
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);

            command.Prepare();

            command.Parameters["@Id"].Value = id;

            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}
