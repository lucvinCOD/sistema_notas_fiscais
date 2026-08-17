using Faturamento.Api.Data;
using Faturamento.Api.Models;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    private static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // Add services to the container.
        builder.Services.AddOpenApi();

        // Add Entity Framework
        builder.Services.AddDbContext<FaturamentoDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("FaturamentoConnection")));
        
        // Add HttpClient for making HTTP requests
        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()){
                app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapPost("/notas", async (NotaFiscal novaNota, FaturamentoDbContext db, IHttpClientFactory httpClientFactory) =>
            {
                var httpClient = httpClientFactory.CreateClient();
                novaNota.Id = 0;
                novaNota.DataEmissao = DateTime.Now;
                novaNota.Status = "Emitida";


                var produtosValidados = new List<(ItemNotaFiscal Item, ProdutoEstoque Produto)>();

                // FASE 1: validar TODOS os produtos antes de alterar o estoque
                foreach (var item in novaNota.Itens){
                   ProdutoEstoque? produto;
                    try{
                        var resposta = await httpClient.GetAsync(
                            $"http://localhost:5266/produtos/{item.ProdutoId}"
                        );

                        if (resposta.StatusCode == System.Net.HttpStatusCode.NotFound){
                            return Results.NotFound(
                                $"Produto {item.ProdutoId} não encontrado."
                            );
                        }

                        if (!resposta.IsSuccessStatusCode){
                            return Results.Problem(
                                "O serviço de estoque retornou um erro.",
                                statusCode: 502
                            );
                        }

                        produto = await resposta.Content
                        .ReadFromJsonAsync<ProdutoEstoque>();
                    }

                    catch (HttpRequestException){
                        return Results.Problem(
                            "O serviço de estoque está indisponível.",
                            statusCode: 503
                        );
                    }

                    if (produto is null){
                        return Results.BadRequest(
                        $"Produto {item.ProdutoId} não encontrado no estoque."
                        );
                    }

                    if (item.Quantidade <= 0){
                        return Results.BadRequest(
                        $"A quantidade do produto {produto.Nome} deve ser maior que zero."
                        );
                    }

                    if (produto.Quantidade < item.Quantidade){
                        return Results.BadRequest(
                        $"Estoque insuficiente para o produto {produto.Nome}."
                        );
                    }
                    // Adiciona o item e o produto validado à lista
                    item.Id = 0;
                    item.NomeProduto = produto.Nome;
                    item.PrecoUnitario = produto.Preco;
                    item.Subtotal = item.Quantidade * produto.Preco;
                    produtosValidados.Add((item, produto));
                }

                // FASE 2: baixar o estoque dos produtos validados
                foreach (var validado in produtosValidados){
                    try{
                        var respostaBaixa = await httpClient.PostAsync(
                        $"http://localhost:5266/produtos/{validado.Item.ProdutoId}/baixar?quantidade={validado.Item.Quantidade}",
                        null
                        );

                        if (!respostaBaixa.IsSuccessStatusCode){
                            return Results.Problem(
                            $"Não foi possível baixar o estoque do produto {validado.Item.ProdutoId}.",
                            statusCode: 500
                            );
                        }
                    }

                    catch (HttpRequestException){
                        return Results.Problem(
                        "O serviço de estoque ficou indisponivel durante a operação.",
                        statusCode: 503
                        );
                    }
                }
            

                //Soma os subtotais dos itens para calcular o valor total da nota fiscal
                novaNota.ValorTotal = novaNota.Itens.Sum(item => item.Subtotal);

                // Adiciona a nota fiscal ao banco de dados
                db.NotasFiscais.Add(novaNota);

                // Salva as alterações no banco de dados
                await db.SaveChangesAsync();

                return Results.Created($"/notas/{novaNota.Id}", novaNota);
            }
        );

       app.MapGet("/notas", async (FaturamentoDbContext db) =>
            {
            var notas = await db.NotasFiscais
            .Include(n => n.Itens)
            .ToListAsync();

            return Results.Ok(notas);
            }
        ); 

        app.MapGet("/notas/{id}", async (int id, FaturamentoDbContext db) =>
            {
            var nota = await db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

            if (nota is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(nota);
            }
        );

        app.Run();
        return 0;
    }
}