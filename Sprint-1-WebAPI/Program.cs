using Interfaces;
using Sprint_1_WebAPI.BackgroundServices;
using Sprint_1_WebAPI.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IBookingStore, InMemoryBookingStore>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingProcessingBackgroundService>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();