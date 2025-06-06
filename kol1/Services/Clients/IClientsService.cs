using kol1.Models;


namespace kol1.Services;


public interface IClientsService
{
    
    Task<bool> DoesClientExistAsync(int clientId);

    Task<bool> DoesClientHaveAnyRentalsAsync(int clientId);
    
    Task<GetClientDto> GetClientAsync(int clientId);


    Task<bool> AddNewClientWithRentalAsync(PostClientDto client);
}