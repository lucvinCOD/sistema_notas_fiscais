import { Routes } from '@angular/router';

import { Produtos } from './pages/produtos/produtos';
import { Notas } from './pages/notas/notas';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'produtos',
    pathMatch: 'full'
  },
  {
    path: 'produtos',
    component: Produtos
  },
  {
    path: 'notas',
    component: Notas
  },
  {
    path: '**',
    redirectTo: 'produtos'
  }
];