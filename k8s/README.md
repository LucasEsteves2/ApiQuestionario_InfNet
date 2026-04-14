# ============================================================
# COMO USAR NO MINIKUBE
# ============================================================
#
# 1. Iniciar o Minikube:
#    minikube start
#
# 2. Aplicar todos os manifests (na ordem):
#    kubectl apply -f k8s/namespace.yaml
#    kubectl apply -f k8s/secrets.yaml
#    kubectl apply -f k8s/mongodb.yaml
#    kubectl apply -f k8s/rabbitmq.yaml
#    kubectl apply -f k8s/backend.yaml
#    kubectl apply -f k8s/frontend.yaml
#
# 3. Verificar se tudo subiu:
#    kubectl get pods -n questionario
#    kubectl get services -n questionario
#
# 4. Acessar o Frontend no navegador:
#    minikube service frontend -n questionario
#
# 5. Ver logs de um pod:
#    kubectl logs -f deployment/backend -n questionario
#    kubectl logs -f deployment/mongodb -n questionario
#
# 6. Escalar o backend (mais réplicas):
#    kubectl scale deployment backend --replicas=3 -n questionario
#
# 7. Derrubar tudo:
#    kubectl delete namespace questionario
#
# ============================================================
