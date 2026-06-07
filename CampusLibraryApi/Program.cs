using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._4_Infrastructure.Persistence;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
using CampusLibraryApi._4_Infrastructure.Persistence.Readers;
using CampusLibraryApi.Configure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlite(builder.Configuration.GetConnectionString("CampusLibraryDb")));

builder.Services.AddScoped<IReaderDbContext, ReaderDbContextEf>();
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
