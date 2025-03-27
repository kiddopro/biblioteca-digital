using BibliotecaDigital.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// Este es el archivo principal que inicia toda la aplicacion
var builder = WebApplication.CreateBuilder(args);

// Configuramos Serilog para registrar eventos y errores de la aplicacion
// Esto nos ayuda a saber que esta pasando en la aplicacion cuando esta en funcionamiento
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Agregamos el servicio de controladores que maneja las peticiones HTTP
builder.Services.AddControllers();

// Configuramos la conexion a la base de datos PostgreSQL local, para hacerlo con supabase varia un poco
// Tambien permite que la aplicacion pueda guardar y recuperar datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuramos la autenticacion con JWT
// Para que los usuarios puedan loguearse y realizar acciones que se encuentran protegidas
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configuramos validacion de los tokens de autenticacion
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Configuramos Swagger -> que genera documentacion automatica de la API
// nos va a facilitar la comprension y el testeo de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Biblioteca Digital API", Version = "v1" });

    // Configuramos Swagger para que incluya la autenticacion JWT (en ingles por costumbre 😂)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Construimos la aplicacion con todas las configuraciones anteriores
var app = builder.Build();

// Probamos la conexion a la base de datos al iniciar la aplicacion
// Esto nos permite detectar problemas de conexion temprano
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        Log.Information("Conexion a la base de datos PostgreSQL local exitosa");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al conectar con la base de datos PostgreSQL local");
    }
}

// Configuramos el pipeline de procesamiento de peticiones HTTP
// Tambien determinamos como se manejan las peticiones que llegan a la aplicacion

// Si estamos en desarrollo habilitamos Swagger para documentacion
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirigimos HTTP a HTTPS para mayor seguridad
app.UseHttpsRedirection();

// Habilitamos la autenticacion y autorizacion
app.UseAuthentication();
app.UseAuthorization();

// Configuramos las rutas a los controladores
app.MapControllers();

// Iniciamos la aplicacion
app.Run();