# 🚀 Kubernetes - Sistema de Questionários Online

> **Aluno:** Lucas Esteves  
> **Disciplina:** Infraestrutura e Deployment com Kubernetes  
> **Repositório:** https://github.com/LucasEsteves2/ApiQuestionario_InfNet  
> **Docker Hub:** https://hub.docker.com/u/luqui25

---

## 📋 Visão Geral

Sistema de questionários online com:
- **Backend:** .NET 8 API (4 réplicas)
- **Frontend:** Angular 18
- **Banco de Dados:** MongoDB 7
- **Message Broker:** RabbitMQ 3.13
- **Monitoramento:** Prometheus + Grafana

---

## 📁 Estrutura do Projeto

```
k8s/
├── kustomization.yaml           # Orquestrador principal
│
├── base/
│   ├── namespace.yaml           # Namespace "questionario"
│   ├── secrets.yaml             # Credenciais
│   │
│   ├── database/
│   │   ├── mongodb.yaml         # MongoDB + PVC (1GB)
│   │   └── rabbitmq.yaml        # RabbitMQ + PVC (500MB)
│   │
│   ├── application/
│   │   ├── backend.yaml         # API .NET (4 réplicas)
│   │   └── frontend.yaml        # Angular
│   │
│   └── monitoring/
│       ├── prometheus.yaml      # Métricas + PVC (2GB)
│       ├── grafana.yaml         # Dashboards
│       └── kube-state-metrics.yaml
│
└── scripts/
    ├── stress-test.ps1          # Teste de carga
    └── cleanup.ps1              # Limpeza
```

---

## 🚀 Como Executar

### 1️⃣ Pré-requisitos

```bash
# Instalar ferramentas
choco install minikube kubectl docker-desktop

# Iniciar Minikube
minikube start

# Verificar
minikube status
```

### 2️⃣ Deploy

```bash
# Na pasta k8s
cd k8s

# Aplicar todos os recursos
kubectl apply -k .

# Aguardar pods ficarem prontos (~5 min)
kubectl wait --for=condition=ready pod --all -n questionario --timeout=300s
```

### 3️⃣ Verificar

```bash
kubectl get pods -n questionario
kubectl get services -n questionario
kubectl get pvc -n questionario
```

**Saída esperada:**
```
NAME                           READY   STATUS    AGE
backend-xxxxx-aaaaa            1/1     Running   2m
backend-xxxxx-bbbbb            1/1     Running   2m
backend-xxxxx-ccccc            1/1     Running   2m
backend-xxxxx-ddddd            1/1     Running   2m  ← 4 réplicas
frontend-xxxxx-aaaaa           1/1     Running   2m
mongodb-xxxxx-aaaaa            1/1     Running   3m
rabbitmq-xxxxx-aaaaa           1/1     Running   3m
prometheus-xxxxx-aaaaa         1/1     Running   1m
grafana-xxxxx-aaaaa            1/1     Running   1m
kube-state-metrics-xxxxx       1/1     Running   1m
```

---

## 🌐 Como Rodar Frontend + Backend

### **📍 Por que preciso de Port-Forward?**

O Minikube cria um cluster Kubernetes dentro de uma **VM isolada**. Os serviços com `NodePort` ficam acessíveis **dentro da VM**, mas não diretamente no `localhost` do seu PC.

**Problema:**
- ✅ Frontend roda em: `http://192.168.49.2:30080` (IP da VM do Minikube)
- ❌ Frontend tenta chamar backend em: `http://localhost:5000` (não existe!)

**Solução:** Fazer **port-forward** do backend para o `localhost` do seu PC.

---

### **🔧 Passo a Passo**


#### **1. Fazer Port-Forward do Backend**

Abra um **novo terminal PowerShell** e deixe rodando:

```powershell
kubectl port-forward -n questionario service/backend 5000:5000
```

