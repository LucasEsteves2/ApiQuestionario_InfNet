# ?? GUIA DE APRENDIZADO - Kubernetes

> **Objetivo:** Entender CADA conceito usado no projeto, linha por linha!

---

## ?? **Estrutura do Projeto**

```
k8s/
??? base/                           # Componentes reutilizáveis
?   ??? namespace.yaml              # Namespace (isola o projeto)
?   ??? secrets.yaml                # Senhas e chaves
?   ?
?   ??? database/                   # Bancos de dados
?   ?   ??? mongodb.yaml
?   ?   ??? rabbitmq.yaml
?   ?
?   ??? application/                # Sua aplicação
?   ?   ??? backend.yaml            # API .NET (4 réplicas)
?   ?   ??? frontend.yaml           # Angular + Nginx
?   ?
?   ??? monitoring/                 # Observabilidade
?       ??? prometheus.yaml         # Coleta métricas
?       ??? grafana.yaml            # Visualiza métricas
?
??? deploy.yaml                     # Deploy TUDO de uma vez
?
??? scripts/                        # Automação
?   ??? deploy.ps1                  # Sobe tudo automaticamente
?   ??? stress-test.ps1             # Teste de carga
?   ??? cleanup.ps1                 # Remove tudo
?
??? docs/                           # Documentação
    ??? GUIA_APRENDIZADO.md         # ?? VOCÊ ESTÁ AQUI!
    ??? GUIA_PROFESSOR.md           # Guia pro trabalho
```

---

## ?? **O QUE É CADA CONCEITO?**

### **1?? Namespace**

**Arquivo:** `base/namespace.yaml`

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: questionario
```

**O que é?**
- "Pasta virtual" dentro do Kubernetes
- Isola TODOS os recursos do projeto
- Evita conflito com outros projetos

**Analogia:**
- Igual pastas no Windows: `C:\MeusProjetos\Questionario\`
- Tudo do projeto fica dentro dessa "pasta"

**Por que precisa?**
- ? Organização (tudo separado)
- ? Segurança (um namespace não vê o outro)
- ? Facilita deletar tudo: `kubectl delete namespace questionario`

---

### **2?? Secrets**

**Arquivo:** `base/secrets.yaml`

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: questionario-secrets
  namespace: questionario
type: Opaque
stringData:
  MONGO_INITDB_ROOT_USERNAME: mongoadmin
  MONGO_INITDB_ROOT_PASSWORD: mongosecret123
  MONGO_CONNECTION_STRING: "mongodb://mongoadmin:mongosecret123@mongodb:27017"
```

**O que é?**
- Armazena dados sensíveis (senhas, chaves)
- Codificado em base64 automaticamente
- Equivalente ao arquivo `.env`

**Como os pods usam?**

```yaml
# backend.yaml
env:
  - name: ConnectionStrings__DefaultConnection
    valueFrom:
      secretKeyRef:
        name: questionario-secrets  # ? Nome do Secret
        key: MONGO_CONNECTION_STRING # ? Chave específica
```

**Fluxo:**
1. Kubernetes lê o Secret
2. Injeta a variável de ambiente no container
3. .NET lê a variável: `builder.Configuration.GetConnectionString("DefaultConnection")`

---

### **3?? PersistentVolumeClaim (PVC)**

**Arquivo:** `base/database/mongodb.yaml` (linhas 10-20)

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: mongodb-pvc
spec:
  accessModes:
    - ReadWriteOnce      # Só 1 pod lê/escreve por vez
  resources:
    requests:
      storage: 1Gi       # Pede 1GB de disco
```

**O que é?**
- "HD externo" pro container
- Dados persistem mesmo se o pod morrer

**Analogia:**
- **SEM PVC:** USB (despluga, perde tudo)
- **COM PVC:** HD externo (despluga, dados continuam lá)

**Exemplo prático:**

```yaml
# Deployment monta o PVC
volumeMounts:
  - name: mongodb-data
    mountPath: /data/db    # MongoDB salva aqui

volumes:
  - name: mongodb-data
    persistentVolumeClaim:
      claimName: mongodb-pvc  # ? Usa o PVC
