var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


var produtos = new[]
{
    new {Id = 1, Nome = "Notebook", Quantidade = 10},
    new {Id = 2, Nome = "Mouse", Quantidade = 25},
    new {Id = 3, Nome = "Teclado", Quantidade = 15},
};

app.MapGet("/produtos", () =>
{
    return produtos;
});

app.Run();
