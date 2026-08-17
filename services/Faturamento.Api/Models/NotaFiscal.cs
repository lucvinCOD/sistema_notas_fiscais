namespace Faturamento.Api.Models;

public class NotaFiscal
{
    public int Id { get; set; }

    public DateTime DataEmissao { get; set; }

    public string Status { get; set; } = "Aberta";

    public decimal ValorTotal { get; set; }

    public List<ItemNotaFiscal> Itens { get; set; } = new();
}