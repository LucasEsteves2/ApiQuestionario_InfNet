import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { Questionario, CriarQuestionarioRequest, Resultados } from '../models/api.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class QuestionarioService {
  private apiUrl = `${environment.apiUrl}/questionario`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  listar(): Observable<any> {
    return this.http.get(`${this.apiUrl}`, {
      headers: this.authService.getHeaders()
    });
  }

  obterPorId(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`, {
      headers: this.authService.getHeaders()
    });
  }

  criar(request: CriarQuestionarioRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}`, request, {
      headers: this.authService.getHeaders()
    });
  }

  obterResultados(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/resultados`, {
      headers: this.authService.getHeaders()
    });
  }

  encerrar(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/encerrar`, {}, {
      headers: this.authService.getHeaders()
    });
  }

  deletar(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.authService.getHeaders()
    });
  }
}
