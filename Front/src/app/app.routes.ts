import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard';
import { CriarQuestionario } from './components/criar-questionario/criar-questionario';
import { VotarPublico } from './components/votar-publico/votar-publico';
import { Votar } from './components/votar/votar';
import { Resultados } from './components/resultados/resultados';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'admin', component: AdminDashboardComponent },
  { path: 'criar-questionario', component: CriarQuestionario },
  { path: 'votar-publico', component: VotarPublico },
  { path: 'votar/:id', component: Votar },
  { path: 'resultados/:id', component: Resultados },
  { path: '**', redirectTo: '' }
];
