using GraphQL.Types;
using QuestionarioOnline.Application.DTOs.Responses;


namespace QuestionarioOnline.Api.GraphQL.Types
{
    public class QuestionarioListaType : ObjectGraphType<QuestionarioListaDto>
    {
        public QuestionarioListaType()
        {
            Field(q => q.Id, type: typeof(IdGraphType)).Description("O ID do questionário.");
            Field(q => q.Titulo).Description("O título do questionário.");
            Field(q => q.DataInicio).Description("A data de criação do questionário.");
            Field(q => q.DataFim, nullable: true).Description("A data de encerramento do questionário, se aplicável.");
            Field(q => q.TotalPerguntas, nullable: true).Description("Total de perguntas");

            //dps adicionar mais propriedades, como perguntas e respostas
        }
    }

    public class QuestionarioType : ObjectGraphType<QuestionarioDto>
    {
        public QuestionarioType()
        {
            Field(q => q.Id).Description("O ID do questionário.");
            Field(q => q.Titulo).Description("O título do questionário.");
            Field(q => q.DataInicio).Description("A data de criação do questionário.");
            Field(q => q.DataFim, nullable: true).Description("A data de encerramento do questionário, se aplicável.");
           // Field(q => q.Perguntas, type: typeof(ListGraphType<PerguntaType>)).Description("As perguntas do questionário.");

            //dps adicionar mais propriedades, como perguntas e respostas

        }
    }
}
