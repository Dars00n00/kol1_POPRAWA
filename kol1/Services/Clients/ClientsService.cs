using kol1.Exceptions;
using kol1.Models;
using kol1.Services.Cars;
using Microsoft.Data.SqlClient;


namespace kol1.Services.Clients;


public class ClientsService : IClientsService
{
    private readonly string _connectionString;
    private readonly ICarsService _carsService;

    public ClientsService(IConfiguration configuration, ICarsService carsService)
    {
        _connectionString = configuration.GetConnectionString("main");
        _carsService = carsService;
    }


    //=========================== metody pomocnicze do walidacji danych ===========================
    public async Task<bool> DoesClientExistAsync(int clientId)
    {
        string commandStr = @"Select count(*) 
                              FROM k1pr_clients 
                              WHERE id = @id";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", clientId);

        await connection.OpenAsync();

        var amountObj = await command.ExecuteScalarAsync();
        var amountInt = Convert.ToInt32(amountObj);

        return amountInt == 1;
    }

    public async Task<bool> DoesClientHaveAnyRentalsAsync(int clientId)
    {
        string commandStr = @"Select count(*)
                              FROM k1pr_clients c
                              INNER JOIN k1pr_car_rentals cr ON cr.clientid = c.id
                              INNER JOIN k1pr_cars ON k1pr_cars.id = cr.carid
                              INNER JOIN k1pr_models m ON m.id = k1pr_cars.modelid
                              INNER JOIN k1pr_colors col ON col.id = k1pr_cars.colorid
                              WHERE c.id = @id;";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@id", clientId);

        await connection.OpenAsync();

        var numberOfRentalsObj = await command.ExecuteScalarAsync();
        var numberOfRentals = Convert.ToInt32(numberOfRentalsObj);

        return numberOfRentals > 0;
    }


    // =========================== metody użyte w końcówkach ===========================
    public async Task<GetClientDto> GetClientAsync(int id)
    {
        if (!await DoesClientExistAsync(id))
        {
            throw new ClientNotFoundException($"klient o id={id} nie istnieje");
        }

        string commandStr = "";
        if (!await DoesClientHaveAnyRentalsAsync(id))
        {
            commandStr = @"Select c.id, c.firstname, c.lastname, c.address
                           FROM k1pr_clients c
                           WHERE c.id = @id;";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(commandStr, connection);

            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();

            var reader = await command.ExecuteReaderAsync();

            GetClientDto client = null;
            
            while (await reader.ReadAsync())
            {
                client ??= new GetClientDto()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                    LastName = reader.GetString(reader.GetOrdinal("lastname")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    Rentals = new List<GetRentalDto>()
                };
            }
            return client;
        }
        else
        {
            commandStr = @"Select c.id, c.firstname, c.lastname, c.address,
                              cr.id AS rentalid, k1pr_cars.vin, col.name as color, m.name as model, 
                              cr.datefrom, cr.dateto, cr.totalprice
                              FROM k1pr_clients c
                              INNER JOIN k1pr_car_rentals cr ON cr.clientid = c.id
                              INNER JOIN k1pr_cars ON k1pr_cars.id = cr.carid
                              INNER JOIN k1pr_models m ON m.id = k1pr_cars.modelid
                              INNER JOIN k1pr_colors col ON col.id = k1pr_cars.colorid
                              WHERE c.id = @id;";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(commandStr, connection);

            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();

            var reader = await command.ExecuteReaderAsync();

            GetClientDto client = null;

            var rentalsDictionary = new Dictionary<int, GetRentalDto>(); // <id_rental, rentalDto>

            while (await reader.ReadAsync())
            {
                client ??= new GetClientDto()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                    LastName = reader.GetString(reader.GetOrdinal("lastname")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    Rentals = new List<GetRentalDto>()
                };

                var rentalId = reader.GetInt32(reader.GetOrdinal("rentalid"));

                if (!rentalsDictionary.ContainsKey(rentalId))
                {
                    var rental = new GetRentalDto()
                    {
                        Vin = reader.GetString(reader.GetOrdinal("vin")),
                        Color = reader.GetString(reader.GetOrdinal("color")),
                        Model = reader.GetString(reader.GetOrdinal("model")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("datefrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("dateto")),
                        TotalPrice = reader.GetInt32(reader.GetOrdinal("totalprice")),
                    };

                    rentalsDictionary.Add(rentalId, rental);
                    client.Rentals.Add(rental);
                }
            }

            return client;
        }
        
    }

    public async Task<bool> AddNewClientWithRentalAsync(PostClientDto clientDto)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;
        
        try
        {
            command.CommandText = @"INSERT INTO k1pr_clients 
                                    (firstname, lastname, address) 
                                    VALUES 
                                    (@firstname, @lastname, @address);
                                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
            command.Parameters.AddWithValue("@firstname", clientDto.client.FirstName);
            command.Parameters.AddWithValue("@lastname", clientDto.client.LastName);
            command.Parameters.AddWithValue("@address", clientDto.client.Address);
            var clientIdObj = await command.ExecuteScalarAsync();
            var clientId = Convert.ToInt32(clientIdObj);

            command.Parameters.Clear();

            if (!await _carsService.DoesCarExistAsync(clientDto.CarId))
            {
                throw new CarNotFoundException($"auto id id={clientDto.CarId} nie istnieje");
            }

            var carRentalTotalPrice
                = await _carsService.CalculateCarRentalPriceAsync(clientDto.CarId, clientDto.DateFrom,
                    clientDto.DateTo);

            command.CommandText = @"INSERT INTO k1pr_car_rentals 
                                    (clientid, carid, datefrom, dateto, totalprice)
                                    VALUES 
                                    (@clientid, @carid, @datefrom, @dateto, @totalprice);";
            command.Parameters.AddWithValue("@clientid", clientId);
            command.Parameters.AddWithValue("@carid", clientDto.CarId);
            command.Parameters.AddWithValue("@datefrom", clientDto.DateFrom);
            command.Parameters.AddWithValue("@dateto", clientDto.DateTo);
            command.Parameters.AddWithValue("@totalprice", carRentalTotalPrice);
            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
}
