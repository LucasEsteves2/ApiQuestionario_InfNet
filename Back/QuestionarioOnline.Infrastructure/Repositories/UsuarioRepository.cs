using MongoDB.Driver;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Interfaces;
using QuestionarioOnline.Domain.ValueObjects;
using QuestionarioOnline.Infrastructure.Persistence;

namespace QuestionarioOnline.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IMongoCollection<Usuario> _collection;

    public UsuarioRepository(MongoDbContext context)
    {
        _collection = context.Usuarios;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Usuario>.Filter.Eq("email", email.Address);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Usuario>.Filter.Eq("email", email.Address);
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(usuario, cancellationToken: cancellationToken);
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(u => u.Id == usuario.Id, usuario, cancellationToken: cancellationToken);
    }
}