**O que isso faz:**
- Cria um "túnel" do `localhost:5000` do seu PC → `backend:5000` dentro do Kubernetes
- Agora o frontend consegue chamar `http://localhost:5000/api/questionario`

**⚠️ Deixe esse terminal aberto!** Se fechar, o port-forward para.

---

#### **. Acessar o Frontend**

Obtenha a Url do frontend:

```bash
minikube service frontend -n questionario

```

**Agora o frontend consegue se comunicar com o backend!**

---

### **📸 Evidência de Funcionamento**


---

### **🎯 Outros Serviços (Opcional)**

```bash
# Obter URLs
minikube service backend -n questionario --url    # Backend API
minikube service frontend -n questionario --url   # Interface web
minikube service grafana -n questionario --url    # Dashboards
minikube service rabbitmq -n questionario --url   # RabbitMQ UI
```

**Credenciais:**
- **Grafana:** admin / admin123
- **RabbitMQ:** admin / admin123

---

## 📊 Grafana - Dashboards Pré-configurados

O Grafana já vem com dashboard automático. Acesse e veja:

```bash
minikube service grafana -n questionario

```

**Dashboard:** "Questionario - Visão Geral do Kubernetes"

**Métricas disponíveis:**
- ✅ Backend Ativo (quantos pods rodando)
- ✅ Frontend Ativo
- ✅ CPU Usage por Pod
- ✅ Memory Usage por Pod
- ✅ Network I/O
- ✅ Pods Running/Pending/Failed
- ✅ HTTP Requests (se backend expor métricas)

**Refresh automático:** 10 segundos

---

## 🔥 Stress Test

```powershell
# Executar teste de carga
cd k8s/scripts
.\stress-test.ps1

# Personalizar
.\stress-test.ps1 -Requests 5000 -Concurrent 100
```

**Durante o teste, observe no Grafana:**
- 📈 CPU aumentando
- 💾 Memória subindo
- 🌐 Pico de requisições

---

## ✅ Checklist - Requisitos da Prova

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| **1** | **Docker + Docker Hub** | | |
| 1.1 | Imagem Docker criada | ✅ | `Back/QuestionarioOnline/Dockerfile` |
| 1.2 | Publicada no Docker Hub | ✅ | https://hub.docker.com/r/luqui25/lucas-fluminense-backend |
| **2** | **Kubernetes** | | |
| 2.1 | Deployment com 4 réplicas | ✅ | `base/application/backend.yaml` (linha 9) |
| 2.2 | NodePort (acesso externo) | ✅ | Backend porta 30500, Grafana porta 30300 |
| 2.3 | Banco com ClusterIP | ✅ | MongoDB (27017) e RabbitMQ (5672) internos |
| 2.4 | Readiness Probe | ✅ | Backend, MongoDB, RabbitMQ, Prometheus, Grafana |
| 2.5 | Liveness Probe | ✅ | Todos os deployments |
| **3** | **Monitoramento** | | |
| 3.1 | Prometheus instalado | ✅ | Coletando métricas do cluster |
| 3.2 | Grafana instalado | ✅ | Dashboard pré-configurado |
| 3.3 | Apenas Grafana externo | ✅ | Prometheus é ClusterIP (interno) |
| 3.4 | PVC para Prometheus | ✅ | 2GB persistente |
| 3.5 | Dashboards criados | ✅ | CPU, Memória, Pods, Network |
| **4** | **Persistência** | | |
| 4.1 | PVC MongoDB | ✅ | 1GB |
| 4.2 | PVC RabbitMQ | ✅ | 500MB |
| 4.3 | PVC Prometheus | ✅ | 2GB |
| **5** | **CI/CD** | | |
| 5.1 | Pipeline criado | ✅ | GitHub Actions (`.github/workflows/deploy.yml`) |
| 5.2 | Build + Push automático | ✅ | Toda alteração na branch main |
| **6** | **Stress Test** | | |
| 6.1 | Script criado | ✅ | `scripts/stress-test.ps1` |
| 6.2 | Prints do Grafana | ✅ | Antes, durante e depois |

