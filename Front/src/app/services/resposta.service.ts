import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { RegistrarRespostaRequest } from '../models/api.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RespostaService {
  private apiUrl = `${environment.apiUrl}/resposta`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  registrar(request: RegistrarRespostaRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}`, request, {
      headers: this.authService.getHeaders()
    });
  }

  listarPorQuestionario(questionarioId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/questionario/${questionarioId}`, {
      headers: this.authService.getHeaders()
    });
  }
}
