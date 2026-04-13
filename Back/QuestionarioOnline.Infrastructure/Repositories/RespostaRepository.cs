using MongoDB.Driver;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;
using QuestionarioOnline.Infrastructure.Persistence;

namespace QuestionarioOnline.Infrastructure.Repositories;

public class RespostaRepository : IRespostaRepository
{
    private readonly IMongoCollection<Resposta> _collection;

    public RespostaRepository(MongoDbContext context)
    {
        _collection = context.Respostas;
    }

    public async Task<Resposta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resposta>> ObterPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(r => r.QuestionarioId == questionarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> JaRespondeuAsync(Guid questionarioId, OrigemResposta origemResposta, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Resposta>.Filter.And(
            Builders<Resposta>.Filter.Eq(r => r.QuestionarioId, questionarioId),
            Builders<Resposta>.Filter.Eq("origemResposta.hash", origemResposta.Hash)
        );

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task AdicionarAsync(Resposta resposta, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(resposta, cancellationToken: cancellationToken);
    }

    public async Task<int> ContarRespostasPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            r => r.QuestionarioId == questionarioId,
            cancellationToken: cancellationToken);
        return (int)count;
    }

    public async Task DeletarPorQuestionarioAsync(Guid questionarioId, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteManyAsync(r => r.QuestionarioId == questionarioId, cancellationToken);
    }
}

