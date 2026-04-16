# ?? **Kubernetes Deployment - Projeto de Disciplina**

> **Aluno:** Lucas Esteves  
> **Disciplina:** Infraestrutura e Deployment com Kubernetes  
> **Repositório:** [ApiQuestionario_InfNet](https://github.com/LucasEsteves2/ApiQuestionario_InfNet)  
> **Docker Hub:** [luqui25](https://hub.docker.com/u/luqui25)

---

## ?? **Visão Geral do Projeto**

Sistema de **Questionário Online** construído com:
- **Backend:** .NET 8 Web API (4 réplicas para alta disponibilidade)
- **Frontend:** Angular 18 com Nginx
- **Banco de Dados:** MongoDB 7 com persistência
- **Message Broker:** RabbitMQ 3.13 com management UI
- **Monitoramento:** Prometheus + Grafana

**Objetivo:** Demonstrar conhecimentos em Docker, Kubernetes, Monitoramento e CI/CD.

---

## ??? **Arquitetura do Sistema**

```
??????????????????????????????????????????????????????????????????
?                    KUBERNETES CLUSTER                          ?
?                                                                ?
?  ???????????????????????????????????????????????????????????? ?
?  ?  NAMESPACE: questionario                                 ? ?
?  ?                                                          ? ?
?  ?  ???????????????     ????????????????                  ? ?
?  ?  ?  Frontend   ???????   Backend    ?                  ? ?
?  ?  ?  (Angular)  ?     ?   (.NET 8)   ?                  ? ?
?  ?  ?  NodePort   ?     ?  4 Réplicas  ?                  ? ?
?  ?  ?  :30080     ?     ?  NodePort    ?                  ? ?
?  ?  ???????????????     ?  :30500      ?                  ? ?
?  ?                       ????????????????                  ? ?
?  ?                              ?                          ? ?
?  ?         ???????????????????????????????????????        ? ?
?  ?         ?                    ?                ?        ? ?
?  ?  ???????????????    ????????????????  ????????????   ? ?
?  ?  ?  MongoDB    ?    ?   RabbitMQ   ?  ?Prometheus?   ? ?
?  ?  ?  ClusterIP  ?    ?  ClusterIP   ?  ?ClusterIP ?   ? ?
?  ?  ?  PVC: 1GB   ?    ?  PVC: 500MB  ?  ?PVC: 2GB  ?   ? ?
?  ?  ???????????????    ????????????????  ????????????   ? ?
?  ?                                              ?         ? ?
?  ?                       ????????????????????????         ? ?
?  ?                       ?                                ? ?
?  ?                ???????????????                         ? ?
?  ?                ?   Grafana   ?                         ? ?
?  ?                ?  NodePort   ?                         ? ?
?  ?                ?  :30300     ?                         ? ?
?  ?                ???????????????                         ? ?
?  ???????????????????????????????????????????????????????? ?
??????????????????????????????????????????????????????????????

?? ACESSOS EXTERNOS:
   ? Frontend:   http://<minikube-ip>:30080
   ? Backend:    http://<minikube-ip>:30500
   ? Grafana:    http://<minikube-ip>:30300
   ? RabbitMQ:   http://<minikube-ip>:31567
```

---

## ?? **Estrutura do Projeto**

```
k8s/
?
??? ?? base/                         # ?? Arquivos separados por categoria
?   ??? namespace.yaml               # Namespace "questionario"
?   ??? secrets.yaml                 # Senhas (MongoDB, RabbitMQ)
?   ?
?   ??? ?? database/
?   ?   ??? mongodb.yaml             # MongoDB + PVC (1GB)
?   ?   ??? rabbitmq.yaml            # RabbitMQ + PVC (500MB)
?   ?
?   ??? ?? application/
?   ?   ??? backend.yaml             # .NET 8 API (4 réplicas)
?   ?   ??? frontend.yaml            # Angular + Nginx
?   ?
?   ??? ?? monitoring/
?       ??? prometheus.yaml          # Servidor de métricas + PVC (2GB)
?       ??? grafana.yaml             # Dashboards + NodePort
?       ??? kube-state-metrics.yaml  # Métricas do Kubernetes
?
??? ?? kustomization.yaml            # Orquestrador de recursos
?
??? ?? scripts/                      # ?? Automação
?   ??? stress-test.ps1              # Testes de carga
?   ??? cleanup.ps1                  # Limpeza do cluster
?
??? ?? docs/                         # ?? Documentação adicional
    ??? GUIA_PROFESSOR.md            # Guia de avaliação detalhado
    ??? ESTUDO_KUBERNETES.md         # Conceitos teóricos
```

---

## ? **Pré-requisitos para Execução**

### **Ferramentas Necessárias**

```sh
# 1. Minikube (cluster Kubernetes local)
minikube version
# Se não tiver instalado: choco install minikube

# 2. kubectl (CLI do Kubernetes)
kubectl version --client
# Se não tiver instalado: choco install kubernetes-cli

# 3. Docker Desktop (rodando)
docker --version
# Se não tiver instalado: choco install docker-desktop
```

### **Verificar Instalação**

```sh
# Verificar se o Docker está rodando
docker ps

# Iniciar o Minikube
minikube start

# Verificar status do cluster
minikube status
```

**? Saída esperada:**
```
minikube
type: Control Plane
host: Running
kubelet: Running
apiserver: Running
kubeconfig: Configured
```

---

## ?? **Como Executar o Projeto (Passo a Passo)**

### **?? 1. Clonar o Repositório**

```sh
# Clone o repositório do GitHub
git clone https://github.com/LucasEsteves2/ApiQuestionario_InfNet.git
cd ApiQuestionario_InfNet
```

---

### **?? 2. Navegar para a Pasta Kubernetes**

```sh
cd k8s
```

---

### **?? 3. Deploy da Aplicação no Kubernetes**

```sh
# Aplica TODOS os recursos de uma vez usando Kustomize
kubectl apply -k .
```

**? O que foi criado:**
- ?? Namespace `questionario`
- ?? Secrets com senhas (MongoDB, RabbitMQ)
- ?? MongoDB com PVC (1GB)
- ?? RabbitMQ com PVC (500MB)
- ?? Backend com 4 réplicas
- ?? Frontend Angular
- ?? Prometheus com PVC (2GB) + RBAC
- ?? Grafana com dashboards pré-configurados
- ?? kube-state-metrics (métricas do cluster)

---

### **? 4. Aguardar Todos os Pods Ficarem Prontos**

```sh
# Aguarda até 5 minutos para tudo subir
kubectl wait --for=condition=ready pod --all -n questionario --timeout=300s
```

**?? Nota:** MongoDB e RabbitMQ podem demorar até 3 minutos para iniciar (é normal!).

---

### **?? 5. Verificar Status dos Recursos**

```sh
# Ver todos os pods
kubectl get pods -n questionario

# Ver todos os services
kubectl get services -n questionario

# Ver todos os PVCs (volumes persistentes)
kubectl get pvc -n questionario
```

**? Saída esperada dos PODS:**
```
NAME                            READY   STATUS    RESTARTS   AGE
backend-xxxxx-aaaaa             1/1     Running   0          2m
backend-xxxxx-bbbbb             1/1     Running   0          2m
backend-xxxxx-ccccc             1/1     Running   0          2m
backend-xxxxx-ddddd             1/1     Running   0          2m  ? 4 réplicas!
frontend-xxxxx-aaaaa            1/1     Running   0          2m
mongodb-xxxxx-aaaaa             1/1     Running   0          3m
rabbitmq-xxxxx-aaaaa            1/1     Running   0          3m
prometheus-xxxxx-aaaaa          1/1     Running   0          1m
grafana-xxxxx-aaaaa             1/1     Running   0          1m
kube-state-metrics-xxxxx-aaaaa  1/1     Running   0          1m
```

**? Saída esperada dos SERVICES:**
```
NAME         TYPE        CLUSTER-IP      EXTERNAL-IP   PORT(S)
backend      NodePort    10.96.xxx.xxx   <none>        5000:30500/TCP
frontend     NodePort    10.96.xxx.xxx   <none>        80:30080/TCP
grafana      NodePort    10.96.xxx.xxx   <none>        3000:30300/TCP
rabbitmq     NodePort    10.96.xxx.xxx   <none>        15672:31567/TCP
mongodb      ClusterIP   10.96.xxx.xxx   <none>        27017/TCP  ? Interno!
prometheus   ClusterIP   10.96.xxx.xxx   <none>        9090/TCP   ? Interno!
```

**? Saída esperada dos PVCs:**
```
NAME             STATUS   VOLUME                  CAPACITY   ACCESS MODES
mongodb-pvc      Bound    pvc-xxxxx-xxxxx-xxxxx   1Gi        RWO
prometheus-pvc   Bound    pvc-yyyyy-yyyyy-yyyyy   2Gi        RWO
rabbitmq-pvc     Bound    pvc-zzzzz-zzzzz-zzzzz   500Mi      RWO
```

---

## ?? **Como Acessar a Aplicação**

### **Obter URLs dos Serviços**

```sh
# Frontend (Angular)
minikube service frontend -n questionario --url
# Exemplo de saída: http://192.168.49.2:30080

# Backend API (.NET 8)
minikube service backend -n questionario --url
# Exemplo de saída: http://192.168.49.2:30500

# Grafana (Dashboards de Monitoramento)
minikube service grafana -n questionario --url
# Exemplo de saída: http://192.168.49.2:30300

# RabbitMQ (Interface de Gerenciamento)
minikube service rabbitmq -n questionario --url
# Exemplo de saída: http://192.168.49.2:31567
```

### **Credenciais de Acesso**

| Serviço | Usuário | Senha | Porta |
|---------|---------|-------|-------|
| ?? **Grafana** | `admin` | `admin123` | `:30300` |
| ?? **RabbitMQ** | `admin` | `admin123` | `:31567` |

---

### **?? IMPORTANTE: Port-Forward (Windows/Docker)**

Se você estiver usando **Minikube com Docker no Windows**, pode ser necessário usar **port-forward** para acessar os serviços:

```sh
# Port-forward do Backend
kubectl port-forward -n questionario service/backend 5000:5000

# Port-forward do Grafana
kubectl port-forward -n questionario service/grafana 3000:3000
```

Depois acesse:
- Backend: `http://localhost:5000/api/questionario`
- Grafana: `http://localhost:3000`

---

## ?? **Configurar Dashboards no Grafana**

### **1. Acessar o Grafana**

```sh
# Pegar URL do Grafana
minikube service grafana -n questionario --url

# Acesse no navegador
# Login: admin / admin123
```

---

### **2. Criar Dashboard de Kubernetes**

O Grafana já vem com o Prometheus configurado como datasource. Agora é só criar dashboards.

#### **?? Dashboard 1: CPU Usage por Pod**

1. No Grafana, clique em **"+"** ? **"Create Dashboard"** ? **"Add Visualization"**
2. Selecione **"Prometheus"** como datasource
3. Cole a query abaixo:

```promql
sum(rate(container_cpu_usage_seconds_total{namespace="questionario"}[5m])) by (pod)
```

4. Em **"Title"** coloque: `CPU Usage por Pod`
5. Em **"Visualization"** escolha: `Time series`
6. Clique em **"Apply"**

---

#### **?? Dashboard 2: Memory Usage por Pod**

1. Adicione nova visualização
2. Query:

```promql
sum(container_memory_working_set_bytes{namespace="questionario"}) by (pod)
```

3. Title: `Memory Usage por Pod`
4. **Unit**: `bytes(IEC)` (no painel direito ? Standard options ? Unit)

---

#### **?? Dashboard 3: HTTP Requests Total**

1. Adicione nova visualização
2. Query:

```promql
sum(rate(http_requests_total[5m])) by (method, status)
```

3. Title: `HTTP Requests Rate`

---

#### **?? Dashboard 4: Request Latency (p95)**

1. Adicione nova visualização
2. Query:

```promql
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

3. Title: `Request Latency (p95)`
4. Unit: `seconds (s)`

---

#### **?? Dashboard 5: Número de Pods Running**

1. Adicione nova visualização
2. Query:

```promql
count(kube_pod_status_phase{namespace="questionario", phase="Running"})
```

3. Title: `Pods Running`
4. Visualization: `Stat`

---

### **3. Salvar o Dashboard**

1. Clique no ícone de **disquete** (??) no topo
2. Nome: `Kubernetes - Questionário Online`
3. Clique em **"Save"**

---

## ?? **Executar Stress Test**

### **Usando o Script PowerShell (Recomendado)**

```powershell
# Navegar para a pasta de scripts
cd k8s/scripts

# Executar o stress test
.\stress-test.ps1

# OU executar com parâmetros customizados
.\stress-test.ps1 -Requests 5000 -Concurrent 100
```

**? Saída esperada:**
```
??????????????????????????????????????????????????????????????
?? STRESS TEST - Questionário Online
??????????????????????????????????????????????????????????????

?? Target: http://192.168.49.2:30500/api/questionario
?? Requisições: 1000
? Concorrentes: 50

?? Iniciando stress test...

? TESTE FINALIZADO!
   Total de requisições: 1000
   Bem-sucedidas: 998
   Erros: 2
   Duração: 12.5s
   Requisições/segundo: 80
```

---

### **Manualmente (Bash - Linux/Mac)**

```sh
# Pegar URL do backend
url=$(minikube service backend -n questionario --url)

# Enviar 1000 requisições (50 paralelas)
seq 1 1000 | xargs -P50 -I{} curl -s "$url/api/questionario" > /dev/null
```

---

### **?? Capturar Evidências no Grafana**

**Passo a passo:**

1. **ANTES do teste:**
   - Acesse o Grafana
   - Tire print do dashboard mostrando CPU/Memória normais
   - Salve como: `grafana-antes-stress.png`

2. **EXECUTE o stress test:**
   ```powershell
   .\stress-test.ps1
   ```

3. **DURANTE o teste (enquanto roda):**
   - Volte ao Grafana
   - Tire prints mostrando:
     - ?? CPU subindo
     - ?? Memória aumentando
     - ?? Pico de requisições HTTP
   - Salve como:
     - `grafana-durante-stress-cpu.png`
     - `grafana-durante-stress-memoria.png`
     - `grafana-durante-stress-requests.png`

4. **DEPOIS do teste:**
   - Aguarde 2 minutos
   - Tire print mostrando a aplicação voltando ao normal
   - Salve como: `grafana-apos-stress.png`

---

## ? **CHECKLIST - Requisitos da Prova**

### **?? 1. Imagem Docker + Docker Hub**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 1.1 | Criar imagem Docker da aplicação | ? | Dockerfile criado |
| 1.2 | Publicar imagem no Docker Hub | ? | `luqui25/lucas-fluminense-backend:latest` |

**?? Localização:**
- Backend: `Back/QuestionarioOnline/Dockerfile`
- Frontend: `Front/Dockerfile`

**?? Docker Hub:**
- Backend: https://hub.docker.com/r/luqui25/lucas-fluminense-backend
- Frontend: https://hub.docker.com/r/luqui25/lucas-fluminense-frontend

**? Como verificar:**
```sh
docker pull luqui25/lucas-fluminense-backend:latest
docker pull luqui25/lucas-fluminense-frontend:latest
```

---

### **?? 2. Deployment com 4 Réplicas**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 2.1 | Deployment com 4 réplicas | ? | Backend com 4 instâncias |

**?? Localização:** `k8s/base/application/backend.yaml` (linha 9)

**? Configuração:**
```yaml
spec:
  replicas: 4  # ? 4 réplicas para alta disponibilidade
```

**? Como verificar:**
```sh
kubectl get deployment backend -n questionario

# Saída esperada:
# NAME      READY   UP-TO-DATE   AVAILABLE   AGE
# backend   4/4     4            4           5m
```

---

### **?? 3. NodePort - Aplicação Acessível Externamente**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 3.1 | Backend exposto via NodePort | ? | Porta 30500 |
| 3.2 | Grafana exposto via NodePort | ? | Porta 30300 |

**?? Localização:** `k8s/base/application/backend.yaml` (linhas 67-77)

**? Configuração:**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: backend
spec:
  type: NodePort  # ? Acessível externamente
  ports:
    - port: 5000
      targetPort: 8080
      nodePort: 30500  # ? Porta fixa
```

**? Como verificar:**
```sh
kubectl get service backend -n questionario

# Saída esperada:
# NAME      TYPE       CLUSTER-IP     EXTERNAL-IP   PORT(S)
# backend   NodePort   10.96.xxx.xxx  <none>        5000:30500/TCP
```

**? Como acessar:**
```sh
minikube service backend -n questionario --url
# http://192.168.49.2:30500/api/questionario
```

---

### **?? 4. Banco de Dados com ClusterIP**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 4.1 | MongoDB como POD | ? | Deployment criado |
| 4.2 | MongoDB com ClusterIP | ? | Não acessível externamente |
| 4.3 | RabbitMQ como POD adicional | ? | Service ClusterIP criado |

**?? Localização:** `k8s/base/database/mongodb.yaml` (linhas 80-88)

**? Configuração:**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: mongodb
spec:
  selector:
    app: mongodb
  ports:
    - port: 27017
      targetPort: 27017
  type: ClusterIP  # ? Apenas interno!
```

**? Como verificar:**
```sh
kubectl get service mongodb -n questionario

# Saída esperada:
# NAME      TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)
# mongodb   ClusterIP   10.96.xxx.xxx  <none>        27017/TCP
```

**?? MongoDB é acessível APENAS internamente:**
- ? Backend consegue acessar: `mongodb:27017`
- ? Você NÃO consegue acessar de fora do cluster

---

### **?? 5. Probes - Readiness e Liveness**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 5.1 | Readiness Probe configurado | ? | Backend + MongoDB + RabbitMQ + Prometheus + Grafana |
| 5.2 | Liveness Probe configurado | ? | Backend + MongoDB + RabbitMQ + Prometheus + Grafana |

**?? Localização:** `k8s/base/application/backend.yaml` (linhas 51-66)

**? Configuração (Backend):**
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

**? Como verificar:**
```sh
kubectl describe deployment backend -n questionario | grep -A 5 "Liveness\|Readiness"
```

**?? O que fazem:**
- **Readiness:** Kubernetes só envia tráfego quando o pod está pronto
- **Liveness:** Kubernetes reinicia o pod se ele travar

---

### **?? 6. Prometheus + Grafana**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 6.1 | Prometheus instalado | ? | Coletando métricas |
| 6.2 | Grafana instalado | ? | Dashboards configurados |
| 6.3 | Prometheus fazendo scrape | ? | Targets configurados |
| 6.4 | Dashboards criados | ? | CPU, Memória, HTTP, Pods |

**?? Localização:**
- Prometheus: `k8s/base/monitoring/prometheus.yaml`
- Grafana: `k8s/base/monitoring/grafana.yaml`

**? Configuração do Prometheus (scrape):**
```yaml
scrape_configs:
  - job_name: 'backend'
    metrics_path: '/metrics'
    static_configs:
      - targets: ['backend:5000']

  - job_name: 'kube-state-metrics'
    static_configs:
      - targets: ['kube-state-metrics.kube-system.svc.cluster.local:8080']
```

**? Como verificar:**
```sh
# Port-forward do Prometheus
kubectl port-forward -n questionario svc/prometheus 9090:9090

# Acesse no navegador:
# http://localhost:9090/targets

# Todos os targets devem estar "UP"
```

---

### **?? 7. Apenas Grafana Acessível Externamente**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 7.1 | Prometheus interno (ClusterIP) | ? | Não acessível de fora |
| 7.2 | Grafana externo (NodePort) | ? | Acessível via :30300 |

**? Configuração:**

**Prometheus (INTERNO):**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: prometheus
spec:
  type: ClusterIP  # ? Apenas interno
  ports:
    - port: 9090
```

**Grafana (EXTERNO):**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: grafana
spec:
  type: NodePort  # ? Acessível externamente
  ports:
    - port: 3000
      targetPort: 3000
      nodePort: 30300
```

**? Como verificar:**
```sh
kubectl get services -n questionario

# Saída esperada:
# prometheus   ClusterIP   10.96.xxx.xxx  <none>        9090/TCP        ? INTERNO
# grafana      NodePort    10.96.xxx.xxx  <none>        3000:30300/TCP  ? EXTERNO
```

---

### **?? 8. PVC para Prometheus (Persistência)**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 8.1 | PVC criado para Prometheus | ? | 2GB de armazenamento |
| 8.2 | Volume montado no container | ? | `/prometheus` |

**?? Localização:** `k8s/base/monitoring/prometheus.yaml` (linhas 59-68)

**? Configuração:**
```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: prometheus-pvc
  namespace: questionario
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 2Gi  # ? 2GB persistente
```

**? Volume montado:**
```yaml
volumeMounts:
  - name: prometheus-storage
    mountPath: /prometheus  # ? Dados salvos aqui

volumes:
  - name: prometheus-storage
    persistentVolumeClaim:
      claimName: prometheus-pvc
```

**? Como verificar:**
```sh
kubectl get pvc prometheus-pvc -n questionario

# Saída esperada:
# NAME             STATUS   VOLUME                  CAPACITY
# prometheus-pvc   Bound    pvc-xxxxx-xxxxx-xxxxx   2Gi
```

**?? Por que isso é importante:**
- ? **SEM PVC:** Se o pod do Prometheus morrer, **perde todos os dados**
- ? **COM PVC:** Os dados ficam salvos no volume persistente

---

### **?? 9. Dashboards no Grafana**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 9.1 | Dashboard de CPU | ? | Query criada |
| 9.2 | Dashboard de Memória | ? | Query criada |
| 9.3 | Dashboard de HTTP Requests | ? | Query criada |
| 9.4 | Dashboard de Pods | ? | Query criada |

**? Queries criadas (ver seção "Configurar Dashboards no Grafana" acima):**
- CPU por Pod
- Memória por Pod
- HTTP Requests Total
- Request Latency (p95)
- Pods Running

**?? Evidências:**
- Prints dos dashboards anexados ao trabalho
- Mostrando métricas ANTES, DURANTE e DEPOIS do stress test

---

### **?? 10. Pipeline CI/CD**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 10.1 | Pipeline GitHub Actions | ? | `.github/workflows/deploy.yml` |
| 10.2 | Build automático | ? | Testa + Compila |
| 10.3 | Push para Docker Hub | ? | Imagens publicadas automaticamente |

**?? Localização:** `.github/workflows/deploy.yml`

**? O que o pipeline faz:**
1. **Testa** o código (.NET)
2. **Compila** o backend
3. **Cria** imagens Docker
4. **Publica** no Docker Hub automaticamente
5. (Opcional) Deploy no Kubernetes

**? Como funciona:**
- Todo `git push` na branch `main` ? roda o pipeline
- Testes precisam passar antes do build
- Imagens são tagueadas como `latest`

**?? Ver pipeline:**
- GitHub ? Actions ? CI/CD Pipeline

---

### **?? 11. Stress Test**

| # | Requisito | Status | Evidência |
|---|-----------|--------|-----------|
| 11.1 | Script de stress test criado | ? | `scripts/stress-test.ps1` |
| 11.2 | Dashboards mostram alterações | ? | Prints anexados |

**?? Localização:** `k8s/scripts/stress-test.ps1`

**? Como executar:**
```powershell
cd k8s/scripts
.\stress-test.ps1
```

**?? Evidências necessárias:**
- ? Print do Grafana ANTES do teste
- ? Print do Grafana DURANTE o teste (CPU/Memória subindo)
- ? Print do Grafana DEPOIS do teste (voltando ao normal)

---

## ?? **Resumo de Todos os PVCs**

| PVC | Tamanho | Montado em | Finalidade |
|-----|---------|------------|------------|
| `mongodb-pvc` | 1GB | `/data/db` | Persistir dados do MongoDB |
| `rabbitmq-pvc` | 500MB | `/var/lib/rabbitmq` | Persistir filas do RabbitMQ |
| `prometheus-pvc` | 2GB | `/prometheus` | Persistir métricas históricas |

**? Como verificar TODOS os PVCs:**
```sh
kubectl get pvc -n questionario
```

---

## ??? **Comandos Úteis para Correção**

```sh
# Ver TUDO de uma vez
kubectl get all -n questionario

# Ver logs do backend
kubectl logs -n questionario -l app=backend -f --tail=50

# Ver eventos (caso algo dê errado)
kubectl get events -n questionario --sort-by='.lastTimestamp'

# Reiniciar um deployment
kubectl rollout restart deployment backend -n questionario

# Escalar réplicas manualmente
kubectl scale deployment backend -n questionario --replicas=6

# Ver descrição detalhada de um pod
kubectl describe pod <nome-do-pod> -n questionario

# Ver uso de recursos (CPU/Memória)
kubectl top pods -n questionario
kubectl top nodes

# Port-forward para acessar serviços internos
kubectl port-forward -n questionario svc/prometheus 9090:9090
kubectl port-forward -n questionario svc/mongodb 27017:27017

# Abrir dashboard do Minikube
minikube dashboard
```

---

## ?? **Troubleshooting**

| Problema | Causa Provável | Solução |
|----------|----------------|---------|
| `ImagePullBackOff` | Nome da imagem incorreto | `kubectl describe pod <pod>` ? Verificar nome da imagem |
| `CrashLoopBackOff` | Container iniciando e morrendo | `kubectl logs <pod> -n questionario` |
| `CreateContainerConfigError` | Secret não encontrado | `kubectl get secrets -n questionario` |
| Pod em `Pending` | Sem recursos no cluster | `kubectl describe pod <pod>` |
| RabbitMQ demora a subir | Normal! Leva ~2-3 min | Aguardar |
| Grafana sem dados | Prometheus não coletando | Port-forward Prometheus ? Ver `/targets` |
| MongoDB sem espaço | PVC cheio | `kubectl describe pvc mongodb-pvc` |

---

## ?? **Resumo Final - O que foi Entregue**

### ? **Docker:**
- [x] Dockerfile para backend (.NET 8)
- [x] Dockerfile para frontend (Angular)
- [x] Imagens publicadas no Docker Hub
- [x] Docker Compose para desenvolvimento local

### ? **Kubernetes:**
- [x] Deployment com 4 réplicas (alta disponibilidade)
- [x] NodePort para exposição externa (Backend :30500, Grafana :30300)
- [x] ClusterIP para serviços internos (MongoDB, Prometheus, RabbitMQ)
- [x] Readiness e Liveness Probes em todos os deployments

### ? **Persistência:**
- [x] PVC de 1GB para MongoDB
- [x] PVC de 500MB para RabbitMQ
- [x] PVC de 2GB para Prometheus

### ? **Monitoramento:**
- [x] Prometheus coletando métricas (backend, kubelet, kube-state-metrics)
- [x] Grafana com datasource configurado
- [x] Dashboards criados (CPU, Memória, HTTP, Pods)
- [x] Apenas Grafana acessível externamente

### ? **CI/CD:**
- [x] Pipeline GitHub Actions
- [x] Build automático
- [x] Push para Docker Hub

### ? **Testes:**
- [x] Script de stress test automatizado
- [x] Evidências capturadas (prints antes/durante/depois)

---

## ????? **Informações do Aluno**

**Nome:** Lucas Esteves  
**GitHub:** [@LucasEsteves2](https://github.com/LucasEsteves2)  
**Docker Hub:** [luqui25](https://hub.docker.com/u/luqui25)  
**Repositório:** [ApiQuestionario_InfNet](https://github.com/LucasEsteves2/ApiQuestionario_InfNet)

---

## ?? **Licença**

Este projeto foi desenvolvido para fins acadêmicos como parte do Projeto de Disciplina do curso de **Infraestrutura e Deployment com Kubernetes**.

---

**?? Instituto Infnet - 2025**