**Verificar requisitos:**
```bash
# 1. Ver 4 réplicas do backend
kubectl get deployment backend -n questionario

# 2. Ver services (NodePort vs ClusterIP)
kubectl get services -n questionario

# 3. Ver PVCs
kubectl get pvc -n questionario

# 4. Ver probes
kubectl describe deployment backend -n questionario | grep -A 5 Probe
```

---

## 📚 Recursos

## 📚 Recursos

| Tipo | Descrição | Link/Localização |
|------|-----------|------------------|
| 📖 Documentação | Guia completo para o professor | `docs/GUIA_PROFESSOR.md` |
| 📖 Documentação | Conceitos de Kubernetes | `docs/ESTUDO_KUBERNETES.md` |
| 🐳 Docker Hub | Imagens publicadas | https://hub.docker.com/u/luqui25 |
| 🐙 GitHub | Código fonte | https://github.com/LucasEsteves2/ApiQuestionario_InfNet |

---

## 🛠️ Comandos Úteis

```bash
# Ver tudo de uma vez
kubectl get all -n questionario

# Ver logs em tempo real
kubectl logs -n questionario -l app=backend -f --tail=50

# Reiniciar deployment
kubectl rollout restart deployment backend -n questionario

# Escalar réplicas
kubectl scale deployment backend -n questionario --replicas=6

# Port-forward (debug)
kubectl port-forward -n questionario svc/prometheus 9090:9090
kubectl port-forward -n questionario svc/mongodb 27017:27017

# Ver eventos
kubectl get events -n questionario --sort-by='.lastTimestamp'

# Ver métricas de recursos
kubectl top pods -n questionario
kubectl top nodes

# Dashboard do Minikube
minikube dashboard

# Remover tudo
kubectl delete namespace questionario
```

---

## 🐛 Troubleshooting

| Problema | Causa | Solução |
|----------|-------|---------|
| `ImagePullBackOff` | Nome da imagem incorreto | Verificar `image:` no YAML |
| `CrashLoopBackOff` | Container morrendo | `kubectl logs <pod> -n questionario` |
| `CreateContainerConfigError` | Secret não encontrado | `kubectl get secrets -n questionario` |
| Pod em `Pending` | Sem recursos | `kubectl describe pod <pod> -n questionario` |
| RabbitMQ demora | Normal (2-3 min) | Aguardar |
| Grafana sem dados | Prometheus não coletando | Port-forward Prometheus → `/targets` |

---

## 📖 O que cada componente faz

### Backend (.NET 8 API)
- API REST para gerenciar questionários
- 4 réplicas para alta disponibilidade
- Readiness/Liveness probes configurados
- Expõe métricas para o Prometheus

### Frontend (Angular)
- Interface web do usuário
- Consome API do backend
- Servido via Nginx

### MongoDB
- Banco de dados NoSQL
- Armazena questionários e respostas
- PVC de 1GB para persistência
- ClusterIP (apenas interno)

### RabbitMQ
- Message broker para filas
- Processamento assíncrono
- PVC de 500MB
- Interface de gerenciamento na porta 31567

### Prometheus
- Coleta métricas do cluster
- Scrape automático de pods
- PVC de 2GB para dados históricos
- ClusterIP (apenas interno)

### Grafana
- Visualização de métricas
- Dashboard pré-configurado
- Refresh automático a cada 10s
- NodePort porta 30300 (acesso externo)

### kube-state-metrics
- Exporta métricas do Kubernetes
- Informações sobre pods, deployments, nodes
- Usado pelo Prometheus

---

## 👨‍💻 Autor

**Lucas Esteves**  
🐙 GitHub: [@LucasEsteves2](https://github.com/LucasEsteves2)  
🐳 Docker Hub: [luqui25](https://hub.docker.com/u/luqui25)

---

**🎓 Instituto Infnet - 2025**
