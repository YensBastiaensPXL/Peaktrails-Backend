using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PeaktrailsApp.Data;
using PeaktrailsBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add DbContext
builder.Services.AddDbContext<PeaktrailsAppContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PeaktrailsAppContext"));
});

// Add repository
builder.Services.AddScoped<PhotosRepository>();
builder.Services.AddScoped<UsersRepository>();

// Set upload size limit to 50MB
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50 MB limit
});

// Add controllers to the app
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Peaktrails API", Version = "v1" });

    // Enable file upload in Swagger
    c.OperationFilter<FileUploadOperation>();
});

// Build the app
var app = builder.Build();

// Serve static files
app.UseStaticFiles();

// Use the CORS policy
app.UseCors("AllowAll");

// Swagger UI setup for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Setup middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Run the app
app.Run();
