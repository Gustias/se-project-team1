var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient<BookService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "BookClubApp");                 // Need to create & add project email
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