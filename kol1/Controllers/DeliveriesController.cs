using kol1.Exceptions;
using kol1.Models;
using kol1.Services;
using Microsoft.AspNetCore.Mvc;


namespace kol1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DeliveriesController : ControllerBase
{
    
    private readonly IDeliveriesService _deliveriesService;

    public DeliveriesController(IDeliveriesService deliveriesService)
    {
        _deliveriesService = deliveriesService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDelivery([FromRoute] int id)
    {
        if (!await _deliveriesService.DoesDeliveryExistAsync(id))
        {
            return NotFound();
        }

        var res = await _deliveriesService.GetDeliveryAsync(id);
        return Ok(res);
    }


    [HttpPost]
    public async Task<IActionResult> PostDelivery([FromBody] PostDeliveryDto delivery)
    {
        try
        {
            if (await _deliveriesService.AddNewDeliveryAsync(delivery))
            {
                return Ok();
            }
        }
        catch (DeliveryExistsException e)
        {
            return BadRequest(e.Message);
        }
        catch (CustomerNotFoundException e)
        {
            return BadRequest(e.Message);

        }
        catch (DriverNotFoundException e)
        {
            return BadRequest(e.Message);
        }
        catch (ProductNotFoundException e)
        {
            return BadRequest(e.Message);

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        return BadRequest();
    }
    
}