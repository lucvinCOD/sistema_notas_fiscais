// Código que representa o contexto do banco de dados para a aplicação Estoque.Api.
// Ele herda de DbContext, que é a classe base do Entity Framework Core para trabalhar com bancos de dados relacionais
// É o intermediário entre a aplicação e o banco de dados, permitindo realizar operações de 
// CRUD (Create, Read, Update, Delete) nos modelos definidos na aplicação
// EF Core é um ORM que permite trabalhar com um banco relacional usando objetos e classes C#
using Estoque.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; } //Aqui diz ao Entity Framework que a classe Produto será mapeada
    //para uma tabela no banco de dados, permitindo realizar operações de CRUD nessa tabela por meio do DbSet.
}