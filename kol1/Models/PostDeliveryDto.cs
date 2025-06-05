namespace kol1.Models;


public class PostDeliveryDto
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber { get; set; }
    public List<PostProductDto> Products { get; set; }
}

public class PostProductDto
{
    public string Name { get; set; }
    public int Amount { get; set; }
}