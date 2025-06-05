using kol1.Models;


namespace kol1.Services;


public interface IDeliveriesService
{
    Task<bool> DoesDeliveryExistAsync(int id);
    Task<bool> DoesClientExistAsync(int id);
    Task<bool> DoesDriverExistAsync(string licenceNumber);
    Task<bool> DoesProductExistAsync(string productName);
   
    
    Task<GetDeliveryDto> GetDeliveryAsync(int id);
    
    Task<bool> AddNewDeliveryAsync(PostDeliveryDto delivery);
}