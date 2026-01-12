using GraphQL.Types;

namespace QuestionarioOnline.Api.GraphQL.InputTypes
{
    public class CriarOpcaoInput : InputObjectGraphType
    {
        public CriarOpcaoInput()
        {
            Name = "CriarOpcaoInput";
            
            Field<NonNullGraphType<StringGraphType>>("texto")
                .Description("O texto da opção de resposta");
            
            Field<NonNullGraphType<IntGraphType>>("ordem")
                .Description("A ordem da opção");
        }
    }
}
