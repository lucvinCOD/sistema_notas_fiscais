namespace Faturamento.Api.Models;

public class ProdutoEstoque
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal Preco { get; set; }
}