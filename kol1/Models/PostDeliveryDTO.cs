namespace kol1.Models;

public class PostDeliveryDTO
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber { get; set; }
    public List<PostProductDTO> Products { get; set; }
}

public class PostProductDTO
{
    public string Name { get; set; }
    public int Amount { get; set; }
}