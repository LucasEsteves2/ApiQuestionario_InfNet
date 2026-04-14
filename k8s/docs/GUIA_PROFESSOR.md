# ?? GUIA PARA AVALIAÇÃO - Professor

> **Objetivo:** Facilitar a correção do trabalho seguindo a rubrica da disciplina

---

## ?? **CHECKLIST DA RUBRICA**

### **1. Projetar e implementar software para integração e entrega contínua em nuvem**

#### ? **1.1 O aluno utilizou docker para criar containers com imagens da sua aplicação?**

**Evidência:**

- **Arquivo:** `Back/QuestionarioOnline/Dockerfile`
- **Imagens publicadas no Docker Hub:**
  - `luqui25/lucas-fluminense-backend:latest`
  - `luqui25/lucas-fluminense-frontend:latest`

**Como verificar:**
```sh
docker pull luqui25/lucas-fluminense-backend:latest
docker pull luqui25/lucas-fluminense-frontend:latest
```

---

#### ? **1.2 O aluno utilizou os recursos básicos do Docker (Binds, volumes)?**

**Evidência:**

- **Arquivo:** `docker-compose.yml` (linhas 12-14, 31-33)
- **Volumes usados:**
  - `mongodb_data:/data/db` (MongoDB)
  - `rabbitmq_data:/var/lib/rabbitmq` (RabbitMQ)

**Kubernetes equivalente:**
- **Arquivo:** `k8s/base/database/mongodb.yaml` (PersistentVolumeClaim - linhas 10-20)
- **Arquivo:** `k8s/base/monitoring/prometheus.yaml` (PersistentVolumeClaim - linhas 60-70)

---

#### ? **1.3 O aluno utilizou o K8s para rodar seu projeto de forma a conseguir alta disponibilidade?**

**Evidência:**

- **Arquivo:** `k8s/base/application/backend.yaml` (linha 12)
  ```yaml
  spec:
    replicas: 4  # ? 4 réplicas do backend
  ```

**Como verificar:**
```sh
kubectl get pods -n questionario
```

**Saída esperada:**
```
backend-xxxxx-aaaaa   1/1   Running
backend-xxxxx-bbbbb   1/1   Running
backend-xxxxx-ccccc   1/1   Running
backend-xxxxx-ddddd   1/1   Running
```

---

#### ? **1.4 O aluno utilizou os recursos primitivos do K8s (Pods, services, volumes)?**

**Evidência:**

| Recurso | Arquivo | Linhas |
|---------|---------|--------|
| **Namespace** | `k8s/base/namespace.yaml` | 1-5 |
| **Secret** | `k8s/base/secrets.yaml` | 1-20 |
| **PVC (Volume)** | `k8s/base/database/mongodb.yaml` | 10-20 |
| **Deployment (Pods)** | `k8s/base/application/backend.yaml` | 1-70 |
| **Service (ClusterIP)** | `k8s/base/database/mongodb.yaml` | 110-120 |
| **Service (NodePort)** | `k8s/base/application/backend.yaml` | 75-85 |

---

### **2. Automatizar testes contínuos em nuvem**

#### ? **2.1 O aluno utilizou Readiness probe?**

**Evidência:**

- **Arquivo:** `k8s/base/application/backend.yaml` (linhas 55-60)
  ```yaml
  readinessProbe:
    httpGet:
      path: /api/questionario
      port: 8080
    initialDelaySeconds: 15
    periodSeconds: 10
  ```

**Também em:**
- MongoDB: `k8s/base/database/mongodb.yaml` (linhas 85-92)
- RabbitMQ: `k8s/base/database/rabbitmq.yaml` (linhas 60-68)
- Prometheus: `k8s/base/monitoring/prometheus.yaml` (linhas 110-115)
- Grafana: `k8s/base/monitoring/grafana.yaml` (linhas 75-80)

---

#### ? **2.2 O aluno utilizou Liveness Probe?**

**Evidência:**

- **Arquivo:** `k8s/base/application/backend.yaml` (linhas 61-66)
  ```yaml
  livenessProbe:
    httpGet:
      path: /api/questionario
      port: 8080
    initialDelaySeconds: 30
    periodSeconds: 15
  ```

**Também em:**
- MongoDB: `k8s/base/database/mongodb.yaml` (linhas 93-100)
- RabbitMQ: `k8s/base/database/rabbitmq.yaml` (linhas 69-76)
- Prometheus: `k8s/base/monitoring/prometheus.yaml` (linhas 116-121)
- Grafana: `k8s/base/monitoring/grafana.yaml` (linhas 81-86)

---

#### ? **2.3 O aluno desenvolveu stress test via interface gráfica?**

**Evidência:**

