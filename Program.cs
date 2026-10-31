using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using tp5_Trani_Joaco_Alex.Data;
using Serilog;

// 1. Inicialización temprana del registrador de arranque (Bootstrap Logger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicación Web API TP5...");
    var builder = WebApplication.CreateBuilder(args);

    // COMENTADO: Kestrel choca con el In-Process de IIS en MonsterASP y bota la conexión.
    // (El límite de subida de 50MB lo configuraremos luego en el web.config si es necesario)
    // builder.WebHost.ConfigureKestrel(options =>
    // {
    //     options.Limits.MaxRequestBodySize = 52428800;
    // });

    QuestPDF.Settings.License = LicenseType.Community;

    // 2. Conectar Serilog al Host leyendo appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddScoped<tp5_Trani_Joaco_Alex.Services.IEmailService, tp5_Trani_Joaco_Alex.Services.EmailService>();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("PoliticaPermisiva", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            Console.WriteLine($"\n---> 1. TOKEN RECIBIDO POR LA API: {(string.IsNullOrEmpty(context.Token) ? "NINGUNO" : "SÍ")}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"\n---> 2. ERROR AL VALIDAR EL TOKEN: {context.Exception.Message}\n");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"\n---> 3. RECHAZADO EN LA PUERTA (CHALLENGE): {context.Error} | {context.ErrorDescription}\n");
                            return Task.CompletedTask;
                        }
                    };
                });

    builder.Services.AddAuthorization();
    builder.Services.AddEndpointsApiExplorer();

    // 3. Configuración de Swagger con soporte para Token JWT (Botón Authorize)
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trabajo Práctico N° 5 - Servicios API REST", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
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
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // 4. Auditoría HTTP de Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    // Habilitamos Swagger tanto en local como en producción (MonsterASP)
    app.UseSwagger();
    app.UseSwaggerUI();

    // COMENTADO: MonsterASP ya maneja el HTTPS. Forzarlo aquí causa un ERR_CONNECTION_RESET.
    // app.UseHttpsRedirection(); 

    app.UseStaticFiles();

    app.UseCors("PoliticaPermisiva");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            Log.Information("Migraciones aplicadas exitosamente en la base de datos.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error crítico al intentar aplicar las migraciones ");
        }
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Fallo crítico no controlado durante el inicio de la aplicación.");
}
finally
{
    Log.CloseAndFlush();
}