import { Routes } from '@angular/router';
import { CreateGroup } from './features/group/create-group/create-group';
import { GroupDetailComponent } from './features/group/group-detail/group-detail';

export const routes: Routes = [
  { path: '', redirectTo: 'create-group', pathMatch: 'full' },
  { path: 'create-group', component: CreateGroup },
  {
    path: 'group/:id',
    loadComponent: () =>
      import('./features/group/group-detail/group-detail')
        .then(m => m.GroupDetailComponent)
  },
  {
    path: 'groups/:id',
    component: GroupDetailComponent
  }
];