```

**Fluxo:**
1. Kubernetes cria um "disco" de 1GB
2. Monta no path `/data/db` dentro do container
3. MongoDB salva dados nesse path
4. Se o pod reiniciar, os dados continuam lá! ?

---

### **4?? Deployment**

**Arquivo:** `base/application/backend.yaml` (linhas 1-50)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend
spec:
  replicas: 4  # ? Quantidade de pods
  selector:
    matchLabels:
      app: backend
  template:
    # Configuração do pod (imagem, variáveis, etc)
```

**O que é?**
- Gerenciador de pods
- Garante que SEMPRE tenham 4 pods rodando
- Se 1 pod morrer, cria outro automaticamente

**Fluxo:**

```
Deployment (backend)
    ? (gerencia)
ReplicaSet (backend-xxxxx)
    ? (cria)
Pods:
  - backend-xxxxx-aaaaa  (1/4)
  - backend-xxxxx-bbbbb  (2/4)
  - backend-xxxxx-ccccc  (3/4)
  - backend-xxxxx-ddddd  (4/4)
```

**Exemplo prático:**
```sh
# Você deleta 1 pod manualmente
kubectl delete pod backend-xxxxx-aaaaa -n questionario

# Deployment detecta: "Opa! Só tem 3 pods, faltam 4!"
# Cria outro automaticamente:
# backend-xxxxx-eeeee (novo pod!)
```

---

### **5?? Service**

**Arquivo:** `base/application/backend.yaml` (linhas 70-80)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: backend
spec:
  selector:
    app: backend  # ? Encontra TODOS os pods com essa label
  ports:
    - port: 5000
      targetPort: 8080
      nodePort: 30500
  type: NodePort
```

**O que é?**
- "DNS interno" do Kubernetes
- Load Balancer automático (distribui requisições entre os 4 pods)

**Tipos de Service:**

| Tipo | Acessível de onde? | Exemplo |
|------|-------------------|---------|
| **ClusterIP** | Só DENTRO do cluster | MongoDB, RabbitMQ |
| **NodePort** | FORA do cluster (porta 30000-32767) | Backend, Frontend, Grafana |
| **LoadBalancer** | Balanceador externo (cloud) | Produção (AWS, Azure) |

**Fluxo de requisição:**

```
Navegador ? http://192.168.49.2:30500/api/questionario
                ?
       Service (backend)
         /    |    \    \
       Pod1  Pod2 Pod3 Pod4  ? Distribui entre os 4!
```

---

### **6?? Probes (Healthchecks)**

**Arquivo:** `base/application/backend.yaml` (linhas 55-65)

```yaml
readinessProbe:
  httpGet:
    path: /api/questionario
    port: 8080
  initialDelaySeconds: 15  # Espera 15s antes de começar a checar
  periodSeconds: 10        # Checa a cada 10s

livenessProbe:
  httpGet:
    path: /api/questionario
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 15
```

**O que é?**

| Probe | Pergunta | Se falhar |
|-------|----------|-----------|
| **Readiness** | "Está pronto pra receber requisições?" | Para de enviar tráfego pro pod |
| **Liveness** | "Está vivo?" | Reinicia o pod |

**Exemplo prático:**

```
1. Pod inicia
2. Kubernetes espera 15s
3. Faz GET http://pod:8080/api/questionario
   - ? Status 200 ? Pod READY (recebe tráfego)
   - ? Status 500 ? Pod NOT READY (não recebe tráfego)
4. Repete a cada 10s
```

**Por que precisa?**
- ? Evita enviar requisições pra pod que ainda está inicializando
- ? Reinicia pods travados automaticamente

---

### **7?? ConfigMap**

**Arquivo:** `base/monitoring/prometheus.yaml` (linhas 10-50)

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
    scrape_configs:
      - job_name: 'backend'
        static_configs:
          - targets: ['backend:5000']
```

**O que é?**
- Arquivo de configuração "externo" pro container
- Igual a um `.json` ou `.yaml` que você injeta no pod

**Como funciona?**

```yaml
# Deployment monta o ConfigMap
volumeMounts:
  - name: prometheus-config
    mountPath: /etc/prometheus

volumes:
  - name: prometheus-config
    configMap:
      name: prometheus-config
```

