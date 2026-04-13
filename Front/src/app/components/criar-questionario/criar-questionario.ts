import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { QuestionarioService } from '../../services/questionario.service';

interface Opcao {
  texto: string;
  ordem: number;
}

interface Pergunta {
  texto: string;
  ordem: number;
  obrigatoria: boolean;
  opcoes: Opcao[];
}

@Component({
  selector: 'app-criar-questionario',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './criar-questionario.html',
  styleUrl: './criar-questionario.scss',
})
export class CriarQuestionario {
  titulo = '';
  descricao = '';
  dataInicio = '';
  dataFim = '';
  perguntas: Pergunta[] = [{
    texto: '',
    ordem: 1,
    obrigatoria: true,
    opcoes: [
      { texto: '', ordem: 1 },
      { texto: '', ordem: 2 }
    ]
  }];
  loading = false;
  error = '';

  constructor(
    private questionarioService: QuestionarioService,
    private router: Router
  ) {
    const hoje = new Date();
    const umMes = new Date(hoje);
    umMes.setMonth(umMes.getMonth() + 1);
    
    this.dataInicio = hoje.toISOString().split('T')[0];
    this.dataFim = umMes.toISOString().split('T')[0];
  }

  adicionarPergunta(): void {
    this.perguntas.push({
      texto: '',
      ordem: this.perguntas.length + 1,
      obrigatoria: true,
      opcoes: [
        { texto: '', ordem: 1 },
        { texto: '', ordem: 2 }
      ]
    });
  }

  removerPergunta(index: number): void {
    if (this.perguntas.length > 1) {
      this.perguntas.splice(index, 1);
      this.perguntas.forEach((p, i) => p.ordem = i + 1);
    }
  }

  adicionarOpcao(pergunta: Pergunta): void {
    pergunta.opcoes.push({
      texto: '',
      ordem: pergunta.opcoes.length + 1
    });
  }

  removerOpcao(pergunta: Pergunta, index: number): void {
    if (pergunta.opcoes.length > 2) {
      pergunta.opcoes.splice(index, 1);
      pergunta.opcoes.forEach((o, i) => o.ordem = i + 1);
    }
  }

  onSubmit(): void {
    this.error = '';

    if (!this.titulo || !this.descricao || !this.dataInicio || !this.dataFim) {
      this.error = 'Preencha todos os campos obrigatórios';
      return;
    }

    for (const p of this.perguntas) {
      if (!p.texto) {
        this.error = 'Todas as perguntas devem ter texto';
        return;
      }
      for (const o of p.opcoes) {
        if (!o.texto) {
          this.error = 'Todas as opções devem ter texto';
          return;
        }
      }
    }

    this.loading = true;

    const request = {
      titulo: this.titulo,
      descricao: this.descricao,
      dataInicio: new Date(this.dataInicio).toISOString(),
      dataFim: new Date(this.dataFim + 'T23:59:59').toISOString(),
      perguntas: this.perguntas
    };

    this.questionarioService.criar(request).subscribe({
      next: () => {
        alert('Votação criada com sucesso!');
        this.router.navigate(['/admin']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao criar votação';
        this.loading = false;
      }
    });
  }
}
