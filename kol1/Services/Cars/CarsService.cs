using Microsoft.Data.SqlClient;


namespace kol1.Services.Cars;


public class CarsService : ICarsService
{
    private readonly string _connectionString;

    public CarsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("main");
    }

    public async Task<bool> DoesCarExistAsync(int carId)
    {
        string commandStr = @"Select count(*) 
                              FROM k1pr_cars 
                              WHERE id = @carId";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);

        command.Parameters.AddWithValue("@carId", carId);

        await connection.OpenAsync();

        var amountObj = await command.ExecuteScalarAsync();
        var amountInt = Convert.ToInt32(amountObj);

        return amountInt == 1;
    }


    public async Task<int> CalculateCarRentalPriceAsync(int carId, DateTime dateFrom, DateTime dateTo)
    {
        string commandStr = @"Select priceperday 
                              FROM k1pr_cars 
                              WHERE id = @carId";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(commandStr, connection);
        
        command.Parameters.AddWithValue("@carId", carId);
        
        await connection.OpenAsync();
        
        var numOfDays = dateTo.Subtract(dateFrom).Days;
        var pricePerDayObj = await command.ExecuteScalarAsync();
        var pricePerDayInt = Convert.ToInt32(pricePerDayObj);
        
        return pricePerDayInt * numOfDays;
    }
}
