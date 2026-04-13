using GraphQL;
using GraphQL.Types;
using QuestionarioOnline.Api.GraphQL.Types;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Application.Services;

namespace QuestionarioOnline.Api.GraphQL.Queries
{
    public class QuestionarioQuery : ObjectGraphType
    {
        public QuestionarioQuery(IQuestionarioService service)
        {
            Field<ListGraphType<QuestionarioListaType>>("questionarios")
                .ResolveAsync(async x => await service.ListarTodosQuestionariosAsync());

            Field<QuestionarioType>("questionario")
             .Arguments(new QueryArguments(
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" }
             ))
             .ResolveAsync(async context =>
             {
                 var id = context.GetArgument<Guid>("id");
                 var result = await service.ObterQuestionarioPorIdAsync(id);

                 if (!result.IsSuccess)
                     throw new ExecutionError(result.Error);

                 return result.Value;
             });


        }
    }
}
