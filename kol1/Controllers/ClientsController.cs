using kol1.Exceptions;
using kol1.Models;
using kol1.Services;
using Microsoft.AspNetCore.Mvc;


namespace kol1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientAsync([FromRoute] int id)
    {
        try
        {
            var res = await _clientsService.GetClientAsync(id);
            Console.WriteLine(res.FirstName + " " + res.LastName + " " + res.Address);
            return Ok(res);
        }
        catch (ClientNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        /*catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }*/
    }


    [HttpPost]
    public async Task<IActionResult> PostNewClient([FromBody] PostClientDto client)
    {
        try
        {
            if (!await _clientsService.AddNewClientWithRentalAsync(client))
            {
                return BadRequest("could not add client");
            }

            return StatusCode(201);
        }
        catch (CarNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
}