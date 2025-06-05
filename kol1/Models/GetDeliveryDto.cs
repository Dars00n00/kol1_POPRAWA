namespace kol1.Models;


public class GetDeliveryDto
{
    public DateTime Date { get; set; }
    
    public GetCustomerDto Customer  { get; set; }
    
    public GetDriverDto Driver { get; set; }
    
    public List<GetProductDto> Products { get; set; }
}

public class GetCustomerDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public DateTime DateOfBirth { get; set; }
}

public class GetDriverDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string LicenseNumber { get; set; }
}

public class GetProductDto
{
    public string Name { get; set; }
    
    public decimal Price { get; set; }
    
    public int Amount { get; set; }
}