using TodosApi.Data;
using TodosApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Todos API", 
        Version = "v1",
        Description = "API dla aplikacji Lista zadań - zarządzanie zadaniami i historią wykonań",
        Contact = new()
        {
            Name = "Todos App",
            Email = "support@todos.app"
        }
    });
    
    // Dodanie komentarzy XML jeśli są dostępne
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", // React dev server
                "http://localhost:5173", // Vite dev server
                "https://localhost:7000", // Blazor HTTPS
                "http://localhost:5000"   // Blazor HTTP
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register services
builder.Services.AddScoped<ICsvDataService, CsvDataService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

// Register background service
builder.Services.AddHostedService<DailyResetService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todos API V1");
        c.RoutePrefix = string.Empty; // Swagger UI na głównej stronie (/)
        c.DocumentTitle = "Todos API - Swagger UI";
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
    });
}
else
{
    // W produkcji też można mieć Swagger jeśli potrzebne
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todos API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRouting();
app.MapControllers();

app.Run();
