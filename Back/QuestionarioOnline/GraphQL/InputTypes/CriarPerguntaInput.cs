using GraphQL.Types;

namespace QuestionarioOnline.Api.GraphQL.InputTypes
{
    public class CriarPerguntaInput : InputObjectGraphType
    {
        public CriarPerguntaInput()
        {
            Name = "CriarPerguntaInput";
            
            Field<NonNullGraphType<StringGraphType>>("texto")
                .Description("O texto da pergunta");
            
            Field<NonNullGraphType<IntGraphType>>("ordem")
                .Description("A ordem da pergunta no questionário");
            
            Field<NonNullGraphType<BooleanGraphType>>("obrigatoria")
                .Description("Indica se a pergunta é obrigatória");
            
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<CriarOpcaoInput>>>>("opcoes")
                .Description("Lista de opções de resposta para a pergunta");
        }
    }
}
