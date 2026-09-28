using System.Text;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. BASE DE DATOS
// ============================================================
builder.Services.AddDbContext<InmobiliariaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("InmobiliariaConnection")));

// ============================================================
// 2. CONTROLADORES + JSON
// ============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ============================================================
// 3. CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7207", "http://localhost:5285")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================================
// 4. JWT
// ============================================================
var jwtKey = builder.Configuration["Jwt:Key"];
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// 5. SWAGGER
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InmobiliariaAPI",
        Version = "v1",
        Description = "API REST para Tapia's Solution"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
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

// ============================================================
// 6. CONFIGURACIÓN DE SUPABASE STORAGE
// ============================================================
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:ApiKey"];

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    try
    {
        var supabaseOptions = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };

        var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, supabaseOptions);
        await supabaseClient.InitializeAsync();

        builder.Services.AddSingleton<Supabase.Client>(supabaseClient);
        builder.Services.AddScoped<StorageService>();

        Console.WriteLine("✅ Supabase Storage configurado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error al configurar Supabase: {ex.Message}");
    }
}
else
{
    Console.WriteLine("⚠️ Supabase NO configurado (falta Url o ApiKey)");
}

// ============================================================
// PIPELINE
// ============================================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowMVC");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();