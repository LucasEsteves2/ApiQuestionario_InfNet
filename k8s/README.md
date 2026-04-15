# 🚀 **Kubernetes Deployment - Projeto de Disciplina**

> **Aluno:** Lucas Esteves  
> **Disciplina:** Infraestrutura e Deployment com Kubernetes  
> **Repositório:** [ApiQuestionario_InfNet](https://github.com/LucasEsteves2/ApiQuestionario_InfNet)  
> **Docker Hub:** [luqui25/lucas-fluminense-backend](https://hub.docker.com/r/luqui25/lucas-fluminense-backend)

---

## 📋 **Visão Geral do Projeto**

Sistema de **Questionário Online** construído com:
- **Backend:** .NET 8 Web API (4 réplicas para alta disponibilidade)
- **Frontend:** Angular 18 com Nginx
- **Banco de Dados:** MongoDB 7 com persistência
- **Message Broker:** RabbitMQ 3.13 com management UI
- **Monitoramento:** Prometheus + Grafana

**Objetivo:** Demonstrar conhecimentos em Docker, Kubernetes, Monitoramento e CI/CD.

---

## 🏗️ **Arquitetura do Sistema**

```
┌────────────────────────────────────────────────────────────────┐
│                    KUBERNETES CLUSTER                          │
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  NAMESPACE: questionario                                 │ │
│  │                                                          │ │
│  │  ┌─────────────┐     ┌──────────────┐                  │ │
│  │  │  Frontend   │────▶│   Backend    │                  │ │
│  │  │  (Angular)  │     │   (.NET 8)   │                  │ │
│  │  │  NodePort   │     │  4 Réplicas  │                  │ │
│  │  │  :30080     │     │  NodePort    │                  │ │
│  │  └─────────────┘     │  :30500      │                  │ │
│  │                       └──────┬───────┘                  │ │
│  │                              │                          │ │
│  │         ┌────────────────────┼────────────────┐        │ │
│  │         ▼                    ▼                ▼        │ │
│  │  ┌─────────────┐    ┌──────────────┐  ┌──────────┐   │ │
│  │  │  MongoDB    │    │   RabbitMQ   │  │Prometheus│   │ │
│  │  │  ClusterIP  │    │  ClusterIP   │  │ClusterIP │   │ │
│  │  │  PVC: 1GB   │    │  PVC: 500MB  │  │PVC: 2GB  │   │ │
│  │  └─────────────┘    └──────────────┘  └────┬─────┘   │ │
│  │                                              │         │ │
│  │                       ┌──────────────────────┘         │ │
│  │                       ▼                                │ │
│  │                ┌─────────────┐                         │ │
│  │                │   Grafana   │                         │ │
│  │                │  NodePort   │                         │ │
│  │                │  :30300     │                         │ │
│  │                └─────────────┘                         │ │
│  └──────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘

🌐 ACESSOS EXTERNOS:
   ✅ Frontend:   http://<minikube-ip>:30080
   ✅ Backend:    http://<minikube-ip>:30500
   ✅ Grafana:    http://<minikube-ip>:30300
   ✅ RabbitMQ:   http://<minikube-ip>:31567
```

---

## 📁 **Estrutura do Projeto**

```
k8s/
│
├── 📄 deploy.yaml                   # ⚡ Arquivo único com TODOS os recursos
│                                    #    Uso: kubectl apply -f deploy.yaml
│
├── 📂 base/                         # 📦 Arquivos separados por categoria
│   ├── namespace.yaml               # Namespace "questionario"
│   ├── secrets.yaml                 # Senhas (MongoDB, RabbitMQ)
│   │
│   ├── 📂 database/
│   │   ├── mongodb.yaml             # MongoDB + PVC (1GB)
│   │   └── rabbitmq.yaml            # RabbitMQ + PVC (500MB)
│   │
│   ├── 📂 application/
│   │   ├── backend.yaml             # .NET 8 API (4 réplicas)
│   │   └── frontend.yaml            # Angular + Nginx
│   │
│   └── 📂 monitoring/
│       ├── prometheus.yaml          # Servidor de métricas + PVC (2GB)
│       └── grafana.yaml             # Dashboards + NodePort
│
├── 📂 scripts/                      # 🔧 Automação
│   ├── deploy.ps1                   # Deploy automatizado
│   ├── stress-test.ps1              # Testes de carga
│   └── cleanup.ps1                  # Limpeza do cluster
│
└── 📂 docs/                         # 📚 Documentação adicional
    ├── GUIA_PROFESSOR.md            # Guia de avaliação
    ├── ESTUDO_KUBERNETES.md         # Conceitos teóricos
    └── DEPLOY_RAPIDO.md             # Comandos rápidos
```

---

## 🔧 **Como Funciona?**

### **Services no Kubernetes**

| Service | Tipo | Porta | Acesso | Descrição |
|---------|------|-------|--------|-----------|
| 🌐 **frontend** | NodePort | :30080 | Externo | Interface web do usuário |
| 🔌 **backend** | NodePort | :30500 | Externo | API REST (.NET 8) |
| 📊 **grafana** | NodePort | :30300 | Externo | Dashboards de monitoramento |
| 🐰 **rabbitmq** | NodePort | :31567 | Externo | Interface de gerenciamento |
| 💾 **mongodb** | ClusterIP | :27017 | Interno | Banco de dados (só interno) |
| 📈 **prometheus** | ClusterIP | :9090 | Interno | Coletor de métricas (só interno) |

### **Fluxo de Requisição**

```
🌐 Usuário
    │
    ├─▶ Frontend (:30080)
    │       │
    │       └─▶ Backend (:30500)
    │               │
    │               ├─▶ MongoDB (ClusterIP)
    │               │
    │               └─▶ RabbitMQ (ClusterIP)
    │
    └─▶ Grafana (:30300)
            │
            └─▶ Prometheus (ClusterIP)
                    │
                    └─▶ Scrape métricas do Backend
```

---

## ✅ **Pré-requisitos**

### **Ferramentas Necessárias**

```bash
# 1. Minikube (cluster local)
minikube version
# Se não tiver: choco install minikube

# 2. kubectl (CLI do Kubernetes)
kubectl version --client
# Se não tiver: choco install kubernetes-cli

# 3. Docker (para builds)
docker --version
# Se não tiver: choco install docker-desktop
```

### **Iniciar Cluster**

```bash
# Inicia Minikube 
minikube start 

# Verifica status
minikube status
```

---

## 🚀 **Deploy da Aplicação**


```bash
# 1. Vai para a pasta k8s
cd k8s

# 2. Aplica todos os recursos de uma vez
kubectl apply -k .

# 3. Aguarda tudo ficar pronto (até 5 min)
kubectl wait --for=condition=ready pod --all -n questionario --timeout=300s

# 4. Verifica status
kubectl get all -n questionario
```

**✅ O que foi criado:**
- ✔️ Namespace `questionario`
- ✔️ Secrets com senhas
- ✔️ MongoDB com PVC (1GB)
- ✔️ RabbitMQ com PVC (500MB)
- ✔️ Backend com 4 réplicas
- ✔️ Frontend Angular
- ✔️ kube-state-metrics (métricas do Kubernetes)
- ✔️ Prometheus com PVC (2GB) + RBAC
- ✔️ Grafana com dashboards pré-configurados


---

# 5️⃣ VERIFICAÇÃO FINAL
kubectl get pods -n questionario
kubectl get services -n questionario
kubectl get pvc -n questionario
```

**📊 Saída Esperada:**

```
NAME                        READY   STATUS    RESTARTS   AGE
backend-xxxxx-aaaaa         1/1     Running   0          2m
backend-xxxxx-bbbbb         1/1     Running   0          2m
backend-xxxxx-ccccc         1/1     Running   0          2m
backend-xxxxx-ddddd         1/1     Running   0          2m  ✅ 4 réplicas!
frontend-xxxxx-aaaaa        1/1     Running   0          2m
mongodb-xxxxx-aaaaa         1/1     Running   0          3m
rabbitmq-xxxxx-aaaaa        1/1     Running   0          3m
prometheus-xxxxx-aaaaa      1/1     Running   0          1m
grafana-xxxxx-aaaaa         1/1     Running   0          1m
```

---

## 🌐 **Acessando a Aplicação**

### **⚠️ IMPORTANTE: Port-Forward Necessário (Windows/Docker)**

Se você está usando **Minikube com Docker no Windows**, é necessário fazer **port-forward** para acessar os serviços localmente.

**Por que isso é necessário?**

O Minikube no Windows com driver Docker cria uma VM isolada. Os serviços NodePort (`:30080`, `:30500`, etc.) ficam acessíveis **dentro da VM**, mas não diretamente no `localhost` do seu PC. O port-forward "conecta" a porta local do seu PC com o serviço dentro do cluster Kubernetes.

---

### **🔧 Configurar Port-Forward (OBRIGATÓRIO)**

Abra **um terminal PowerShell** e execute:
kubectl port-forward -n questionario service/backend 5000:5000

---
### **Obter URLs do Minikube**

```bash
# Frontend (Angular)
minikube service frontend -n questionario --url
# 🔗 Exemplo: http://192.168.49.2:30080

# Backend (API .NET)
minikube service backend -n questionario --url
# 🔗 Exemplo: http://192.168.49.2:30500

# Grafana (Dashboards)
minikube service grafana -n questionario --url
# 🔗 Exemplo: http://192.168.49.2:30300

# RabbitMQ (Management UI)
minikube service rabbitmq -n questionario --url
# 🔗 Exemplo: http://192.168.49.2:31567
```

### **Credenciais de Acesso**

| Serviço | Usuário | Senha | URL |
|---------|---------|-------|-----|
| 📊 Grafana | `admin` | `admin123` | `:30300` |
| 🐰 RabbitMQ | `admin` | `admin123` | `:31567` |

---

## 📈 **Configurando Dashboards no Grafana**

### **1. Acesso Inicial**

```bash
# Pega URL do Grafana
minikube service grafana -n questionario --url

# Acessa no navegador: http://<ip>:30300
# Login: admin / admin123
```

---

## 🔥 **Stress Test - Testes de Carga**

### **Usando o Script PowerShell (Windows)**

```powershell
# Executa script pronto
cd k8s/scripts
.\stress-test.ps1

# OU executa manualmente
$url = minikube service backend -n questionario --url
1..1000 | ForEach-Object -Parallel {
    Invoke-WebRequest -Uri "$using:url/api/questionario" -Method GET -ErrorAction SilentlyContinue
} -ThrottleLimit 50
```

### **Usando Bash (Linux/Mac)**

```bash
# Pega URL do backend
url=$(minikube service backend -n questionario --url)

# Envia 1000 requisições (50 paralelas)
seq 1 1000 | xargs -P50 -I{} curl -s "$url/api/questionario" > /dev/null
```

### **📸 Capturando Evidências**

1. **ANTES do teste:** Tire print do Grafana com métricas normais
2. **EXECUTE o stress test**
3. **DURANTE o teste:** Tire prints mostrando:
   - 📈 Aumento de CPU
   - 💾 Aumento de Memória
   - 🌐 Pico de requisições HTTP
   - ⏱️ Latência aumentando
4. **DEPOIS do teste:** Mostre a aplicação se estabilizando

> 💡 **Dica:** Salve os prints com nomes descritivos:
> - `grafana-antes-stress.png`
> - `grafana-durante-stress-cpu.png`
> - `grafana-durante-stress-memoria.png`
> - `grafana-apos-stress.png`

---

## 🎯 **Checklist - Requisitos do Trabalho**

### **📦 1. Docker e Containers**

| # | Requisito | Status | Evidência | Localização |
|---|-----------|--------|-----------|-------------|
| 1.1 | Criar imagem Docker da aplicação | ✅ | Imagem publicada no DockerHub | [Docker Hub - Backend](https://hub.docker.com/r/luqui25/lucas-fluminense-backend) |
| 1.2 | Publicar imagem no Docker Hub | ✅ | `luqui25/lucas-fluminense-backend:latest` | Docker Hub |
| 1.3 | Utilizar recursos básicos do Docker (volumes) | ✅ | Volumes no docker-compose | `../docker-compose.yml` (linhas 14, 31) |

**Comandos para verificar:**
```bash
docker pull luqui25/lucas-fluminense-backend:latest
docker pull luqui25/lucas-fluminense-frontend:latest
```

---

### **☸️ 2. Kubernetes - Deployment e Alta Disponibilidade**

| # | Requisito | Status | Evidência | Localização |
|---|-----------|--------|-----------|-------------|
| 2.1 | Deployment com 4 réplicas | ✅ | Backend com 4 instâncias | `base/application/backend.yaml` (linha 12) |
| 2.2 | Expor aplicação via NodePort | ✅ | Backend NodePort :30500 | `base/application/backend.yaml` (linha 80) |
| 2.3 | Expor Grafana via NodePort | ✅ | Grafana NodePort :30300 | `base/monitoring/grafana.yaml` (linha 117) |
| 2.4 | Banco de dados com ClusterIP | ✅ | MongoDB ClusterIP :27017 | `base/database/mongodb.yaml` (linha 93) |
| 2.5 | Redis/BD adicional com ClusterIP | ✅ | RabbitMQ ClusterIP :5672 | `base/database/rabbitmq.yaml` (linha 81) |

**Comandos para verificar:**
```bash
kubectl get deployments -n questionario backend -o yaml | grep replicas
kubectl get services -n questionario
```

---

### **🔍 3. Probes - Health Checks**

| # | Requisito | Status | Evidência | Localização |
|---|-----------|--------|-----------|-------------|
| 3.1 | Readiness Probe | ✅ | Configurado em todos os deployments | `base/application/backend.yaml` (linhas 55-60) |
| 3.2 | Liveness Probe | ✅ | Configurado em todos os deployments | `base/application/backend.yaml` (linhas 61-66) |

**Exemplo de configuração:**
```yaml
readinessProbe:
  httpGet:
    path: /api/questionario
    port: 8080
  initialDelaySeconds: 15
  periodSeconds: 10

livenessProbe:
  httpGet:
    path: /api/questionario
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 15
```

**Comandos para verificar:**
```bash
kubectl describe deployment backend -n questionario | grep -A 10 "Liveness\|Readiness"
```

---

### **📊 4. Monitoramento - Prometheus e Grafana**

| # | Requisito | Status | Evidência | Localização |
|---|-----------|--------|-----------|-------------|
| 4.1 | Exportar métricas da aplicação | ✅ | Endpoint `/metrics` no backend | Backend expõe métricas ASP.NET |
| 4.2 | Prometheus fazendo scrape | ✅ | ConfigMap com scrape_configs | `base/monitoring/prometheus.yaml` (linhas 30-35) |
| 4.3 | Grafana instanciado no cluster | ✅ | Deployment + Service | `base/monitoring/grafana.yaml` |
| 4.4 | Dashboards criados no Grafana | ✅ | Queries prontas na documentação | Seção "Configurando Dashboards" acima |
| 4.5 | PVC para dados do Prometheus | ✅ | 2GB de armazenamento persistente | `base/monitoring/prometheus.yaml` (linhas 60-70) |
| 4.6 | Apenas Grafana acessível externamente | ✅ | Grafana: NodePort / Prometheus: ClusterIP | `base/monitoring/grafana.yaml` (linha 117) |

**Comandos para verificar:**
```bash
# Ver PVCs
kubectl get pvc -n questionario

# Verificar scrape targets do Prometheus
kubectl port-forward -n questionario svc/prometheus 9090:9090
# Acesse: http://localhost:9090/targets

# Verificar Grafana
minikube service grafana -n questionario --url
```

---

### **💾 5. Persistência de Dados (PVC)**

| # | Requisito | Status | Volume | Tamanho | Localização |
|---|-----------|--------|--------|---------|-------------|
| 5.1 | PVC para MongoDB | ✅ | `mongodb-pvc` | 1GB | `base/database/mongodb.yaml` (linhas 10-20) |
| 5.2 | PVC para RabbitMQ | ✅ | `rabbitmq-pvc` | 500MB | `base/database/rabbitmq.yaml` (linhas 10-20) |
| 5.3 | PVC para Prometheus | ✅ | `prometheus-pvc` | 2GB | `base/monitoring/prometheus.yaml` (linhas 60-70) |

**Comandos para verificar:**
```bash
kubectl get pvc -n questionario
kubectl describe pvc mongodb-pvc -n questionario
```

---

### **🧪 6. Testes de Carga (Stress Test)**

| # | Requisito | Status | Evidência | Localização |
|---|-----------|--------|-----------|-------------|
| 6.1 | Stress test via script | ✅ | Script PowerShell automatizado | `scripts/stress-test.ps1` |
| 6.2 | Dashboard mostrando alterações | ✅ | Instruções para captura | Seção "Stress Test" acima |

**Como executar:**
```powershell
cd k8s/scripts
.\stress-test.ps1
```

---

### **🔄 7. CI/CD - Pipeline de Entrega (OPCIONAL)**

| # | Requisito | Status | Evidência | Observação |
|---|-----------|--------|-----------|------------|
| 7.1 | Pipeline Jenkins/GitHub Actions | ⚠️ | Não implementado | Sugestão: GitHub Actions com deploy automático |

**Sugestão de implementação:**
- GitHub Actions para build automático
- Push para DockerHub em cada commit
- Deploy automático no cluster Kubernetes

---

## 📚 **Documentação Adicional**

- [`docs/GUIA_PROFESSOR.md`](docs/GUIA_PROFESSOR.md) - Guia completo para avaliação com rubrica detalhada
- [`docs/ESTUDO_KUBERNETES.md`](docs/ESTUDO_KUBERNETES.md) - Conceitos teóricos e explicações
- [`docs/DEPLOY_RAPIDO.md`](docs/DEPLOY_RAPIDO.md) - Comandos rápidos de referência

---

## 🛠️ **Comandos Úteis**

```bash
# Ver todos os recursos
kubectl get all -n questionario

# Ver logs em tempo real
kubectl logs -n questionario -l app=backend -f --tail=50

# Reiniciar deployment
kubectl rollout restart deployment backend -n questionario

# Escalar réplicas
kubectl scale deployment backend -n questionario --replicas=6

# Port-forward Prometheus (para debug)
kubectl port-forward -n questionario service/prometheus 9090:9090
# http://localhost:9090/targets

# Ver eventos do namespace
kubectl get events -n questionario --sort-by='.lastTimestamp'

# Abrir dashboard do Minikube
minikube dashboard

# Ver métricas de recursos
kubectl top pods -n questionario
kubectl top nodes

# Remover tudo
kubectl delete namespace questionario
# OU
cd k8s/scripts
.\cleanup.ps1
```

---

## 🐛 **Troubleshooting**

| Problema | Possível Causa | Solução |
|----------|----------------|---------|
| `ImagePullBackOff` | Nome da imagem incorreto | Verificar `image:` no YAML |
| `CrashLoopBackOff` | Container iniciando e morrendo | `kubectl logs -n questionario <pod>` |
| `CreateContainerConfigError` | Secret não encontrado | `kubectl apply -f base/secrets.yaml` |
| Pod em `Pending` | Sem recursos no cluster | `kubectl describe pod <pod> -n questionario` |
| RabbitMQ demora a subir | Normal! Inicialização lenta | Aguardar até 3 minutos |
| Grafana sem dados | Prometheus não está coletando | Port-forward Prometheus e ver `/targets` |
| MongoDB sem espaço | PVC cheio | `kubectl get pvc -n questionario` |

---

## 📝 **Resumo das Entregas**

### **O que foi feito:**

✅ **Docker:**
- Dockerfile para backend (.NET 8)
- Dockerfile para frontend (Angular)
- Imagens publicadas no Docker Hub
- Docker Compose para desenvolvimento local

✅ **Kubernetes:**
- Deployment com 4 réplicas (alta disponibilidade)
- NodePort para exposição externa (Backend, Frontend, Grafana)
- ClusterIP para serviços internos (MongoDB, Prometheus)
- Readiness e Liveness Probes configurados

✅ **Persistência:**
- PVC de 1GB para MongoDB
- PVC de 500MB para RabbitMQ
- PVC de 2GB para Prometheus

✅ **Monitoramento:**
- Prometheus coletando métricas
- Grafana com dashboards
- Queries prontas para visualização

✅ **Testes:**
- Script de stress test automatizado
- Documentação para captura de evidências

---

## 👨‍💻 **Autor**

**Lucas Esteves**  
📧 Email: [lucas@example.com]  
🐙 GitHub: [@LucasEsteves2](https://github.com/LucasEsteves2)  
🐳 Docker Hub: [luqui25](https://hub.docker.com/u/luqui25)

---

## 📄 **Licença**

Este projeto foi desenvolvido para fins acadêmicos como parte do Projeto de Disciplina do curso de Infraestrutura e Deployment com Kubernetes.

---

**🎓 Instituto Infnet - 2025**
