# ?? Questionário Online — Deploy Kubernetes

> ?? **Projeto de Disciplina — Infraestrutura e Deployment**
> Deploy da aplicação no Kubernetes (Minikube) com Prometheus + Grafana.

---

## ?? Estrutura do Projeto

```
k8s/
?
??? deploy.yaml                  # ? Arquivo único com TODOS os recursos
?                                # Uso: kubectl apply -f deploy.yaml
?
??? base/                        # Arquivos separados por categoria
?   ??? namespace.yaml           # Namespace "questionario"
?   ??? secrets.yaml             # Senhas (MongoDB, RabbitMQ, JWT)
?   ??? database/
?   ?   ??? mongodb.yaml         # MongoDB (PVC 1GB)
?   ?   ??? rabbitmq.yaml        # RabbitMQ (PVC 500MB)
?   ??? application/
?   ?   ??? backend.yaml         # .NET 8 (4 réplicas)
?   ?   ??? frontend.yaml        # Angular + Nginx
?   ??? monitoring/
?       ??? prometheus.yaml      # Coleta métricas (PVC 2GB)
?       ??? grafana.yaml         # Dashboards
?
??? scripts/                     # Automação (opcional)
    ??? deploy.ps1
    ??? stress-test.ps1
    ??? cleanup.ps1
```

---

## ?? Como funciona?

**Service ClusterIP** ? Só acessível dentro do cluster (MongoDB, Prometheus)  
**Service NodePort** ? Acessível do navegador (Backend, Frontend, Grafana)

**Fluxo:**
```
Navegador ? Frontend (:30080) ? Backend (:30500) ? MongoDB
                                      ?
                              Load Balancer (4 pods)
```

---

## ? Pré-requisitos

```bash
minikube version    # Verifica Minikube
kubectl version --client  # Verifica kubectl
minikube start      # Inicia o cluster
```

---

## ?? Deploy da Aplicação

### **Opção 1: Um único comando (RECOMENDADO)**

```bash
cd k8s
kubectl apply -f deploy.yaml

# Aguarda tudo ficar pronto (até 5 min)
kubectl wait --for=condition=ready pod --all -n questionario --timeout=300s
```

**O que faz:**
- Cria namespace, secrets
- Sobe MongoDB e RabbitMQ (aguarda ficarem prontos)
- Sobe Backend (4 réplicas) e Frontend
- Sobe Prometheus e Grafana

---

### **Opção 2: Passo a passo (para aprender)**

```bash
cd k8s

# 1. Infraestrutura
kubectl apply -f base/namespace.yaml
kubectl apply -f base/secrets.yaml

# 2. Bancos de dados
kubectl apply -f base/database/mongodb.yaml
kubectl apply -f base/database/rabbitmq.yaml
kubectl wait --for=condition=ready pod -l app=mongodb -n questionario --timeout=120s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n questionario --timeout=180s

# 3. Aplicação
kubectl apply -f base/application/backend.yaml
kubectl apply -f base/application/frontend.yaml
kubectl wait --for=condition=ready pod -l app=backend -n questionario --timeout=120s

# 4. Monitoramento
kubectl apply -f base/monitoring/prometheus.yaml
kubectl apply -f base/monitoring/grafana.yaml
kubectl wait --for=condition=ready pod -l app=prometheus -n questionario --timeout=120s
kubectl wait --for=condition=ready pod -l app=grafana -n questionario --timeout=120s

# 5. Verificar
kubectl get pods -n questionario
kubectl get services -n questionario
kubectl get pvc -n questionario
```

**Saída esperada:**
```
NAME                       READY   STATUS    RESTARTS   AGE
backend-xxxxx-aaaaa        1/1     Running   0          2m
backend-xxxxx-bbbbb        1/1     Running   0          2m
backend-xxxxx-ccccc        1/1     Running   0          2m
backend-xxxxx-ddddd        1/1     Running   0          2m  ? 4 réplicas
frontend-xxxxx             1/1     Running   0          2m
mongodb-xxxxx              1/1     Running   0          3m
rabbitmq-xxxxx             1/1     Running   0          3m
prometheus-xxxxx           1/1     Running   0          1m
grafana-xxxxx              1/1     Running   0          1m
```

---

## ?? Acessando a Aplicação

```bash
# Frontend
minikube service frontend -n questionario --url
# ? http://192.168.49.2:30080

# Backend (API)
minikube service backend -n questionario --url
# ? http://192.168.49.2:30500

# Grafana
minikube service grafana -n questionario --url
# ? http://192.168.49.2:30300

# RabbitMQ UI
minikube service rabbitmq -n questionario --url
# ? http://192.168.49.2:31567
```

