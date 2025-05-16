using kol1.Models;

namespace kol1.Services;

public interface IDeliveriesService
{
    Task<bool> DoesDeliveryExistAsync(int id);
    Task<GetDeliveryDTO> GetDeliveryAsync(int id);

    Task<bool> AddNewDelivery(PostDeliveryDTO delivery);
}