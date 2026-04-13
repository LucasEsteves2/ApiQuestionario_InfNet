
# Sistema de Votação Online - Pós-Graduação Infnet

Este projeto é um frontend desenvolvido em Angular para o sistema de questionário/votação online da pós-graduação do Infnet. Ele permite a criação, participação e acompanhamento de votações de forma segura, transparente e moderna, integrando-se à API oficial do backend.

## Contexto
O sistema foi criado para facilitar a realização de votações e enquetes acadêmicas, promovendo a participação ativa dos alunos e a transparência nos resultados. O frontend consome a API REST do backend, permitindo autenticação, criação de questionários, votação e visualização de resultados em tempo real.

## Funcionalidades
- Autenticação de usuários (JWT)
- Criação de questionários e enquetes (admin)
- Participação em votações públicas e privadas
- Resultados em tempo real
- Interface responsiva e intuitiva
- Segurança e transparência nos processos

## Como usar
### Pré-requisitos
- Node.js e npm instalados
- API backend de questionário online em execução (projeto de worker e Api devem estar rodando!)

### Instalação
1. Clone este repositório
2. Instale as dependências:
	```bash
	npm install
	```
3. Configure a URL da API no arquivo `src/environments/environment.ts` se necessário.

### Servidor de desenvolvimento
Para iniciar o servidor local, execute:
```bash
ng serve
```
Acesse `http://localhost:4200/` no navegador.