**Fluxo:**
1. Kubernetes lê o ConfigMap
2. Cria um arquivo `/etc/prometheus/prometheus.yml` DENTRO do pod
3. Prometheus lê esse arquivo e configura automaticamente

**Vantagem:**
- ? Muda configuração SEM rebuildar a imagem!
- ? Reutiliza em vários ambientes (dev/staging/prod)

---

### **8?? Prometheus (Monitoramento)**

**Arquivo:** `base/monitoring/prometheus.yaml`

**O que faz?**
- Coleta métricas da aplicação (CPU, RAM, requisições HTTP)
- Armazena em banco de dados de séries temporais
- Grafana lê esses dados pra criar dashboards

**Arquitetura:**

```
Backend (.NET) ? Expõe métricas em /metrics
         ?
Prometheus ? Faz "scrape" (coleta) a cada 15s
         ?
Armazena no PVC (dados persistem!)
         ?
Grafana ? Lê os dados e cria gráficos
```

**Exemplo de métrica:**

```
# CPU do pod backend-xxxxx-aaaaa
container_cpu_usage_seconds_total{pod="backend-xxxxx-aaaaa"} 0.25

# Requisições HTTP no backend
http_requests_total{path="/api/questionario", status="200"} 1523
```

---

### **9?? Grafana (Visualização)**

**Arquivo:** `base/monitoring/grafana.yaml`

**O que faz?**
- Conecta no Prometheus
- Cria dashboards interativos com gráficos
- Mostra CPU, RAM, requisições, latência, etc

**Fluxo:**

```
1. Acessa http://192.168.49.2:30300
2. Login: admin / admin123
3. Cria dashboard:
   - Métrica: container_memory_usage_bytes
   - Query: sum(container_memory_usage_bytes{namespace="questionario"})
   - Resultado: Gráfico mostrando RAM dos pods
```

**Datasource (ConfigMap):**

```yaml
datasources:
  - name: Prometheus
    type: prometheus
    url: http://prometheus:9090  # ? Service do Prometheus
```

Kubernetes resolve `prometheus:9090` automaticamente pro IP correto!

---

## ?? **RESUMO: Como tudo se conecta?**

```
???????????????????????????????????????????????????????????????
?                    CLUSTER KUBERNETES                        ?
?  ?????????????????????????????????????????????????????????? ?
?  ?          NAMESPACE: questionario                       ? ?
?  ?                                                         ? ?
?  ?  ??? BANCOS DE DADOS (ClusterIP - só interno)          ? ?
?  ?  ?? MongoDB (PVC 1GB)                                  ? ?
?  ?  ?? RabbitMQ (PVC 500MB)                               ? ?
?  ?                                                         ? ?
?  ?  ?? APLICAÇÃO (NodePort - acessível externamente)      ? ?
?  ?  ?? Backend (4 réplicas)                               ? ?
?  ?  ?  ?? Lê secrets (connection string)                  ? ?
?  ?  ?  ?? Conecta no MongoDB via Service                  ? ?
?  ?  ?  ?? Expõe métricas em /metrics                      ? ?
?  ?  ?? Frontend (1 réplica)                               ? ?
?  ?                                                         ? ?
?  ?  ?? MONITORAMENTO                                       ? ?
?  ?  ?? Prometheus (PVC 2GB)                               ? ?
?  ?  ?  ?? Coleta métricas do backend                      ? ?
?  ?  ?  ?? Armazena no PVC                                 ? ?
?  ?  ?? Grafana (NodePort 30300)                           ? ?
?  ?     ?? Lê dados do Prometheus                          ? ?
?  ?????????????????????????????????????????????????????????? ?
???????????????????????????????????????????????????????????????
```

---

## ?? **Próximos Passos:**

1. ? Leia o **GUIA_PROFESSOR.md** pra entender como rodar e gerar evidências
2. ? Rode `.\scripts\deploy.ps1` pra subir tudo
3. ? Acesse o Grafana e explore os dashboards
4. ? Rode `.\scripts\stress-test.ps1` e veja as métricas mudarem!

---

**Dúvidas?** Releia esta seção devagar! Cada conceito é fundamental! ??
