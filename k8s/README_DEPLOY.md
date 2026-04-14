# ?? Guia Completo - Deploy no Minikube

## ?? Pré-requisitos
1. **Minikube instalado** ? `minikube version`
2. **kubectl instalado** ? `kubectl version --client`
3. **Docker rodando** (o Minikube usa o Docker como driver)

---

## ?? 1. Iniciar o Minikube

```bash
# Inicia o cluster Kubernetes local
minikube start

# Verifica se está rodando
minikube status
```

**O que faz?**  
Cria um cluster Kubernetes completo na sua máquina. É como subir um "mini servidor" local.

---

## ?? 2. Aplicar os Manifests (ORDEM IMPORTA!)

### **Por que a ordem importa?**
- O **Namespace** precisa existir antes de criar recursos nele.
- **Secrets** precisam existir antes dos Deployments que os usam.
- **MongoDB e RabbitMQ** (dependências) devem subir antes do **Backend**.
- O **Backend** deve estar rodando antes do **Frontend** (senão a API não responde).

### **Ordem correta:**

```bash
# 1?? Cria o namespace (a "pasta" que isola tudo)
kubectl apply -f k8s/namespace.yaml

# 2?? Cria os secrets (senhas, chaves)
kubectl apply -f k8s/secrets.yaml

# 3?? Sobe o MongoDB (banco de dados)
kubectl apply -f k8s/mongodb.yaml

# 4?? Sobe o RabbitMQ (fila de mensagens)
kubectl apply -f k8s/rabbitmq.yaml

# ? AGUARDA os bancos ficarem prontos (IMPORTANTE!)
kubectl wait --for=condition=ready pod -l app=mongodb -n questionario --timeout=120s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n questionario --timeout=120s

# 5?? Sobe o Backend (.NET API)
kubectl apply -f k8s/backend.yaml

# ? AGUARDA o backend ficar pronto
kubectl wait --for=condition=ready pod -l app=backend -n questionario --timeout=120s

# 6?? Sobe o Frontend (Angular)
kubectl apply -f k8s/frontend.yaml
```

### **Ou aplica tudo de uma vez (o Kubernetes tenta resolver as dependências):**

```bash
kubectl apply -f k8s/
```

?? **Atenção:** Aplicar tudo de uma vez pode fazer o Backend tentar conectar no MongoDB antes dele estar pronto. O Kubernetes vai reiniciar o pod automaticamente até conseguir, mas a forma sequencial é mais segura.

---

## ?? 3. Comandos para Verificar se Subiu

### **Ver todos os recursos criados:**

```bash
kubectl get all -n questionario
```

**O que faz?**  
Lista Pods, Services, Deployments no namespace `questionario`.

**Saída esperada:**
```
NAME                            READY   STATUS    RESTARTS   AGE
pod/backend-xxxxx               1/1     Running   0          2m
pod/backend-yyyyy               1/1     Running   0          2m
pod/frontend-xxxxx              1/1     Running   0          1m
pod/mongodb-xxxxx               1/1     Running   0          3m
pod/rabbitmq-xxxxx              1/1     Running   0          3m

NAME               TYPE        CLUSTER-IP      EXTERNAL-IP   PORT(S)
service/backend    ClusterIP   10.96.xxx.xxx   <none>        5000/TCP
service/frontend   NodePort    10.96.xxx.xxx   <none>        80:30080/TCP
service/mongodb    ClusterIP   10.96.xxx.xxx   <none>        27017/TCP
service/rabbitmq   ClusterIP   10.96.xxx.xxx   <none>        5672/TCP,15672/TCP
```

---

### **Ver detalhes de um Pod específico:**

```bash
# Listar pods
kubectl get pods -n questionario

# Ver logs de um pod (ex: backend)
kubectl logs -n questionario backend-xxxxx

# Ver logs em tempo real (útil pra debug)
kubectl logs -n questionario backend-xxxxx -f

# Descrever um pod (mostra eventos, erros, etc)
kubectl describe pod -n questionario backend-xxxxx
```

---

### **Verificar se os bancos estão saudáveis:**

```bash
# MongoDB
kubectl exec -n questionario mongodb-xxxxx -- mongosh --eval "db.adminCommand('ping')"

# RabbitMQ
kubectl exec -n questionario rabbitmq-xxxxx -- rabbitmq-diagnostics -q ping
```

---

## ?? 4. Acessar a Aplicação

