using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestionarioOnline.Api.GraphQL.InputTypes;
using QuestionarioOnline.Api.GraphQL.Types;
using QuestionarioOnline.Application.DTOs.Requests;
using QuestionarioOnline.Application.Interfaces;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.ValueObjects;
using QuestionarioOnline.Infrastructure.Persistence;
using System.Collections;

namespace QuestionarioOnline.Api.GraphQL.Mutations
{
    public class QuestionarioMutation : ObjectGraphType
    {
        public QuestionarioMutation(IQuestionarioService service, IServiceProvider serviceProvider)
        {
            CriarQuestionarioMutation(service, serviceProvider);
            DeletarQuestionarioMutation(service);
        }

        private void CriarQuestionarioMutation(IQuestionarioService service, IServiceProvider serviceProvider)
        {
            Field<QuestionarioType>("criarQuestionario")
                .Description("Cria um novo questionário com perguntas e opções")
                .Arguments(
                    new QueryArguments(
                        new QueryArgument<NonNullGraphType<StringGraphType>>
                        {
                            Name = "titulo",
                            Description = "Título do questionário (obrigatório)"
                        },
                        new QueryArgument<StringGraphType>
                        {
                            Name = "descricao",
                            Description = "Descrição do questionário (opcional)"
                        },
                        new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<CriarPerguntaInput>>>>
                        {
                            Name = "perguntas",
                            Description = "Lista de perguntas com suas opções (obrigatório)"
                        }
                    )
                )
                .ResolveAsync(async context =>
                {
                    try
                    {
                        var titulo = context.GetArgument<string>("titulo");
                        var descricao = context.GetArgument<string?>("descricao");
                        var dataInicio = DateTime.UtcNow;
                        var dataFim = dataInicio.AddDays(30);

                        using var scope = serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<QuestionarioOnlineDbContext>();
                        
                        var usuarioId = await ObterUsuarioMock(dbContext);
                        var perguntas = ConverterPerguntas(context);

                        var request = new CriarQuestionarioRequest(
                            titulo,
                            descricao,
                            dataInicio,
                            dataFim,
                            perguntas
                        );

                        var result = await service.CriarQuestionarioAsync(request, usuarioId);

                        if (!result.IsSuccess)
                            throw new ExecutionError(result.Error);

                        return result.Value;
                    }
                    catch (ExecutionError)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new ExecutionError($"Erro ao criar questionário: {ex.Message}", ex);
                    }
                });
        }

        private void DeletarQuestionarioMutation(IQuestionarioService service)
        {
            Field<BooleanGraphType>("deletarQuestionario")
                .Description("Deleta um questionário existente")
                .Arguments(
                    new QueryArguments(
                        new QueryArgument<NonNullGraphType<IdGraphType>>
                        {
                            Name = "id",
                            Description = "ID do questionário a ser deletado"
                        }
                    )
                )
                .ResolveAsync(async context =>
                {
                    var id = context.GetArgument<Guid>("id");
                    var result = await service.DeletarQuestionarioAsync(id);

                    if (!result.IsSuccess)
                        throw new ExecutionError(result.Error);

                    return true;
                });
        }

        private static async Task<Guid> ObterUsuarioMock(QuestionarioOnlineDbContext dbContext)
        {
            var usuario = await dbContext.Usuarios.FirstOrDefaultAsync();

            if (usuario == null)
            {
                usuario = new Usuario(
                    "Usuário Teste GraphQL",
                    Email.Create("teste@graphql.com"),
                    "hash_temporario_123",
                    UsuarioRole.Analista
                );

                dbContext.Usuarios.Add(usuario);
                await dbContext.SaveChangesAsync();
            }

            return usuario.Id;
        }

        private static List<CriarPerguntaDto> ConverterPerguntas(IResolveFieldContext context)
        {
            var perguntasRaw = context.GetArgument<object>("perguntas");
            var perguntas = new List<CriarPerguntaDto>();

            if (perguntasRaw is not IEnumerable perguntasEnum)
                return perguntas;

            foreach (var perguntaObj in perguntasEnum)
            {
                if (perguntaObj is IDictionary<string, object> pergunta)
                {
                    var opcoes = ConverterOpcoes(pergunta);

                    perguntas.Add(new CriarPerguntaDto(
                        Texto: pergunta["texto"]?.ToString() ?? string.Empty,
                        Ordem: Convert.ToInt32(pergunta["ordem"]),
                        Obrigatoria: Convert.ToBoolean(pergunta["obrigatoria"]),
                        Opcoes: opcoes
                    ));
                }
            }

            return perguntas;
        }

        private static List<CriarOpcaoDto> ConverterOpcoes(IDictionary<string, object> pergunta)
        {
            var opcoes = new List<CriarOpcaoDto>();

            if (pergunta["opcoes"] is not IEnumerable opcoesEnum)
                return opcoes;

            foreach (var opcaoObj in opcoesEnum)
            {
                if (opcaoObj is IDictionary<string, object> opcao)
                {
                    opcoes.Add(new CriarOpcaoDto(
                        Texto: opcao["texto"]?.ToString() ?? string.Empty,
                        Ordem: Convert.ToInt32(opcao["ordem"])
                    ));
                }
            }

            return opcoes;
        }
    }
}
