using System.Data;
using kol1.Models;
using Microsoft.Data.SqlClient;

namespace kol1.Services;

public class DeliveriesService : IDeliveriesService
{

    private readonly string _connectionString;

    public DeliveriesService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("main");
    }

    public async Task<bool> DoesDeliveryExistAsync(int id)
    {
        string commandStr = @"Select count(*) 
                            FROM Delivery 
                            WHERE delivery_id = @id";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        var result = reader.GetInt32(reader.GetOrdinal("delivery_id"));

        return result == 1;
    }

    public async Task<GetDeliveryDTO> GetDeliveryAsync(int id)
    {
        string commandStr = @"Select delivery.date, 
                           customer.first_name, customer.last_name, customer.date_of_birth,
                           driver.first_name, driver.last_name, driver.date_of_birth,
                           products.name, products.price, producsts_delivery.amount
                           FROM Delivery
                           INNER JOIN Customer ON Delivery.customer_id = Customer.customer_id
                           INNER JOIN Product_Delivery ON Delivery.delivery_id = Product_Delivery.delivery_id
                           INNER JOIN Driver ON Delivery.driver_id = Driver.driver_id
                           INNER JOIN Product ON Product_Delivery.product_id = Product.product_id
                           WHERE Delivery.delivery_id = @id;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        GetDeliveryDTO delivery = null;


        var productsDict = new Dictionary<string, ProductsDTO>();

        while (await reader.ReadAsync())
        {
            if (delivery == null)
            {
                delivery = new GetDeliveryDTO
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")), 
                    Customer = null,
                    Driver = null,
                    Products = new List<ProductsDTO>()
                };
            }

            if (delivery.Customer == null)
            {
                delivery.Customer = new CustomerDTO
                {
                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                    LastName = reader.GetString(reader.GetOrdinal("last_name")),
                    DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                };
            }

            if (delivery.Driver == null)
            {
                delivery.Driver = new DriverDTO()
                {
                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                    LastName = reader.GetString(reader.GetOrdinal("last_name")),
                    LicenseNumber = reader.GetString(reader.GetOrdinal("license_number")),
                };
            }

            var productStr = reader.GetString(reader.GetOrdinal("product_name"));

            if (!productsDict.TryGetValue(productStr, out var prod))
            {
                prod = new ProductsDTO
                {
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Amount = reader.GetInt32(reader.GetOrdinal("amount")),
                };
                productsDict[productStr] = prod;
                delivery.Products.Add(prod);
            }
        }
        return delivery;
    }

    public async Task<bool> AddNewDelivery(PostDeliveryDTO delivery)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();
    
        command.Connection = connection;
        await connection.OpenAsync();
    
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            var license_number = delivery.LicenceNumber;
            command.CommandText = @"SELECT driver_id 
                                    FROM Driver 
                                    WHERE licence_number = @license_number";
            command.Parameters.AddWithValue("@license_number", license_number);
            var driverIdObj = await command.ExecuteScalarAsync();
            if (driverIdObj == null)
            {
                throw new Exception("Driver not found");
            }

            var driverId = (int)driverIdObj;
            command.Parameters.Clear();

            command.CommandText = "INSERT INTO Delivery VALUES(@deliveryId, @customerId, @driverId, @date)";
            command.Parameters.AddWithValue("@deliveryId", delivery.DeliveryId);
            command.Parameters.AddWithValue("@customerId", delivery.CustomerId);
            command.Parameters.AddWithValue("@driverId", driverId);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();

            var productsIds = new List<int>();
            foreach (var product in delivery.Products)
            {
                command.CommandText = "SELECT product_id FROM Product WHERE name = @name";
                command.Parameters.AddWithValue("@name", product.Name);
                var productIdObj = await command.ExecuteScalarAsync();
                if (productIdObj == null)
                {
                    throw new Exception("Product not found");
                }

                var productId = (int)productIdObj;
                productsIds.Add(productId);
                command.Parameters.Clear();
            }


            foreach (var productId in productsIds)
            {
                command.CommandText = "INSERT INTO Product_Delivery VALUES(@productId, @deliveryId, @amount)";
                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@deliveryId", delivery.DeliveryId);
                command.Parameters.AddWithValue("@amount", 0);
                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return false;
        }
    }
}

