using GraphQL.Types;
using QuestionarioOnline.Api.GraphQL.Mutations;
using QuestionarioOnline.Api.GraphQL.Queries;

namespace QuestionarioOnline.Api.GraphQL
{
    public class QuestionarioSchema : Schema
    {
        public QuestionarioSchema(IServiceProvider provider)
        {
            Query = provider.GetRequiredService<QuestionarioQuery>();
            Mutation = provider.GetRequiredService<QuestionarioMutation>();
        }
    }
}
