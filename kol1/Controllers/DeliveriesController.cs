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
        try
        {
            if (!await _deliveriesService.DoesDeliveryExistAsync(id))
            {
                return NotFound();
            }

            var res = await _deliveriesService.GetDeliveryAsync(id);
            return Ok(res);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
    }


    [HttpPost]
    public async Task<IActionResult> PostDelivery([FromBody] PostDeliveryDTO delivery)
    {
        try
        {
            if (await _deliveriesService.DoesDeliveryExistAsync(delivery.DeliveryId))
            {
                return BadRequest();
            }

            if (!await _deliveriesService.AddNewDelivery(delivery))
            {
                return BadRequest();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }


        return Ok();
    }
    
}