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
    public async Task<IActionResult> PostDelivery([FromBody] PostDeliveryDTO delivery)
    {
        if (await _deliveriesService.DoesDeliveryExistAsync(delivery.DeliveryId))
        {
            return BadRequest();
        }

        if (await _deliveriesService.AddNewDelivery(delivery))
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
    
}