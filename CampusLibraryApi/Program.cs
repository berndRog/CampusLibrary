using CampusLibraryApi._4_Infrastructure;
using CampusLibraryApi.Configure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddReadersModule();

builder.Services.AddInfrastructureModule(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

// Exposes the top-level Program type to WebApplicationFactory in API tests.
public partial class Program { }
