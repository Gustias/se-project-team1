using BookClub.Api.Data;
using BookClub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<ReadingProgressService>();

builder.Services.AddHttpClient<BookService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "BookClubApp"
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("ReactApp");

app.MapControllers();

app.Run();