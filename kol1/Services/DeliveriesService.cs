using kol1.Exceptions;
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

    //metody pomocnicze do walidacji danych
    public async Task<bool> DoesDeliveryExistAsync(int id)
    {
        string commandStr = @"Select count(*) 
                              FROM k1r_Delivery 
                              WHERE delivery_id = @id";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var amountObj = await command.ExecuteScalarAsync();
        var amountInt = (int)amountObj;
        
        return amountInt == 1;
    }

    public async Task<bool> DoesClientExistAsync(int id)
    {
        string commandStr = @"Select count(*) 
                              FROM k1r_Customer 
                              WHERE customer_id = @id";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var amountObj = await command.ExecuteScalarAsync();
        var amountInt = (int)amountObj;
        
        return amountInt == 1;
    }

    public async Task<bool> DoesDriverExistAsync(string licenceNumber)
    {
        string commandStr = @"Select count(*) 
                              FROM k1r_Driver
                              WHERE licence_number = @licenceNumber";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@licenceNumber", licenceNumber);

        await connection.OpenAsync();

        var amountObj = await command.ExecuteScalarAsync();
        var amountInt = (int)amountObj;
        
        return amountInt == 1;
    }

    public async Task<bool> DoesProductExistAsync(string productName)
    {
        string commandStr = @"Select count(*) 
                              FROM k1r_Product
                              WHERE name = @productName;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@productName", productName);

        await connection.OpenAsync();

        var amountObj = await command.ExecuteScalarAsync();
        var amountInt = (int)amountObj;
        
        return amountInt == 1;
    }
    

    //metody do rozwiązania kolokwium
    public async Task<GetDeliveryDto> GetDeliveryAsync(int id)
    {
        string commandStr = @"Select d.date, 
                              c.first_name, c.last_name, c.date_of_birth,
                              dr.first_name AS dr_first_name, dr.last_name dr_last_name, dr.licence_number,
                              p.name AS product_name, p.price, pd.amount
                              FROM k1r_Delivery d
                              INNER JOIN k1r_Customer c ON d.customer_id = c.customer_id
                              INNER JOIN k1r_Product_Delivery pd ON d.delivery_id = pd.delivery_id
                              INNER JOIN k1r_Driver dr ON d.driver_id = dr.driver_id
                              INNER JOIN k1r_Product p ON pd.product_id = p.product_id
                              WHERE d.delivery_id = @id;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        GetDeliveryDto delivery = null;
        
        var productsDictionary = new Dictionary<string, GetProductDto>();

        while (await reader.ReadAsync())
        {
            delivery ??= new GetDeliveryDto()
            {
                Date = reader.GetDateTime(reader.GetOrdinal("date")),
                Customer = null,
                Driver = null,
                Products = new List<GetProductDto>()
            };

            delivery.Customer ??= new GetCustomerDto
            {
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
            };

            delivery.Driver ??= new GetDriverDto
            {
                FirstName = reader.GetString(reader.GetOrdinal("dr_first_name")),
                LastName = reader.GetString(reader.GetOrdinal("dr_last_name")),
                LicenseNumber = reader.GetString(reader.GetOrdinal("licence_number")),
            };

            var productName = reader.GetString(reader.GetOrdinal("product_name"));

            if (!productsDictionary.TryGetValue(productName, out var product))
            {
                product = new GetProductDto
                {
                    Name = reader.GetString(reader.GetOrdinal("product_name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Amount = reader.GetInt32(reader.GetOrdinal("amount")),
                };
                productsDictionary[productName] = product;
                delivery.Products.Add(product);
            }
        }
        
        return delivery;
    }

    public async Task<bool> AddNewDeliveryAsync(PostDeliveryDto delivery)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();
    
        command.Connection = connection;
        await connection.OpenAsync();
    
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            if (await DoesDeliveryExistAsync(delivery.DeliveryId))
            {
                throw new DeliveryExistsException($"delivery with id={delivery.DeliveryId} already exists");
            }

            if (!await DoesClientExistAsync(delivery.CustomerId))
            {
                throw new CustomerNotFoundException($"customer with id={delivery.CustomerId} does not exist");
            }

            if (!await DoesDriverExistAsync(delivery.LicenceNumber))
            {
                throw new DriverNotFoundException($"driver with licence={delivery.LicenceNumber} does not exist");
            }

            foreach (var product in delivery.Products)
            {
                if (!await DoesProductExistAsync(product.Name))
                {
                    throw new ProductNotFoundException($"product with name={product.Name} does not exist");
                }
            }

            //pobranie id kierowcy
            command.CommandText = @"SELECT driver_id 
                                    FROM k1r_Driver 
                                    WHERE licence_number = @licenseNumber";
            command.Parameters.AddWithValue("@licenseNumber", delivery.LicenceNumber);
            var driverIdObj = await command.ExecuteScalarAsync();
            var driverId = (int)driverIdObj;

            command.Parameters.Clear();

            //dodanie dostawy do delivery
            command.CommandText = @"INSERT INTO k1r_Delivery 
                                    (delivery_id, customer_id, driver_id, date) 
                                    VALUES
                                    (@deliveryId, @customerId, @driverId, @date);";
            command.Parameters.AddWithValue("@deliveryId", delivery.DeliveryId);
            command.Parameters.AddWithValue("@customerId", delivery.CustomerId);
            command.Parameters.AddWithValue("@driverId", driverId);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();


            //pobranie id produktów z dostawy
            var productsDictionary = new Dictionary<int, PostProductDto>(); // <id_product, PostProductDto>
            foreach (var product in delivery.Products)
            {
                command.CommandText = @"SELECT product_id 
                                        FROM k1r_Product 
                                        WHERE name = @name";
                command.Parameters.AddWithValue("@name", product.Name);
                var productIdObj = await command.ExecuteScalarAsync();
                var productId = (int)productIdObj;
                productsDictionary[productId] = product;

                command.Parameters.Clear();
            }

            //dodanie produktów do tabeli product_delivery
            foreach (var product in productsDictionary)
            {
                command.CommandText = @"INSERT INTO k1r_Product_Delivery
                                        (product_id, delivery_id, amount)
                                        VALUES
                                        (@productId, @deliveryId, @amount);";
                command.Parameters.AddWithValue("@productId", product.Key);
                command.Parameters.AddWithValue("@deliveryId", delivery.DeliveryId);
                command.Parameters.AddWithValue("@amount", product.Value.Amount);
                await command.ExecuteNonQueryAsync();

                command.Parameters.Clear();
            }

            await transaction.CommitAsync();

            return true;
        }
        catch (DeliveryExistsException e)
        {
            await transaction.RollbackAsync();
            throw e;
        }
        catch (CustomerNotFoundException e)
        {
            await transaction.RollbackAsync();
            throw e;
        }
        catch (DriverNotFoundException e)
        {
            await transaction.RollbackAsync();
            throw e;
        }
        catch (ProductNotFoundException e)
        {
            await transaction.RollbackAsync();
            throw e;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return false;
        }
        
    }
}

