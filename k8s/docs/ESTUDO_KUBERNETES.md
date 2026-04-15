kubectl delete namespace questionario
kubectl delete clusterrole prometheus kube-state-metrics
kubectl delete clusterrolebinding prometheus kube-state-metrics
kubectl apply -k k8s/
kubectl wait --for=condition=ready pod --all -n questionario --timeout=300s
kubectl get pods -n questionario





# ?? Estudo Kubernetes — Do Zero ao Deploy

> Anotações baseadas no projeto **Fluminense / QuestionarioOnline**

---

## ?? O que é um Cluster?

É o **conjunto de máquinas** que o Kubernetes gerencia. Pode ser:

```
Cluster
??? Máquina 1 (Node) ? roda alguns pods
??? Máquina 2 (Node) ? roda outros pods
??? Máquina 3 (Node) ? roda outros pods
```

No **Minikube** tudo roda em 1 máquina só (seu PC), simulando um cluster real.

---

## ?? Do maior pro menor

```
CLUSTER  (o conjunto todo)
  ??? NODE  (cada máquina física/virtual)
        ??? POD  (1 container rodando)
```

**Exemplo real no nosso projeto:**
```
Cluster (Minikube = seu PC)
  ??? Node (seu PC)
        ??? Pod: mongodb-abc123
        ??? Pod: rabbitmq-xyz789
        ??? Pod: backend-aaa111  ? Réplica 1
        ??? Pod: backend-bbb222  ? Réplica 2
        ??? Pod: frontend-ccc333
```

---

## ?? O que cada recurso faz

### **Pod**
> A menor unidade. É 1 container rodando.

```
Pod backend-abc123
  ??? Container: sua API .NET rodando na porta 8080
```

- Se morrer ? o **Deployment** cria outro automaticamente
- Cada réplica **é** um Pod
- O nome do pod é gerado automaticamente: `backend-abc123`

---

### **Deployment**
> Define **o que** rodar e **quantas** cópias.

```yaml
kind: Deployment
spec:
  replicas: 2        # quero 2 pods
  template:
    spec:
      containers:
        - image: luqui25/lucas-fluminense-backend:latest
```

É como falar pro Kubernetes: *"Mantém 2 cópias da minha API vivas sempre"*.

---

### **ReplicaSet**
> Criado automaticamente pelo Deployment. Garante que o número de pods está correto.

```
Você define:  replicas: 2
ReplicaSet monitora:
  ??? Pod 1 vivo? ?
  ??? Pod 2 vivo? ?
  ??? Pod 3 existe? ? não precisa
```

Se um pod morrer:
```
Pod 1 morreu ?
ReplicaSet detecta: "tenho 1, preciso de 2"
ReplicaSet cria Pod novo ?
```

> ?? Você **não mexe** no ReplicaSet diretamente — o Deployment cuida disso pra você.

---

### **Service**
> DNS interno. Dá um nome fixo pros pods.

Sem Service, cada pod tem um IP aleatório que muda quando reinicia.  
Com Service:
```
backend chama ? "mongodb:27017"
Service mongodb ? encontra o pod certo ? manda a requisição
```

**Tipos de Service:**

| Tipo | Quando usar |
|------|-------------|
| **ClusterIP** | Só dentro do cluster (MongoDB, RabbitMQ) |
| **NodePort** | Expõe pra fora em porta alta (30000-32767) — usado no Minikube |
| **LoadBalancer** | Expõe com IP público — usado em cloud (AWS, Azure, GCP) |

**Como o Service encontra o Pod? — pelo `selector`:**
```yaml
# No Service:
selector:
  app: mongodb      # procura pods com essa label

# No Deployment (pod):
labels:
  app: mongodb      # esse pod tem essa label
```
O Service lê a label e direciona o tráfego pro pod certo. ?

---

### **Namespace**
> "Pasta" que agrupa recursos e isola ambientes dentro do mesmo cluster.

```
Cluster
??? Namespace: questionario-dev   ? ambiente de dev
??? Namespace: questionario-hmg   ? ambiente de homologação
??? Namespace: questionario-prd   ? ambiente de produção
```

---

### **Secret**
> Armazena dados sensíveis (senhas, chaves). Equivalente ao `.env`.

```yaml
kind: Secret
stringData:
  MONGO_INITDB_ROOT_PASSWORD: mongosecret123
  JWT_SECRET_KEY: MinhaChaveSuperSecreta
```

Os pods puxam os valores do Secret assim:
```yaml
env:
  - name: MONGO_INITDB_ROOT_PASSWORD
    valueFrom:
      secretKeyRef:
        name: questionario-secrets
        key: MONGO_INITDB_ROOT_PASSWORD
```

---

### **PersistentVolumeClaim (PVC)**
> Pede um disco persistente pro cluster. Sem ele, os dados somem quando o pod reinicia.

```yaml
kind: PersistentVolumeClaim
spec:
  resources:
    requests:
      storage: 1Gi    # pede 1GB de disco
```

É o equivalente ao `volumes: mongodb_data:/data/db` do docker-compose.

---

## ?? Estrutura dos arquivos do projeto

```
k8s/
??? namespace.yaml        ? Cria o namespace "questionario"
??? secrets.yaml          ? Senhas (equivale ao .env)
??? mongodb.yaml          ? PVC + Deployment + Service do banco
??? rabbitmq.yaml         ? PVC + Deployment + Service da fila
??? backend.yaml          ? Deployment + Service da API .NET (2 réplicas)
??? frontend.yaml         ? Deployment + Service do Angular (NodePort)
??? ESTUDO_KUBERNETES.md  ? Este arquivo
```

