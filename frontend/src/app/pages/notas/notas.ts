import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Produto } from '../../models/produto';
import { ItemNotaFiscal } from '../../models/item-nota-fiscal';
import { NotaFiscal } from '../../models/nota-fiscal';
import { EstoqueService } from '../../services/estoque.service';
import { FaturamentoService } from '../../services/faturamento.service';

@Component({
  selector: 'app-notas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notas.html',
  styleUrl: './notas.css'
})
export class Notas implements OnInit {
  produtos: Produto[] = [];
  notas: NotaFiscal[] = [];

  produtoSelecionadoId: number | null = null;
  quantidade = 1;

  itens: ItemNotaFiscal[] = [];

  carregando = false;
  mensagem = '';
  erro = '';

  constructor(
    private estoqueService: EstoqueService,
    private faturamentoService: FaturamentoService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.carregarProdutos();
    this.carregarNotas();
  }

  carregarProdutos(): void {
    this.estoqueService.listar().subscribe({
      next: (produtos) => {
        this.produtos = produtos;
        this.cdr.markForCheck();
      },
      error: () => {
        this.erro = 'Não foi possível carregar os produtos.';
        this.cdr.markForCheck();
      }
    });
  }

  carregarNotas(): void {
    this.carregando = true;

    this.faturamentoService.listar().subscribe({
      next: (notas) => {
        this.notas = notas;
        this.carregando = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.erro = 'Não foi possível carregar as notas fiscais.';
        this.carregando = false;
        this.cdr.markForCheck();
      }
    });
  }

  adicionarItem(): void {
    this.mensagem = '';
    this.erro = '';

    if (this.produtoSelecionadoId === null) {
      this.erro = 'Selecione um produto.';
      return;
    }

    if (this.quantidade <= 0) {
      this.erro = 'A quantidade deve ser maior que zero.';
      return;
    }

    const produto = this.produtos.find(
      p => p.id === Number(this.produtoSelecionadoId)
    );

    if (!produto) {
      this.erro = 'Produto não encontrado.';
      return;
    }

    const itemExistente = this.itens.find(
      item => item.produtoId === produto.id
    );

    if (itemExistente) {
      itemExistente.quantidade += this.quantidade;
    } else {
      this.itens.push({
        produtoId: produto.id,
        nomeProduto: produto.nome,
        quantidade: this.quantidade,
        precoUnitario: produto.preco,
        subtotal: produto.preco * this.quantidade
      });
    }

    this.recalcularItens();

    this.produtoSelecionadoId = null;
    this.quantidade = 1;
  }

  removerItem(indice: number): void {
    this.itens.splice(indice, 1);
    this.recalcularItens();
  }

  emitirNota(): void {
    this.mensagem = '';
    this.erro = '';

    if (this.itens.length === 0) {
      this.erro = 'Adicione pelo menos um item à nota.';
      return;
    }

    const nota: NotaFiscal = {
      itens: this.itens.map(item => ({
        produtoId: item.produtoId,
        quantidade: item.quantidade
      }))
    };

    this.faturamentoService.emitir(nota).subscribe({
        next: () => {
          this.mensagem = 'Nota fiscal emitida com sucesso.';
          this.itens = [];

          this.carregarProdutos();
          this.carregarNotas();
          this.cdr.markForCheck();
        },
        error: (erro) => {
          if (erro.status === 400) {
            this.erro =
              erro.error?.detail ??
              erro.error ??
              'Não foi possível emitir a nota.';
            } else if (erro.status === 503) {
            this.erro = 'O serviço de estoque está indisponível.';
            } else {
            this.erro = 'Erro ao emitir a nota fiscal.';
            }
        this.cdr.markForCheck();
        }
      }
    );
  }

  get totalAtual(): number {
    return this.itens.reduce(
      (total, item) => total + (item.subtotal ?? 0),
      0
    );
  }

  private recalcularItens(): void {
    this.itens = this.itens.map(item => ({
      ...item,
      subtotal:
        (item.precoUnitario ?? 0) *
        item.quantidade
    }));
  }
}