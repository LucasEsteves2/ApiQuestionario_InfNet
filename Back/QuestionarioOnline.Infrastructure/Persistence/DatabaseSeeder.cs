using BCrypt.Net;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QuestionarioOnline.Domain.Entities;
using QuestionarioOnline.Domain.Enums;
using QuestionarioOnline.Domain.ValueObjects;

namespace QuestionarioOnline.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly MongoDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(MongoDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("?? Verificando dados iniciais...");

        var admin = await SeedAdminAsync();
        await SeedQuestionariosAsync(admin.Id);

        _logger.LogInformation("? Seed conclu�do.");
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Admin
    // ?????????????????????????????????????????????????????????????????????????
    private async Task<Usuario> SeedAdminAsync()
    {
        const string adminEmail = "admin@questionario.com";
        const string analistaEmail = "analista@questionario.com";

        var filter = Builders<Usuario>.Filter.Eq("email", adminEmail);
        var existente = await _context.Usuarios.Find(filter).FirstOrDefaultAsync();

        if (existente is not null)
        {
            _logger.LogInformation("Admin já existe, pulando.");
            return existente;
        }

        var admin = new Usuario(
            nome: "Administrador",
            email: Email.Create(adminEmail),
            senhaHash: BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            role: UsuarioRole.Admin
        );

        await _context.Usuarios.InsertOneAsync(admin);
        _logger.LogInformation("Admin criado | {Email} | senha: Admin@123", adminEmail);

        var filterAnalista = Builders<Usuario>.Filter.Eq("email", analistaEmail);
        var analistaExistente = await _context.Usuarios.Find(filterAnalista).FirstOrDefaultAsync();

        if (analistaExistente is null)
        {
            var analista = new Usuario(
                nome: "Analista Exemplo",
                email: Email.Create(analistaEmail),
                senhaHash: BCrypt.Net.BCrypt.HashPassword("Analista@123"),
                role: UsuarioRole.Analista
            );

            await _context.Usuarios.InsertOneAsync(analista);
            _logger.LogInformation("Analista criado | {Email} | senha: Analista@123", analistaEmail);
        }

        return admin;
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Question�rios de exemplo
    // ?????????????????????????????????????????????????????????????????????????
    private async Task SeedQuestionariosAsync(Guid adminId)
    {
        var totalExistente = await _context.Questionarios
            .CountDocumentsAsync(_ => true);

        if (totalExistente > 0)
        {
            _logger.LogInformation("??  Question�rios j� existem, pulando.");
            return;
        }

        // --- Question�rio 1: Satisfa��o ---
        var satisfacao = Questionario.Criar(
            titulo: "Pesquisa de Satisfa��o",
            descricao: "Avalie sua experi�ncia com nossos servi�os.",
            dataInicio: DateTime.UtcNow,
            dataFim: DateTime.UtcNow.AddDays(30),
            usuarioId: adminId
        );

        satisfacao.AdicionarPergunta(
            texto: "Como voc� avalia o atendimento?",
            ordem: 1,
            obrigatoria: true,
            opcoes: new[]
            {
                ("�timo", 1),
                ("Bom", 2),
                ("Regular", 3),
                ("Ruim", 4)
            }
        );

        satisfacao.AdicionarPergunta(
            texto: "Voc� recomendaria nossos servi�os?",
            ordem: 2,
            obrigatoria: true,
            opcoes: new[]
            {
                ("Sim, com certeza", 1),
                ("Provavelmente sim", 2),
                ("Provavelmente n�o", 3),
                ("N�o recomendaria", 4)
            }
        );

        satisfacao.AdicionarPergunta(
            texto: "Qual aspecto voc� mais valoriza?",
            ordem: 3,
            obrigatoria: false,
            opcoes: new[]
            {
                ("Qualidade", 1),
                ("Pre�o", 2),
                ("Prazo de entrega", 3),
                ("Suporte", 4)
            }
        );

        await _context.Questionarios.InsertOneAsync(satisfacao);
        _logger.LogInformation("?? Question�rio criado  ?  '{Titulo}'", satisfacao.Titulo);

        // --- Question�rio 2: Clima Organizacional ---
        var clima = Questionario.Criar(
            titulo: "Clima Organizacional",
            descricao: "Pesquisa interna de clima e cultura da empresa.",
            dataInicio: DateTime.UtcNow,
            dataFim: DateTime.UtcNow.AddDays(60),
            usuarioId: adminId
        );

        clima.AdicionarPergunta(
            texto: "Como voc� avalia o ambiente de trabalho?",
            ordem: 1,
            obrigatoria: true,
            opcoes: new[]
            {
                ("Excelente", 1),
                ("Bom", 2),
                ("Neutro", 3),
                ("Ruim", 4)
            }
        );

        clima.AdicionarPergunta(
            texto: "Voc� se sente valorizado pela empresa?",
            ordem: 2,
            obrigatoria: true,
            opcoes: new[]
            {
                ("Sempre", 1),
                ("Frequentemente", 2),
                ("Raramente", 3),
                ("Nunca", 4)
            }
        );

        await _context.Questionarios.InsertOneAsync(clima);
        _logger.LogInformation("?? Question�rio criado  ?  '{Titulo}'", clima.Titulo);
    }
}
