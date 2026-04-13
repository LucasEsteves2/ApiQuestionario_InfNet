import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-votar-publico',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './votar-publico.html',
  styleUrl: './votar-publico.scss',
})
export class VotarPublico implements OnInit {
  questionarios: any[] = [];
  loading = true;
  error = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.carregarQuestionarios();
  }

  carregarQuestionarios(): void {
    this.loading = true;

    this.http.get(`${environment.apiUrl}/questionario`).subscribe({
      next: (response: any) => {
        this.questionarios = (response.data || []).filter(
          (q: any) => q.status === 'Ativo'
        );
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Erro ao carregar votações:', err);
        this.error = 'Erro ao carregar votações disponíveis';
        this.loading = false;
      },
    });
  }
}
