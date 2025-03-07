using Microsoft.EntityFrameworkCore;
using TodoApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Añade servicios al contenedor.

builder.Services.AddControllers();
builder.Services.AddDbContext<TodoContext>(opt1 => opt1.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<VideoclubDbContext>(opt2 => opt2.UseInMemoryDatabase("Videoclub"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
