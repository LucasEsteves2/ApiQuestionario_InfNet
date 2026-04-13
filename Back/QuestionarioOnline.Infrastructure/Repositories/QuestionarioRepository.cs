using MongoDB.Driver;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Infrastructure.Persistence;

namespace QuestionarioOnline.Infrastructure.Repositories;

public class QuestionarioRepository : IQuestionarioRepository
{
    private readonly IMongoCollection<Questionario> _collection;

    public QuestionarioRepository(MongoDbContext context)
    {
        _collection = context.Questionarios;
    }

    public async Task<Questionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(q => q.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Questionario?> ObterPorIdComPerguntasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // No MongoDB, perguntas e opções são embedded no documento — retorna tudo já incluso.
        return await _collection
            .Find(q => q.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Questionario>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(_ => true)
            .SortByDescending(q => q.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Questionario>> ObterTodosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(q => q.UsuarioId == usuarioId)
            .SortByDescending(q => q.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Questionario> Items, int Total)> ObterTodosPorUsuarioPaginadoAsync(
        Guid usuarioId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Questionario>.Filter.Eq(q => q.UsuarioId, usuarioId);

        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _collection
            .Find(filter)
            .SortByDescending(q => q.DataCriacao)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IEnumerable<Questionario>> ObterPublicadosAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(q => q.Status == StatusQuestionario.Ativo)
            .SortByDescending(q => q.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Questionario questionario, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(questionario, cancellationToken: cancellationToken);
    }

    public async Task AtualizarAsync(Questionario questionario, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(q => q.Id == questionario.Id, questionario, cancellationToken: cancellationToken);
    }

    public async Task DeletarAsync(Questionario questionario, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(q => q.Id == questionario.Id, cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(q => q.Id == id, cancellationToken: cancellationToken);
        return count > 0;
    }
}

