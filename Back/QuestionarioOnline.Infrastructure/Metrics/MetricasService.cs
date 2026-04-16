using Prometheus;
using QuestionarioOnline.Application.Interfaces;

namespace QuestionarioOnline.Infrastructure.Metrics;

public class MetricasService : IMetricasService
{
    private static readonly Counter QuestionariosCriados = Prometheus.Metrics
        .CreateCounter("questionarios_criados_total", "Total de questionarios criados");

    private static readonly Counter QuestionariosEncerrados = Prometheus.Metrics
        .CreateCounter("questionarios_encerrados_total", "Total de questionarios encerrados");

    private static readonly Counter QuestionariosDeletados = Prometheus.Metrics
        .CreateCounter("questionarios_deletados_total", "Total de questionarios deletados");

    private static readonly Counter RespostasEnviadasParaFila = Prometheus.Metrics
        .CreateCounter("respostas_fila_total", "Total de respostas enviadas para a fila RabbitMQ");

    private static readonly Counter RespostasProcessadas = Prometheus.Metrics
        .CreateCounter("respostas_processadas_total", "Total de respostas processadas pelo worker");

    private static readonly Counter ErrosDominio = Prometheus.Metrics
        .CreateCounter("erros_dominio_total", "Total de erros de dominio", labelNames: ["operacao"]);

    public void QuestionarioCriado() => QuestionariosCriados.Inc();
    public void QuestionarioEncerrado() => QuestionariosEncerrados.Inc();
    public void QuestionarioDeletado() => QuestionariosDeletados.Inc();
    public void RespostaEnviadaParaFila() => RespostasEnviadasParaFila.Inc();
    public void RespostaProcessada() => RespostasProcessadas.Inc();
    public void ErroDominio(string operacao) => ErrosDominio.WithLabels(operacao).Inc();
}
