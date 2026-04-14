# ============================================================
# STRESS TEST - Testa a aplicação sob carga
# ============================================================
# Faz múltiplas requisições simultâneas no backend
# pra gerar métricas visíveis no Grafana
# ============================================================

param(
    [int]$Requests = 1000,      # Número de requisições
    [int]$Concurrent = 50       # Requisições simultâneas
)

Write-Host "?????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? STRESS TEST - Questionário Online" -ForegroundColor Yellow
Write-Host "?????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Pega URL do backend
$backendUrl = minikube service backend -n questionario --url
Write-Host "?? Target: $backendUrl/api/questionario" -ForegroundColor Green
Write-Host "?? Requisições: $Requests" -ForegroundColor Green
Write-Host "? Concorrentes: $Concurrent" -ForegroundColor Green
Write-Host ""

Write-Host "? Iniciando stress test..." -ForegroundColor Yellow
Write-Host ""

# Função para fazer requisição
function Invoke-LoadTest {
    param($url)

    $jobs = @()
    $completed = 0
    $errors = 0
    $startTime = Get-Date

    for ($i = 0; $i -lt $Requests; $i++) {
        # Limita concorrência
        while (($jobs | Where-Object { $_.State -eq 'Running' }).Count -ge $Concurrent) {
            Start-Sleep -Milliseconds 10
        }

        # Inicia requisição em background
        $jobs += Start-Job -ScriptBlock {
            param($url)
            try {
                Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 10 -ErrorAction Stop | Out-Null
                return $true
            } catch {
                return $false
            }
        } -ArgumentList $url

        # Mostra progresso a cada 50 requisições
        if (($i + 1) % 50 -eq 0) {
            $completed = ($jobs | Where-Object { $_.State -eq 'Completed' }).Count
            $percent = [math]::Round(($completed / $Requests) * 100, 2)
            Write-Host "   Progresso: $completed/$Requests ($percent%)" -ForegroundColor Cyan
        }
    }

    # Aguarda todas as requisições terminarem
    Write-Host ""
    Write-Host "? Aguardando requisições finalizarem..." -ForegroundColor Yellow
    $jobs | Wait-Job | Out-Null

    # Conta sucessos e erros
    $results = $jobs | Receive-Job
    $success = ($results | Where-Object { $_ -eq $true }).Count
    $errors = $Requests - $success

    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalSeconds
    $rps = [math]::Round($Requests / $duration, 2)

    # Remove jobs
    $jobs | Remove-Job

    Write-Host ""
    Write-Host "?????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "? TESTE FINALIZADO!" -ForegroundColor Green
    Write-Host "?????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? RESULTADOS:" -ForegroundColor Cyan
    Write-Host "   Total de requisições: $Requests" -ForegroundColor White
    Write-Host "   Bem-sucedidas: $success" -ForegroundColor Green
    Write-Host "   Erros: $errors" -ForegroundColor $(if ($errors -gt 0) { 'Red' } else { 'Green' })
    Write-Host "   Duração: $([math]::Round($duration, 2))s" -ForegroundColor White
    Write-Host "   Requisições/segundo: $rps" -ForegroundColor Yellow
    Write-Host ""
}

# Executa teste
Invoke-LoadTest -url "$backendUrl/api/questionario"

# Instruções pós-teste
Write-Host "?????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?? GRAFANA - CAPTURE OS DASHBOARDS AGORA!" -ForegroundColor Yellow
Write-Host "?????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$grafanaUrl = minikube service grafana -n questionario --url
Write-Host "?? Abra o Grafana: $grafanaUrl" -ForegroundColor Green
Write-Host "?? Login: admin / admin123" -ForegroundColor Green
Write-Host ""
Write-Host "?? Tire prints dos dashboards mostrando:" -ForegroundColor Yellow
Write-Host "   - Aumento de CPU" -ForegroundColor White
Write-Host "   - Aumento de Memória" -ForegroundColor White
Write-Host "   - Número de requisições HTTP" -ForegroundColor White
Write-Host "   - Latência das requisições" -ForegroundColor White
Write-Host ""
Write-Host "?? Salve os prints para o relatório do trabalho!" -ForegroundColor Yellow
Write-Host ""
