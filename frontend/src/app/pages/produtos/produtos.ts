import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Produto } from '../../models/produto';
import { EstoqueService } from '../../services/estoque.service';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './produtos.html',
  styleUrl: './produtos.css'
})
export class Produtos implements OnInit {
  produtos: Produto[] = [];

  nome = '';
  quantidade = 0;
  preco = 0;

  editandoId: number | null = null;

  carregando = false;
  mensagem = '';
  erro = '';

  constructor(private estoqueService: EstoqueService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregando = true;
    this.erro = '';

    this.estoqueService.listar().subscribe(
      {
        next: (produtos) => {
          this.produtos = produtos;
          this.carregando = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.erro = 'Não foi possível carregar os produtos.';
          this.carregando = false;
          this.cdr.markForCheck();
        }
      }
    );
  }

  salvar(): void {
    this.mensagem = '';
    this.erro = '';

    if (!this.nome.trim()) {
      this.erro = 'Informe o nome do produto.';
      return;
    }

    if (this.quantidade < 0) {
      this.erro = 'A quantidade não pode ser negativa.';
      return;
    }

    if (this.preco < 0) {
      this.erro = 'O preço não pode ser negativo.';
      return;
    }

    const produto = {
      nome: this.nome.trim(),
      quantidade: this.quantidade,
      preco: this.preco
    };

    if (this.editandoId !== null) {
      this.estoqueService.atualizar(this.editandoId, produto).subscribe({
        next: () => {
          this.mensagem = 'Produto atualizado com sucesso.';
          this.limparFormulario();
          this.carregarProdutos();
        },
        error: () => {
          this.erro = 'Não foi possível atualizar o produto.';
        }
      });

      return;
    }

    this.estoqueService.criar(produto).subscribe({
      next: () => {
        this.mensagem = 'Produto cadastrado com sucesso.';
        this.limparFormulario();
        this.carregarProdutos();
      },
      error: () => {
        this.erro = 'Não foi possível cadastrar o produto.';
      }
    });
  }

  editar(produto: Produto): void {
    this.editandoId = produto.id;
    this.nome = produto.nome;
    this.quantidade = produto.quantidade;
    this.preco = produto.preco;

    this.mensagem = '';
    this.erro = '';
  }

  excluir(produto: Produto): void {
    const confirmou = confirm(
      `Deseja excluir o produto "${produto.nome}"?`
    );

    if (!confirmou) {
      return;
    }

    this.estoqueService.excluir(produto.id).subscribe({
      next: () => {
        this.mensagem = 'Produto excluído com sucesso.';
        this.carregarProdutos();
      },
      error: () => {
        this.erro = 'Não foi possível excluir o produto.';
      }
    });
  }

  cancelarEdicao(): void {
    this.limparFormulario();
  }

  private limparFormulario(): void {
    this.nome = '';
    this.quantidade = 0;
    this.preco = 0;
    this.editandoId = null;
  }
}