import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.scss']
})
export class RegisterComponent {
  nome = '';
  email = '';
  senha = '';
  confirmarSenha = '';
  loading = false;
  error = '';
  success = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.error = '';
    this.success = '';

    if (!this.nome || !this.email || !this.senha || !this.confirmarSenha) {
      this.error = 'Preencha todos os campos';
      return;
    }

    if (this.senha !== this.confirmarSenha) {
      this.error = 'As senhas não coincidem';
      return;
    }

    if (this.senha.length < 6) {
      this.error = 'A senha deve ter pelo menos 6 caracteres';
      return;
    }

    this.loading = true;

    this.authService.register({
      nome: this.nome,
      email: this.email,
      senha: this.senha
    }).subscribe({
      next: (response) => {
        this.success = 'Cadastro realizado com sucesso! Redirecionando...';
        setTimeout(() => {
          this.router.navigate(['/admin']);
        }, 1500);
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao cadastrar. Tente novamente.';
        this.loading = false;
      }
    });
  }
}
