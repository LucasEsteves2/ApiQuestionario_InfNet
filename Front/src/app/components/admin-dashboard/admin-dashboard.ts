import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { QuestionarioService } from '../../services/questionario.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.scss']
})
export class AdminDashboardComponent implements OnInit {
  questionarios: any[] = [];
  loading = true;
  userName: string | null = '';

  constructor(
    private questionarioService: QuestionarioService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.userName = this.authService.getUserName();
    this.carregarQuestionarios();
  }

  carregarQuestionarios(): void {
    this.loading = true;
    console.log('Carregando questionários...');
    
    this.questionarioService.listar().subscribe({
      next: (response: any) => {
        console.log('Resposta da API:', response);
        this.questionarios = response.data || [];
        this.loading = false;
        console.log('Questionários carregados:', this.questionarios.length);
        console.log('Loading:', this.loading);
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Erro ao carregar questionários', err);
        this.loading = false;
        this.cdr.detectChanges();
        if (err.status === 401) {
          alert('Sessão expirada. Faça login novamente.');
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      }
    });
  }

  encerrarQuestionario(id: string): void {
    if (confirm('Deseja realmente encerrar esta votação?')) {
      this.questionarioService.encerrar(id).subscribe({
        next: () => {
          alert('Votação encerrada com sucesso!');
          this.carregarQuestionarios();
        },
        error: () => {
          alert('Erro ao encerrar votação');
        }
      });
    }
  }

  deletarQuestionario(id: string): void {
    if (confirm('Deseja realmente deletar esta votação? Esta ação não pode ser desfeita!')) {
      this.questionarioService.deletar(id).subscribe({
        next: () => {
          alert('Votação deletada com sucesso!');
          this.carregarQuestionarios();
        },
        error: () => {
          alert('Erro ao deletar votação');
        }
      });
    }
  }
}
