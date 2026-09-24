using IoTGateway.API.Data;
using IoTGateway.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTGateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DevicesController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        return await _context.Devices
            .AsNoTracking()
            .ToListAsync();
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Device>> GetDevice(Guid id)
    {
        var device = await _context.Devices
            .Include(d => d.Telemetries)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
            return NotFound(new { message = $"Dispositivo con ID {id} non trovato." });

        return device;
    }

    [HttpPost]
    public async Task<ActionResult<Device>> RegisterDevice([FromBody] Device device)
    {
        if (string.IsNullOrWhiteSpace(device.SerialNumber))
            return BadRequest(new { message = "Il campo SerialNumber è obbligatorio." });

        var exists = await _context.Devices.AnyAsync(d => d.SerialNumber == device.SerialNumber);
        if (exists)
            return Conflict(new { message = $"Un dispositivo con serial number '{device.SerialNumber}' esiste già." });

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDevice(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return NotFound();

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}