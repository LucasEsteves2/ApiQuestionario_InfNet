namespace QuestionarioOnline.Application.Interfaces;

public interface IMetricasService
{
    void QuestionarioCriado();
    void QuestionarioEncerrado();
    void QuestionarioDeletado();
    void RespostaEnviadaParaFila();
    void RespostaProcessada();
    void ErroDominio(string operacao);
}
