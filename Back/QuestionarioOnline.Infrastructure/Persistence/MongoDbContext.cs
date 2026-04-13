using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        RegisterConventions();
        RegisterClassMaps();
    }

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Usuario> Usuarios =>
        _database.GetCollection<Usuario>("usuarios");

    public IMongoCollection<Questionario> Questionarios =>
        _database.GetCollection<Questionario>("questionarios");

    public IMongoCollection<Resposta> Respostas =>
        _database.GetCollection<Resposta>("respostas");

    private static void RegisterConventions()
    {
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("QuestionarioOnlineConventions", pack, _ => true);
    }

    private static void RegisterClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Usuario)))
        {
            BsonClassMap.RegisterClassMap<Usuario>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(u => u.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(u => u.Nome);
                cm.MapMember(u => u.SenhaHash);
                cm.MapMember(u => u.Role);
                cm.MapMember(u => u.DataCriacao);
                cm.MapMember(u => u.Ativo);
                cm.MapMember(u => u.Email).SetSerializer(new EmailSerializer());
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Questionario)))
        {
            BsonClassMap.RegisterClassMap<Questionario>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(q => q.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(q => q.Titulo);
                cm.MapMember(q => q.Descricao);
                cm.MapMember(q => q.Status);
                cm.MapMember(q => q.UsuarioId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(q => q.DataCriacao);
                cm.MapMember(q => q.DataEncerramento);
                cm.MapMember(q => q.PeriodoColeta);
                cm.MapField("_perguntas").SetElementName("perguntas");
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Pergunta)))
        {
            BsonClassMap.RegisterClassMap<Pergunta>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(p => p.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(p => p.QuestionarioId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(p => p.Texto);
                cm.MapMember(p => p.Ordem);
                cm.MapMember(p => p.Obrigatoria);
                cm.MapField("_opcoes").SetElementName("opcoes");
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(OpcaoResposta)))
        {
            BsonClassMap.RegisterClassMap<OpcaoResposta>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(o => o.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(o => o.PerguntaId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(o => o.Texto);
                cm.MapMember(o => o.Ordem);
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Resposta)))
        {
            BsonClassMap.RegisterClassMap<Resposta>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(r => r.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(r => r.QuestionarioId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(r => r.DataResposta);
                cm.MapMember(r => r.Estado);
                cm.MapMember(r => r.Cidade);
                cm.MapMember(r => r.RegiaoGeografica);
                cm.MapMember(r => r.DispositivoTipo);
                cm.MapMember(r => r.NavegadorTipo);
                cm.MapMember(r => r.OrigemResposta);
                cm.MapField("_itens").SetElementName("itens");
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(RespostaItem)))
        {
            BsonClassMap.RegisterClassMap<RespostaItem>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(ri => ri.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(ri => ri.RespostaId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(ri => ri.PerguntaId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapMember(ri => ri.OpcaoRespostaId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(PeriodoColeta)))
        {
            BsonClassMap.RegisterClassMap<PeriodoColeta>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.DataInicio);
                cm.MapMember(p => p.DataFim);
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(OrigemResposta)))
        {
            BsonClassMap.RegisterClassMap<OrigemResposta>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(o => o.Hash);
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}

/// <summary>
/// Serializa o value object Email como uma string simples no MongoDB.
/// </summary>
internal class EmailSerializer : IBsonSerializer<Email>
{
    public Type ValueType => typeof(Email);

    public Email Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var address = context.Reader.ReadString();
        return Email.Create(address);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Email value)
    {
        context.Writer.WriteString(value.Address);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        => Serialize(context, args, (Email)value);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => Deserialize(context, args);
}