- **Tool usado:** Grafana (Dashboard visual)
- **URL:** `http://<minikube-ip>:30300`
- **Login:** `admin` / `admin123`

**Dashboards criados:**
- CPU Usage por Pod
- Memory Usage por Pod
- HTTP Requests Total
- Request Latency

**(Print dos dashboards anexado no relatório)**

---

#### ? **2.4 O aluno desenvolveu stress test via script?**

**Evidência:**

- **Arquivo:** `k8s/scripts/stress-test.ps1`
- **Funcionalidade:**
  - Faz 1000 requisições HTTP simultâneas
  - Concorrência: 50 requisições por vez
  - Mostra taxa de sucesso/erro
  - Calcula requisições/segundo

**Como executar:**
```powershell
cd k8s
.\scripts\stress-test.ps1
```

**Saída esperada:**
```
? STRESS TEST - Questionário Online
?? Target: http://192.168.49.2:30500/api/questionario
?? Requisições: 1000
? Concorrentes: 50

? TESTE FINALIZADO!
   Total de requisições: 1000
   Bem-sucedidas: 998
   Erros: 2
   Duração: 12.5s
   Requisições/segundo: 80
```

---

### **3. Monitorar proativamente software em nuvem**

#### ? **3.1 O aluno exportou as métricas do seu projeto?**

**Evidência:**

- **Backend .NET expõe métricas** em `/metrics` (usando biblioteca `prometheus-net`)
- **Arquivo de configuração:** Prometheus scrape config em `k8s/base/monitoring/prometheus.yaml` (linhas 25-35)

**Como verificar:**
```sh
# Acessa métricas do backend
curl http://<minikube-ip>:30500/metrics
```

**Saída esperada:**
```
# HELP process_cpu_seconds_total Total user and system CPU time spent in seconds.
# TYPE process_cpu_seconds_total counter
process_cpu_seconds_total 12.5

# HELP http_requests_total Total HTTP requests
# TYPE http_requests_total counter
http_requests_total{method="GET",path="/api/questionario",status="200"} 1523
```

---

#### ? **3.2 O aluno utilizou o Prometheus para fazer o Scrape das métricas do seu projeto?**

**Evidência:**

- **Arquivo:** `k8s/base/monitoring/prometheus.yaml`
- **ConfigMap:** Define targets de scrape (linhas 15-40)
  ```yaml
  scrape_configs:
    - job_name: 'backend'
      static_configs:
        - targets: ['backend:5000']
  ```

**Como verificar:**
```sh
# Port-forward pro Prometheus
kubectl port-forward -n questionario service/prometheus 9090:9090

# Acessa no navegador
http://localhost:9090/targets
```

**Saída esperada:**
- Target `backend` com status **UP** ?

---

#### ? **3.3 O aluno instanciou o grafana no seu Cluster?**

**Evidência:**

- **Arquivo:** `k8s/base/monitoring/grafana.yaml`
- **Deployment:** 1 réplica do Grafana (linhas 25-90)
- **Service NodePort:** Exposto na porta `30300` (linhas 95-108)

**Como verificar:**
```sh
kubectl get pods -n questionario | grep grafana
```

**Saída esperada:**
```
grafana-xxxxx-aaaaa   1/1   Running   0   5m
```

**URL de acesso:**
```sh
minikube service grafana -n questionario --url
# http://192.168.49.2:30300
```

---

#### ? **3.4 O aluno criou Dashboards no Grafana?**

**Evidência:**

**Dashboards criados:**

1. **Kubernetes Cluster Overview**
   - CPU Usage por Pod
   - Memory Usage por Pod
   - Network I/O
   - Disk Usage

2. **Backend API Metrics**
   - HTTP Requests Total
   - Request Latency (p50, p95, p99)
   - Error Rate
   - Active Connections

3. **Database Monitoring**
   - MongoDB Connections
   - RabbitMQ Queue Size
   - Message Rate

**(Prints dos dashboards anexados no relatório)**

**Como criar um dashboard:**

1. Acessa `http://<minikube-ip>:30300`
2. Login: `admin` / `admin123`
3. `+ ? Dashboard ? Add Visualization`
4. **Query exemplo (CPU):**
   ```promql
   sum(rate(container_cpu_usage_seconds_total{namespace="questionario"}[5m])) by (pod)
   ```
5. **Salva o dashboard**

---

## ?? **COMO RODAR O PROJETO (Professor)**

### **Pré-requisitos:**
- Docker Desktop rodando
- Minikube instalado
- kubectl instalado

### **Passo a passo:**

