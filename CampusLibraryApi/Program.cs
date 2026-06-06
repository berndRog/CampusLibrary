using CampusLibrary.Api._2_Shared;
using CampusLibrary.Api._4_Infrastructure.Persistence;
using CampusLibrary.Api.Configure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<LibraryDbContext>(options =>
   options.UseSqlite(builder.Configuration.GetConnectionString("CampusLibraryDb")));

builder.Services.AddScoped<IReadersDbContext>(sp =>
   sp.GetRequiredService<LibraryDbContext>());

builder.Services.AddScoped<IUnitOfWork, UnitOfWorkEf>();

builder.Services.AddReadersModule();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
