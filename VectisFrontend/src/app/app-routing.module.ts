import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportDepenseComponent } from './features/import-depense/import-depense.component';

const routes: Routes = [
  { path: 'import-depense', component: ImportDepenseComponent },
  { path: '', redirectTo: '/import-depense', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }