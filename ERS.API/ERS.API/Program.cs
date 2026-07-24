using ERS.Core.Data;
using ERS.Core.Interfaces;
using ERS.Core.Algorithms;
using ERS.Core.Service;
using ERS.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string udemyCsvPath = builder.Configuration.GetSection("DataPipeline")["UdemyCsvFilePath"];
if (string.IsNullOrEmpty(udemyCsvPath))
{
    throw new Exception("Шлях до CSV файлу не знайдено в appsettings.json!");
}

builder.Services.AddScoped<ICourseRepository>(provider => new MySqlRepository(connectionString));
builder.Services.AddScoped<IUserRepository>(provider => new MySqlRepository(connectionString));
builder.Services.AddScoped<IInteractionRepository>(provider => new MySqlRepository(connectionString));

builder.Services.AddSingleton<TF_IDF>();

builder.Services.AddScoped<ICourseExtractor>(provider =>
    new ERS.Core.DataPipeline.Udemy.UdemyCsvExtractor(udemyCsvPath));

builder.Services.AddScoped<ERS.Core.DataPipeline.DataPipelineManager>();

builder.Services.AddScoped<IRecommendService, RecommendService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<CacheWarmupService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()     
              .AllowAnyMethod()      
              .AllowAnyHeader();    
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();