### **Frontend (Angular):**

```bash
# Pega a URL do frontend
minikube service frontend -n questionario --url
```

**O que faz?**  
O Minikube cria um túnel e te dá a URL pra acessar no navegador.

**Você vai ver algo tipo:**
```
http://192.168.49.2:30080
```

Copia e cola no navegador! ??

---

### **Backend (API .NET):**

```bash
# Pega a URL do backend (mas ele é ClusterIP, então precisa de port-forward)
kubectl port-forward -n questionario service/backend 5000:5000
```

**O que faz?**  
Cria um "túnel" da sua máquina pro cluster. Agora acessa `http://localhost:5000/api/questionario` no navegador.

---

### **RabbitMQ Management UI:**

```bash
kubectl port-forward -n questionario service/rabbitmq 15672:15672
```

Acessa: `http://localhost:15672`  
**Login:** `admin` / `admin123` (valores do `secrets.yaml`)

---

## ?? 5. Comandos de Debug

### **Pod não inicia (status CrashLoopBackOff ou Error):**

```bash
# Ver logs do pod
kubectl logs -n questionario <nome-do-pod>

# Ver eventos (mostra porque o pod falhou)
kubectl describe pod -n questionario <nome-do-pod>
```

---

### **Reiniciar um Deployment:**

```bash
kubectl rollout restart deployment backend -n questionario
```

**O que faz?**  
Recria todos os pods daquele Deployment (útil se mudou algo no código/imagem).

---

### **Deletar e recriar tudo:**

```bash
# Deleta todos os recursos
kubectl delete -f k8s/

# Recria
kubectl apply -f k8s/
```

---

## ?? 6. Atualizar a Imagem Docker

Se você fez mudanças no código e quer atualizar no Kubernetes:

```bash
# 1?? Faz build da nova imagem (exemplo pro backend)
cd Back/QuestionarioOnline
docker build -t luqui25/lucas-fluminense-backend:latest .

# 2?? Envia pro DockerHub
docker push luqui25/lucas-fluminense-backend:latest

# 3?? Força o Kubernetes baixar a nova imagem
kubectl rollout restart deployment backend -n questionario

# 4?? Acompanha o rollout
kubectl rollout status deployment backend -n questionario
```

---

## ?? 7. Limpar Tudo

```bash
# Para o Minikube
minikube stop

# Deleta o cluster (libera espaço em disco)
minikube delete
```

---

## ?? Glossário Rápido

| Termo | O que é? |
|-------|---------|
| **Namespace** | "Pasta" que isola recursos (evita conflito com outros projetos) |
| **Pod** | Container rodando (menor unidade do Kubernetes) |
| **Deployment** | Controla quantos Pods rodar e garante que estão sempre vivos |
| **Service** | "DNS interno" que dá um nome fixo pros Pods (ex: `mongodb`, `backend`) |
| **ClusterIP** | Service só acessível DENTRO do cluster |
| **NodePort** | Service exposto FORA do cluster (porta 30000-32767) |
| **PVC** | "Disco" persistente (senão os dados somem quando o Pod reinicia) |
| **Secret** | Guarda senhas/chaves de forma segura |
| **Healthcheck (Probe)** | Kubernetes verifica se o Pod está saudável e reinicia se travar |

---

## ?? Resumo do Fluxo

```
1. minikube start                      ? Inicia o cluster
2. kubectl apply -f k8s/               ? Sobe todos os recursos
3. kubectl get all -n questionario     ? Verifica se está tudo rodando
4. minikube service frontend -n questionario --url  ? Pega a URL
5. Abre no navegador ??
```

---

## ?? Problemas Comuns

### **"ImagePullBackOff" ou "ErrImagePull"**
- O Kubernetes não conseguiu baixar a imagem do DockerHub.
- **Solução:** Verifica se a imagem existe: `docker pull luqui25/lucas-fluminense-backend:latest`

### **Backend fica reiniciando (CrashLoopBackOff)**
- Provavelmente não consegue conectar no MongoDB/RabbitMQ.
- **Solução:** Verifica os logs: `kubectl logs -n questionario backend-xxxxx`

### **Frontend carrega mas não faz chamadas pra API**
- A URL da API pode estar hardcoded errado no Angular.
- **Solução:** Verifica o `environment.ts` do Angular (tem que apontar pro service do backend).

---

**Dúvidas?** Roda os comandos e cola a saída aqui que te ajudo! ??
