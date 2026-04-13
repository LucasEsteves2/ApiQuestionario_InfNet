export interface LoginRequest {
  email: string;
  senha: string;
}

export interface RegisterRequest {
  nome: string;
  email: string;
  senha: string;
}

export interface LoginResponse {
  usuarioId: string;
  nome: string;
  email: string;
  token: string;
  role: string;
}

export interface Questionario {
  id: string;
  titulo: string;
  descricao: string;
  status: string;
  dataInicio: string;
  dataFim: string;
  dataCriacao: string;
  dataEncerramento?: string;
  perguntas: Pergunta[];
}

export interface Pergunta {
  id: string;
  texto: string;
  ordem: number;
  obrigatoria: boolean;
  opcoes: OpcaoResposta[];
}

export interface OpcaoResposta {
  id: string;
  texto: string;
  ordem: number;
}

export interface CriarQuestionarioRequest {
  titulo: string;
  descricao: string;
  dataInicio: string;
  dataFim: string;
  perguntas: CriarPerguntaRequest[];
}

export interface CriarPerguntaRequest {
  texto: string;
  ordem: number;
  obrigatoria: boolean;
  opcoes: CriarOpcaoRequest[];
}

export interface CriarOpcaoRequest {
  texto: string;
  ordem: number;
}

export interface RegistrarRespostaRequest {
  questionarioId: string;
  respostas: RespostaItem[];
  estado?: string;
  cidade?: string;
  regiaoGeografica?: string;
}

export interface RespostaItem {
  perguntaId: string;
  opcaoRespostaId: string;
}

export interface Resultados {
  questionarioId: string;
  titulo: string;
  totalRespostas: number;
  perguntas: ResultadoPergunta[];
}

export interface ResultadoPergunta {
  perguntaId: string;
  texto: string;
  totalRespostas: number;
  opcoes: ResultadoOpcao[];
}

export interface ResultadoOpcao {
  opcaoId: string;
  texto: string;
  quantidade: number;
  percentual: number;
}
