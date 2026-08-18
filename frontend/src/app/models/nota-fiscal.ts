import { ItemNotaFiscal } from './item-nota-fiscal';

export interface NotaFiscal {
  id?: number;
  dataEmissao?: string;
  status?: string;
  valorTotal?: number;
  itens: ItemNotaFiscal[];
}