Cada arquivo `.yaml` pode ter **múltiplos recursos** separados por `---`:

```yaml
# mongodb.yaml tem 3 blocos:

# Bloco 1 — disco
kind: PersistentVolumeClaim
---
# Bloco 2 — pods
kind: Deployment
---
# Bloco 3 — DNS interno
kind: Service
```

---

## ?? O que acontece quando você roda `kubectl apply`

```
kubectl apply -f k8s/mongodb.yaml

O Kubernetes lê o arquivo e cria:

  ?? PVC (disco de 1GB) ??????????????????
  ?                                       ?
  ?? Deployment ???????????????????????????
  ?   ?? Cria 1 Pod com container Mongo   ?
  ?      ?? Monta o disco do PVC          ?
  ?                                       ?
  ?? Service ??????????????????????????????
  ?   ?? Nome "mongodb" apontando pro Pod ?
  ?????????????????????????????????????????
```

---

## ?? Como os serviços se comunicam

```
Usuário acessa http://localhost:30080
        ?
        ?
  ???????????????
  ?   Service   ?  ? Recebe tráfego de fora (NodePort: 30080)
  ?  frontend   ?
  ???????????????
         ?
  ???????????????
  ?    Pod      ?  ? Container do Angular/Nginx
  ?  frontend   ?
  ???????????????
         ? chama "backend:5000"
         ?
  ???????????????
  ?   Service   ?  ? Distribui entre os pods (load balancer)
  ?   backend   ?
  ???????????????
         ?
  ???????????????
  ? Pod 1  Pod 2?  ? 2 cópias da API .NET
  ???????????????
         ? chama "mongodb:27017" e "rabbitmq:5672"
         ?
  ???????????????
  ?   Service   ?
  ?   mongodb   ?
  ???????????????
         ?
  ???????????????
  ?    Pod      ?  ? Container do MongoDB
  ???????????????
```

---

## ?? DEV / HMG / PRD no Kubernetes

Você usa os **mesmos arquivos yaml**, só muda o namespace e as variáveis:

```
k8s/
??? dev/
?   ??? namespace.yaml    ? name: questionario-dev
?   ??? secrets.yaml      ? senhas do banco de dev
?
??? hmg/
?   ??? namespace.yaml    ? name: questionario-hmg
?   ??? secrets.yaml      ? senhas do banco de hmg
?
??? prd/
    ??? namespace.yaml    ? name: questionario-prd
    ??? secrets.yaml      ? senhas do banco de produção
```

E nos deployments, muda só a **tag da imagem**:

```yaml
# dev
image: luqui25/lucas-fluminense-backend:dev

# hmg
image: luqui25/lucas-fluminense-backend:hmg

# prd
image: luqui25/lucas-fluminense-backend:latest
```

---

## ?? Pipeline CI/CD com Kubernetes

```
Você faz git push
        ?
        ?
   GitHub Actions (pipeline)
        ?
        ??? Roda os testes
        ?
        ??? Build da imagem Docker
        ?   ??? docker build + docker push ? DockerHub
        ?
        ??? Deploy no DEV (automático)
        ?   ??? kubectl apply -f k8s/dev/
        ?
        ??? Aprovação manual ? Deploy no HMG
        ?   ??? kubectl apply -f k8s/hmg/
        ?
        ??? Aprovação manual ? Deploy no PRD
            ??? kubectl apply -f k8s/prd/
```

---

## ?? Comandos essenciais

```sh
# Iniciar o Minikube
minikube start

# Subir TUDO de uma vez
kubectl apply -f k8s/

# Ver pods rodando
kubectl get pods -n questionario

# Ver serviços
kubectl get services -n questionario

# Ver logs de um pod
kubectl logs -f deployment/backend -n questionario

# Escalar réplicas ao vivo
kubectl scale deployment backend --replicas=5 -n questionario

# Acessar o frontend no navegador
minikube service frontend -n questionario

# Derrubar tudo
kubectl delete namespace questionario
```

---

## ? Resumão final

| Conceito | O que é | Analogia |
|----------|---------|----------|
| **Cluster** | Conjunto de máquinas | Seu datacenter |
| **Node** | Uma máquina do cluster | Um servidor |
| **Pod** | 1 container rodando | Um processo |
| **Réplica** | Cada cópia de um Pod | Mesma coisa que Pod |
| **ReplicaSet** | Garante N pods vivos | Supervisor automático |
| **Deployment** | Define o que/quantos rodar | Receita |
| **Service** | DNS interno entre pods | Lista telefônica |
| **Namespace** | Pasta que isola recursos | DEV / HMG / PRD |
| **Secret** | Senhas seguras | .env |
| **PVC** | Disco persistente | Volume do Docker |

---

## ?? Docker Compose vs Kubernetes

| Docker Compose | Kubernetes |
|----------------|------------|
| 1 arquivo `docker-compose.yml` | Vários arquivos `.yaml` organizados |
| Roda na **sua máquina** | Roda num **cluster** |
| `docker-compose up` | `kubectl apply -f k8s/` |
| Se o container morre, morreu | Se o pod morre, **recria sozinho** |
| 1 instância por serviço | Múltiplas **réplicas** |
| Dev local / projetos pequenos | Produção / escalar |
