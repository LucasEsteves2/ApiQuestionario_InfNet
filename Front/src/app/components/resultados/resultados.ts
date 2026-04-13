import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-resultados',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './resultados.html',
  styleUrl: './resultados.scss',
})
export class Resultados implements OnInit {
  resultados: any = null;
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.carregarResultados(id);
    }
  }

  carregarResultados(id: string): void {
    this.loading = true;
    console.log('Carregando resultados:', id);
    
    const token = localStorage.getItem('token');
    const headers = {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };

    this.http.get(`${environment.apiUrl}/questionario/${id}/resultados`, { headers }).subscribe({
      next: (response: any) => {
        console.log('✅ Resultados recebidos:', response);
        this.resultados = response.data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('❌ Erro ao carregar resultados:', err);
        this.error = err.error?.message || 'Erro ao carregar resultados';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getTotalVotosPergunta(pergunta: any): number {
    return pergunta.opcoes?.reduce((sum: number, opcao: any) => sum + opcao.quantidade, 0) || 0;
  }

  isVencedor(opcao: any, opcoes: any[]): boolean {
    if (!opcoes || opcoes.length === 0) return false;
    const maxVotos = Math.max(...opcoes.map(o => o.quantidade));
    return opcao.quantidade === maxVotos && opcao.quantidade > 0;
  }

  getBarWidth(percentual: number): string {
    return `${percentual}%`;
  }

  getVencedor(opcoes: any[]): any {
    if (!opcoes || opcoes.length === 0) return null;
    return opcoes.reduce((prev, current) => 
      (prev.quantidade > current.quantidade) ? prev : current
    );
  }
}
