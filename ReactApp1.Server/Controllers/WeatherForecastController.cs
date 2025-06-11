using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            ApplicationDbContext context,
            ILogger<WeatherForecastController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/weatherforecast
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWeatherForecasts()
        {
            try
            {
                return await _context.WeatherForecasts
                    .OrderBy(w => w.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather forecasts");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/weatherforecast/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WeatherForecast>> GetWeatherForecast(int id)
        {
            var forecast = await _context.WeatherForecasts.FindAsync(id);

            if (forecast == null)
            {
                return NotFound();
            }

            return forecast;
        }

        // POST: api/weatherforecast
        [HttpPost]
        public async Task<ActionResult<WeatherForecast>> PostWeatherForecast(WeatherForecast forecast)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Set ID to 0 to let database auto-generate
                forecast.Id = 0;

                _context.WeatherForecasts.Add(forecast);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetWeatherForecast),
                    new { id = forecast.Id }, forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating weather forecast");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/weatherforecast/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWeatherForecast(int id, WeatherForecast forecast)
        {
            if (id != forecast.Id)
            {
                return BadRequest();
            }

            try
            {
                _context.Entry(forecast).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!WeatherForecastExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating weather forecast");
                    return StatusCode(500, "Internal server error");
                }
            }

            return NoContent();
        }

        // DELETE: api/weatherforecast/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeatherForecast(int id)
        {
            var forecast = await _context.WeatherForecasts.FindAsync(id);
            if (forecast == null)
            {
                return NotFound();
            }

            try
            {
                _context.WeatherForecasts.Remove(forecast);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting weather forecast");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool WeatherForecastExists(int id)
        {
            return _context.WeatherForecasts.Any(e => e.Id == id);
        }
    }
}