using Estoque.Api.Data;
using Estoque.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//O AddDbContext registra nosso contexto no sistema de injeção de dependência do ASP.NET Core, 
// permitindo que ele seja injetado em controladores ou outros serviços quando necessário.
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EstoqueConnection"))
    );

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// Retorna todos os produtos
app.MapGet("/produtos", async (EstoqueDbContext db) =>
{
    var produtos = await db.Produtos.ToListAsync();

    return Results.Ok(produtos);
});

// Retorna um produto pelo ID
app.MapGet("/produtos/{id}", async (int id, EstoqueDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(produto);
});

app.MapPost("/produtos", async (Produto novoProduto, EstoqueDbContext db) =>
{
    novoProduto.Id = 0; // Garante que o ID seja gerado pelo banco de dados
    db.Produtos.Add(novoProduto);
    await db.SaveChangesAsync();
    return Results.Created($"/produtos/{novoProduto.Id}", novoProduto);
});

app.MapPut("/produtos/{id}", async (int id, Produto produtoAtualizado, EstoqueDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if (produto == null)
    {
        return Results.NotFound();
    }
    produto.Nome = produtoAtualizado.Nome;
    produto.Quantidade = produtoAtualizado.Quantidade;
    produto.Preco = produtoAtualizado.Preco;
    await db.SaveChangesAsync();
    
    return Results.Ok(produto);
});

app.MapDelete("/produtos/{id}", async (int id, EstoqueDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if (produto == null)
    {
        return Results.NotFound();
    }
    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

app.MapPost("/produtos/{id}/baixar", async (
    int id,
    int quantidade,
    EstoqueDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto is null)
    {
        return Results.NotFound();
    }

    if (quantidade <= 0)
    {
        return Results.BadRequest("A quantidade deve ser maior que zero.");
    }

    if (produto.Quantidade < quantidade)
    {
        return Results.BadRequest("Estoque insuficiente.");
    }

    produto.Quantidade -= quantidade;

    await db.SaveChangesAsync();

    return Results.Ok(produto);
});

app.Run();
