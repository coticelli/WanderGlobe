// Modifica per Program.cs per utilizzare SQLite
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // For IEmailSender in this namespace
using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;
using WanderGlobe.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging; // Added for logging in migration section

var builder = WebApplication.CreateBuilder(args);

// Cambia da SQL Server a SQLite
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? throw new InvalidOperationException("Connection string 'SqliteConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Configurazione Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Set to true for production if using email confirmation
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddRazorPages();

// Registra i servizi
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IDreamService, DreamService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ITravelJournalService, TravelJournalService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IUserProgressService, UserProgressService>();

// --- ADD THIS LINE TO REGISTER IVISITEDCITYSERVICE ---
builder.Services.AddScoped<IVisitedCityService, VisitedCityService>();
// ------------------------------------------------------

// Using the fully qualified name for IEmailSender if there's an ambiguity
// or ensure your 'using' statement points to the correct one.
// If EmailSender is in WanderGlobe.Services:
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
// Or if your EmailSender implements a custom IEmailSender in WanderGlobe.Services:
// builder.Services.AddTransient<WanderGlobe.Services.IEmailSender, WanderGlobe.Services.EmailSender>();


// Aggiungi HttpClient per API esterne (WeatherService will get it via DI if needed)
builder.Services.AddHttpClient(); // Generally good to have if services need it.
                                  // WeatherService can also have HttpClient injected directly.
                                  // If WeatherService is the only one, you can register it like:
                                  // builder.Services.AddHttpClient<IWeatherService, WeatherService>();

// Configure FormOptions for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue; // Or a specific large value
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB, adjust as needed
    options.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseMigrationsEndPoint(); // If you were using SQL Server Identity default UI
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
// If you have API controllers:
// app.MapControllers();

// Apply migrations at startup (optional, but good for development/simple deployments)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        // Consider re-throwing or exiting if DB is critical for startup
    }
}

app.Run();