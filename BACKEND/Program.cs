using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ProjectName.Data;
using ProjectName.Repositories;
using ProjectName.Services;
using ProjectName.Llm;
using ProjectName.Models;
using Npgsql; 

var builder = WebApplication.CreateBuilder(args);
var assemblyName = typeof(Program).Assembly.GetName().Name ?? "ProjectName";

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("A 'PostgresConnection' kapcsolati sztring nem található vagy üres. Kérlek, ellenőrizd az appsettings.json fájlt.");
}

// 0. JSONB Szerializáció Engedélyezése (Npgsql 7.0+ ajánlott módszere)
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson(); 
var dataSource = dataSourceBuilder.Build(); 

// 1. PostgreSQL DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // A DbContext most már a megfelelően beállított data source objektumot használja.
    options.UseNpgsql(dataSource);
});

// 1.5. Gemini konfiguráció regisztrálása
builder.Services.Configure<GeminiConfig>(
    builder.Configuration.GetSection(GeminiConfig.Gemini));

// 2. HTTP Kliens a Gemini API-hoz
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>();

// 3. Repository-k (Adatbázis elérés)
builder.Services.AddScoped<IZhRepository, ZhRepository>();
builder.Services.AddScoped<IUploadedSolutionsRepository, UploadedSolutionsRepository>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();

// 4. Service-ek (Üzleti logika)
builder.Services.AddScoped<SolutionService>();
builder.Services.AddScoped<TestGeneratorService>();
builder.Services.AddScoped<CorrectionService>();

// 5. Controller-ek, Swagger és JSON beállítások
builder.Services.AddControllers()
    .AddNewtonsoftJson(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = assemblyName, Version = "v1" });
});


// --- Alkalmazás Építése ---
var app = builder.Build();

// --- HTTP Request Pipeline konfigurációja ---
if (app.Environment.IsDevelopment())
{
    // Opcionális: Migration alkalmazása startupkor
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();