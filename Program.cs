using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // <- Necesario para SQL Server
using Microsoft.IdentityModel.Tokens;
using System.Text;
using tp5_Trani_Joaco_Alex.Data; // <- Necesario para leer tu ApplicationDbContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ---> ESTO FALTABA: CONECTAR EL DBCONTEXT A TU CADENA DE CONEXIÓN <---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. CONFIGURACIÓN DE CORS PERMISIVO (Módulo B)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaPermisiva", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 2. CONFIGURACIÓN DE AUTENTICACIÓN JWT (Módulo B)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpsRedirection();

// Habilitar la carpeta wwwroot para servir imágenes
app.UseStaticFiles();


// 3. APLICAR CORS
app.UseCors("PoliticaPermisiva");

// 4. APLICAR AUTENTICACIÓN Y AUTORIZACIÓN
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();