import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { QuestionarioService } from '../../services/questionario.service';
import { RespostaService } from '../../services/resposta.service';
import { AuthService } from '../../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-votar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './votar.html',
  styleUrl: './votar.scss',
})
export class Votar implements OnInit {
  questionario: any = null;
  loading = true;
  error = '';
  success = false;
  respostas: { [perguntaId: string]: string } = {};
  enviando = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private questionarioService: QuestionarioService,
    private respostaService: RespostaService,
    private authService: AuthService,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.carregarQuestionario(id);
    }
  }

  carregarQuestionario(id: string): void {
    this.loading = true;
    console.log('Carregando questionário:', id);
    
    this.http.get(`${environment.apiUrl}/questionario/${id}`).subscribe({
      next: (response: any) => {
        console.log('✅ Questionário recebido:', response);
        this.questionario = response.data;
        this.loading = false;
        
        if (this.questionario.status !== 'Ativo') {
          this.error = 'Esta votação está encerrada';
        }
        
        this.cdr.detectChanges();
        console.log('✅ Loading:', this.loading);
      },
      error: (err: any) => {
        console.error('❌ Erro ao carregar questionário:', err);
        this.error = 'Votação não encontrada';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSubmit(): void {
    this.error = '';

    const perguntasObrigatorias = this.questionario.perguntas.filter((p: any) => p.obrigatoria);
    const todasRespondidas = perguntasObrigatorias.every((p: any) => this.respostas[p.id]);

    if (!todasRespondidas) {
      this.error = 'Por favor, responda todas as perguntas obrigatórias';
      return;
    }

    this.enviando = true;

    const request = {
      questionarioId: this.questionario.id,
      respostas: Object.keys(this.respostas).map(perguntaId => ({
        perguntaId,
        opcaoRespostaId: this.respostas[perguntaId]
      }))
    };

    console.log('Enviando voto:', request);

    this.http.post(`${environment.apiUrl}/resposta`, request).subscribe({
      next: () => {
        console.log('✅ Voto registrado com sucesso!');
        this.success = true;
        this.cdr.detectChanges();
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 3000);
      },
      error: (err: any) => {
        console.error('❌ Erro ao registrar voto:', err);
        this.error = err.error?.message || 'Erro ao registrar voto';
        this.enviando = false;
        this.cdr.detectChanges();
      }
    });
  }
}