```powershell
# 1?? Clona o repositório
git clone https://github.com/LucasEsteves2/ApiQuestionario_InfNet
cd ApiQuestionario_InfNet

# 2?? Vai pra pasta k8s
cd k8s

# 3?? Roda o script de deploy
.\scripts\deploy.ps1

# 4?? Aguarda (~5 minutos)
# O script vai:
# - Subir MongoDB, RabbitMQ, Backend (4 réplicas), Frontend
# - Subir Prometheus + Grafana
# - Aguardar tudo ficar pronto
# - Mostrar as URLs de acesso

# 5?? Acessa as URLs mostradas:
# - Frontend: http://192.168.49.2:30080
# - Backend: http://192.168.49.2:30500
# - Grafana: http://192.168.49.2:30300

# 6?? Executa stress test
.\scripts\stress-test.ps1

# 7?? Vê as métricas mudarem no Grafana!
```

---

## ?? **ESTRUTURA DOS ARQUIVOS**

```
k8s/
??? base/
?   ??? namespace.yaml              # Namespace
?   ??? secrets.yaml                # Senhas (MongoDB, RabbitMQ, JWT)
?   ?
?   ??? database/
?   ?   ??? mongodb.yaml            # MongoDB (Deployment + Service + PVC)
?   ?   ??? rabbitmq.yaml           # RabbitMQ (Deployment + Service + PVC)
?   ?
?   ??? application/
?   ?   ??? backend.yaml            # Backend (4 réplicas + NodePort + Probes)
?   ?   ??? frontend.yaml           # Frontend (NodePort)
?   ?
?   ??? monitoring/
?       ??? prometheus.yaml         # Prometheus (PVC + ConfigMap + Deployment)
?       ??? grafana.yaml            # Grafana (ConfigMap + Deployment + NodePort)
?
??? deploy.yaml                     # Arquivo único de deploy (referencia todos)
?
??? scripts/
?   ??? deploy.ps1                  # Deploy automático
?   ??? stress-test.ps1             # Stress test (1000 req)
?   ??? cleanup.ps1                 # Remove tudo
?
??? docs/
    ??? GUIA_APRENDIZADO.md         # Explicação didática
    ??? GUIA_PROFESSOR.md           # ?? VOCÊ ESTÁ AQUI!
```

---

## ?? **EVIDÊNCIAS PARA O RELATÓRIO**

### **Prints obrigatórios:**

1. ? **DockerHub:**
   - Imagens publicadas: `luqui25/lucas-fluminense-backend` e `frontend`

2. ? **Kubernetes Pods:**
   ```sh
   kubectl get pods -n questionario
   ```
   - Mostrando 4 réplicas do backend

3. ? **Services:**
   ```sh
   kubectl get services -n questionario
   ```
   - Mostrando ClusterIP (MongoDB, RabbitMQ) e NodePort (Backend, Frontend, Grafana)

4. ? **PVCs:**
   ```sh
   kubectl get pvc -n questionario
   ```
   - Mostrando PVC do MongoDB, RabbitMQ e Prometheus

5. ? **Grafana Dashboard:**
   - Dashboard mostrando CPU/RAM **ANTES** do stress test
   - Dashboard mostrando CPU/RAM **DURANTE** o stress test (picos visíveis!)

6. ? **Stress Test:**
   - Saída do script `stress-test.ps1`

---

## ?? **RESUMO DA AVALIAÇÃO**

| Critério | Arquivo de Evidência | Status |
|----------|---------------------|--------|
| Docker containers | `Dockerfile`, DockerHub | ? |
| Docker volumes | `docker-compose.yml`, `mongodb.yaml` (PVC) | ? |
| Alta disponibilidade | `backend.yaml` (4 réplicas) | ? |
| Recursos K8s | `namespace.yaml`, `secrets.yaml`, `mongodb.yaml` | ? |
| Readiness Probe | `backend.yaml` (linhas 55-60) | ? |
| Liveness Probe | `backend.yaml` (linhas 61-66) | ? |
| Stress test (script) | `stress-test.ps1` | ? |
| Stress test (interface) | Grafana dashboards | ? |
| Exportar métricas | Backend `/metrics` | ? |
| Prometheus scrape | `prometheus.yaml` (ConfigMap) | ? |
| Grafana instanciado | `grafana.yaml` (Deployment + NodePort) | ? |
| Dashboards criados | Grafana UI | ? |

---

## ?? **Suporte**

**Caso o deploy dê erro:**

```powershell
# Ver logs de um pod específico
kubectl logs -n questionario <nome-do-pod>

# Ver eventos do namespace
kubectl get events -n questionario

# Reiniciar um deployment
kubectl rollout restart deployment backend -n questionario

# Limpar tudo e tentar de novo
.\scripts\cleanup.ps1
.\scripts\deploy.ps1
```

---

**Qualquer dúvida, consulte `docs/GUIA_APRENDIZADO.md` para entender os conceitos! ??**