**Credenciais:**
- Grafana: `admin` / `admin123`
- RabbitMQ: `admin` / `admin123`

---

## ?? Configurando o Grafana

1. Acessa `http://<minikube-ip>:30300`
2. Login: `admin` / `admin123`
3. **Configuration ? Data Sources ? Prometheus ? Test** (deve estar ?)
4. **Dashboards ? + ? Add Visualization**

**Queries Prometheus:**

| Dashboard | Query |
|-----------|-------|
| CPU por Pod | `sum(rate(container_cpu_usage_seconds_total{namespace="questionario",container!=""}[5m])) by (pod)` |
| Memória por Pod | `sum(container_memory_usage_bytes{namespace="questionario",container!=""}) by (pod) / 1024 / 1024` |
| Réplicas Backend | `count(kube_pod_info{namespace="questionario",pod=~"backend.*"})` |
| HTTP Requests | `sum(rate(http_requests_total{namespace="questionario"}[5m])) by (path)` |

---

## ? Stress Test

```powershell
# PowerShell
$url = minikube service backend -n questionario --url
1..1000 | ForEach-Object -Parallel {
    Invoke-WebRequest -Uri "$using:url/api/questionario" -Method GET -ErrorAction SilentlyContinue
} -ThrottleLimit 50
```

```bash
# Linux/Mac
url=$(minikube service backend -n questionario --url)
seq 1 1000 | xargs -P50 -I{} curl -s "$url/api/questionario" > /dev/null
```

> ?? Abra o Grafana e veja os gráficos mudarem! Tire prints para o relatório.

---

## ?? Requisitos do Trabalho (Rubrica)

| Requisito | Evidência | Arquivo |
|-----------|-----------|---------|
| ? Docker containers | DockerHub: `luqui25/lucas-fluminense-backend` | - |
| ? Volumes (PVC) | MongoDB, RabbitMQ, Prometheus | `mongodb.yaml` L10-20 |
| ? Alta disponibilidade | Backend 4 réplicas | `backend.yaml` L12 |
| ? Readiness Probe | Todos os deployments | `backend.yaml` L55-60 |
| ? Liveness Probe | Todos os deployments | `backend.yaml` L61-66 |
| ? Stress test | Script PowerShell/Bash | Acima ?? |
| ? Prometheus | Coleta métricas | `prometheus.yaml` |
| ? PVC Prometheus | 2GB persistente | `prometheus.yaml` L60-70 |
| ? Grafana | NodePort :30300 | `grafana.yaml` |
| ? Dashboards | CPU, RAM, HTTP | Grafana UI |

---

## ??? Comandos Úteis

```bash
# Ver pods
kubectl get pods -n questionario

# Ver logs em tempo real
kubectl logs -n questionario <pod-name> -f

# Logs de todas as réplicas do backend
kubectl logs -n questionario -l app=backend --tail=50

# Reiniciar backend
kubectl rollout restart deployment backend -n questionario

# Escalar réplicas
kubectl scale deployment backend -n questionario --replicas=6

# Port-forward Prometheus
kubectl port-forward -n questionario service/prometheus 9090:9090
# ? http://localhost:9090/targets

# Ver eventos
kubectl get events -n questionario --sort-by='.lastTimestamp'

# Remover tudo
kubectl delete namespace questionario
```

---

## ?? Troubleshooting

| Problema | Solução |
|----------|---------|
| `ImagePullBackOff` | Verificar nome da imagem no YAML |
| `CrashLoopBackOff` | `kubectl logs -n questionario <pod>` |
| `CreateContainerConfigError` | `kubectl apply -f base/secrets.yaml` |
| Pod `Pending` | `minikube ssh -- df -h` (sem espaço) |
| RabbitMQ demora | Normal! Aguardar até 3 min |
| Grafana sem dados | Port-forward Prometheus e verificar `/targets` |

---

## ?? Documentação Adicional

- [`docs/GUIA_APRENDIZADO.md`](docs/GUIA_APRENDIZADO.md) — Explicação de cada conceito K8s
- [`docs/GUIA_PROFESSOR.md`](docs/GUIA_PROFESSOR.md) — Rubrica completa com evidências

---

**Lucas Esteves** | GitHub: [@LucasEsteves2](https://github.com/LucasEsteves2)
