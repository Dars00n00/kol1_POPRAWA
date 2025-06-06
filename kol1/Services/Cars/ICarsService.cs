namespace kol1.Services.Cars;


public interface ICarsService
{
    
    Task<bool> DoesCarExistAsync(int carId);

    Task<int> CalculateCarRentalPriceAsync(int carId, DateTime dateFrom, DateTime dateTo);

}