namespace Faturamento.Api.Models;

public class ItemNotaFiscal
{
    public int Id { get; set; }

    public int NotaFiscalId { get; set; }

    public int ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal { get; set; }
}