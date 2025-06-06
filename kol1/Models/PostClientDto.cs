namespace kol1.Models;

public class PostClientDto
{
    public PostClientInnerObjectDto client { get; set; }
    
    public int CarId { get; set; }
    
    public DateTime DateFrom { get; set; }
    
    public DateTime DateTo { get; set; }
}


public class PostClientInnerObjectDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Address { get; set; }
}