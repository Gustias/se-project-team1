using BookClub.Api.Data;
using BookClub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

<<<<<<< HEAD
=======
builder.Services.AddDbContext<AppDbContext>();

>>>>>>> 64cfb316cc62c3702457920a9a1aff462f5be0a1
builder.Services.AddHttpClient<BookService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "BookClubApp"
    );
});
<<<<<<< HEAD
=======

builder.Services.AddScoped<ReadingProgressService>();
>>>>>>> 64cfb316cc62c3702457920a9a1aff462f5be0a1

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