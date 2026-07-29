using System.Text;
using System.Threading.RateLimiting;
using Backend.Data;
using Backend.Services.Business;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configurar WebRootPath tempranamente si no se ha detectado
var webRootPath = builder.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRootPath))
{
    webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    builder.Environment.WebRootPath = webRootPath;
}
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
var avataresPath = Path.Combine(webRootPath, "uploads", "avatares");
if (!Directory.Exists(avataresPath))
{
    Directory.CreateDirectory(avataresPath);
}
var progresoPath = Path.Combine(webRootPath, "uploads", "progreso");
if (!Directory.Exists(progresoPath))
{
    Directory.CreateDirectory(progresoPath);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuración de CORS para Frontend (Vue)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración de Rate Limiting (Protección Anti Fuerza Bruta)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("LoginPolicy", policyOptions =>
    {
        policyOptions.PermitLimit = 5;
        policyOptions.Window = TimeSpan.FromMinutes(1);
        policyOptions.QueueLimit = 0;
    });
});

// Configuración de MySQL con EF Core (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configuración de Autenticación JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secret = jwtSettings.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT Secret is not configured.");
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Registrar servicios de negocio
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IActividadService, ActividadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<ICuotaService, CuotaService>();
builder.Services.AddScoped<ICoachService, CoachService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ISocioProgresoService, SocioProgresoService>();

var app = builder.Build();

// Sembrado de datos (DataSeeder)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await DataSeeder.SeedDataAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Un error ocurrió durante el sembrado de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Habilitar archivos estáticos para wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath),
    RequestPath = ""
});

app.UseCors("AllowFrontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
