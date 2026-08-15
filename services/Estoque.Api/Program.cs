using Estoque.Api.Models;
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


var produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Notebook", Quantidade = 10, Preco = 3500.00m},
    new Produto { Id = 2, Nome = "Mouse", Quantidade = 25, Preco = 120.00m },
    new Produto { Id = 3, Nome = "Teclado", Quantidade = 15, Preco = 250.00m },
    new Produto { Id = 4, Nome = "Monitor", Quantidade = 30, Preco = 2050.00m },
    new Produto { Id = 5, Nome = "Gabinete", Quantidade = 28, Preco = 1250.00m },
    new Produto { Id = 6, Nome = "Estabilizador", Quantidade = 7, Preco = 750.00m },
};

app.MapGet("/produtos", () => {return produtos;});

app.MapGet("/produtos/{id}", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    if (produto == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(produto);

});

app.MapPost("/produtos", (Produto novoProduto) =>
{
    novoProduto.Id = produtos.Max(p => p.Id) + 1;
    produtos.Add(novoProduto);
    return Results.Created($"/produtos/{novoProduto.Id}", novoProduto);
});

app.MapPut("/produtos/{id}", (int id, Produto produtoAtualizado) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    if (produto == null)
    {
        return Results.NotFound();
    }
    produto.Nome = produtoAtualizado.Nome;
    produto.Quantidade = produtoAtualizado.Quantidade;
    produto.Preco = produtoAtualizado.Preco;
    return Results.Ok(produto);
});

app.MapDelete("/produtos/{id}", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    if (produto == null)
    {
        return Results.NotFound();
    }
    produtos.Remove(produto);
    return Results.NoContent();
});

app.Run();
