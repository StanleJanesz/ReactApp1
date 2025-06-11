using Microsoft.EntityFrameworkCore;
using ReactApp1.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not found");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));


var app = builder.Build();

// Initialize database without migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        var dbConnection = db.Database.GetDbConnection();
        dbConnection.Open();

        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = "SHOW TABLES LIKE 'WeatherForecasts';";
        var result = cmd.ExecuteScalar();

        if (result == null)
        {
            Console.WriteLine("WeatherForecasts table does not exist. Creating...");

            // Create the table using EF Core
            db.Database.ExecuteSqlRaw(@"
            CREATE TABLE WeatherForecasts (
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                Date DATE NOT NULL,
                TemperatureC INT NOT NULL,
                Summary VARCHAR(100)
            );
        ");

            // Optionally seed initial data
            db.WeatherForecasts.AddRange(
                new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(1),
                    TemperatureC = 20,
                    Summary = "Mild"
                },
                new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(2),
                    TemperatureC = 25,
                    Summary = "Warm"
                }
            );
            db.SaveChanges();

            Console.WriteLine("WeatherForecasts table created and data seeded.");
        }
        else
        {
            Console.WriteLine("WeatherForecasts table already exists.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }

    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }
}

// Configure middleware pipeline
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the WeatherForecast table
        modelBuilder.Entity<WeatherForecast>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date)
                .HasColumnType("date");
            entity.Property(e => e.TemperatureC)
                .IsRequired();
            entity.Property(e => e.Summary)
                .HasMaxLength(100);
        });
    }
}