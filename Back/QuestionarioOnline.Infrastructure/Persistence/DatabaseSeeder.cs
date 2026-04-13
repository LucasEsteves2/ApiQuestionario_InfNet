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

        _logger.LogInformation("? Seed concluído.");
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Admin
    // ?????????????????????????????????????????????????????????????????????????
    private async Task<Usuario> SeedAdminAsync()
    {
        const string adminEmail = "admin@questionario.com";

        var filter = Builders<Usuario>.Filter.Eq("email", adminEmail);
        var existente = await _context.Usuarios
            .Find(filter)
            .FirstOrDefaultAsync();

        if (existente is not null)
        {
            _logger.LogInformation("??  Admin já existe, pulando.");
            return existente;
        }

        var admin = new Usuario(
            nome: "Administrador",
            email: Email.Create(adminEmail),
            senhaHash: BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            role: UsuarioRole.Admin
        );

        await _context.Usuarios.InsertOneAsync(admin);
        _logger.LogInformation("?? Admin criado  ?  {Email}  |  senha: Admin@123", adminEmail);

        // Cria também um analista de exemplo
        var analista = new Usuario(
            nome: "Analista Exemplo",
            email: Email.Create("analista@questionario.com"),
            senhaHash: BCrypt.Net.BCrypt.HashPassword("Analista@123"),
            role: UsuarioRole.Analista
        );

        await _context.Usuarios.InsertOneAsync(analista);
        _logger.LogInformation("?? Analista criado  ?  analista@questionario.com  |  senha: Analista@123");

        return admin;
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Questionários de exemplo
    // ?????????????????????????????????????????????????????????????????????????
    private async Task SeedQuestionariosAsync(Guid adminId)
    {
        var totalExistente = await _context.Questionarios
            .CountDocumentsAsync(_ => true);

        if (totalExistente > 0)
        {
            _logger.LogInformation("??  Questionários já existem, pulando.");
            return;
        }

        // --- Questionário 1: Satisfação ---
        var satisfacao = Questionario.Criar(
            titulo: "Pesquisa de Satisfação",
            descricao: "Avalie sua experiência com nossos serviços.",
            dataInicio: DateTime.UtcNow,
            dataFim: DateTime.UtcNow.AddDays(30),
            usuarioId: adminId
        );

        satisfacao.AdicionarPergunta(
            texto: "Como você avalia o atendimento?",
            ordem: 1,
            obrigatoria: true,
            opcoes: new[]
            {
                ("Ótimo", 1),
                ("Bom", 2),
                ("Regular", 3),
                ("Ruim", 4)
            }
        );

        satisfacao.AdicionarPergunta(
            texto: "Você recomendaria nossos serviços?",
            ordem: 2,
            obrigatoria: true,
            opcoes: new[]
            {
                ("Sim, com certeza", 1),
                ("Provavelmente sim", 2),
                ("Provavelmente não", 3),
                ("Não recomendaria", 4)
            }
        );

        satisfacao.AdicionarPergunta(
            texto: "Qual aspecto você mais valoriza?",
            ordem: 3,
            obrigatoria: false,
            opcoes: new[]
            {
                ("Qualidade", 1),
                ("Preço", 2),
                ("Prazo de entrega", 3),
                ("Suporte", 4)
            }
        );

        await _context.Questionarios.InsertOneAsync(satisfacao);
        _logger.LogInformation("?? Questionário criado  ?  '{Titulo}'", satisfacao.Titulo);

        // --- Questionário 2: Clima Organizacional ---
        var clima = Questionario.Criar(
            titulo: "Clima Organizacional",
            descricao: "Pesquisa interna de clima e cultura da empresa.",
            dataInicio: DateTime.UtcNow,
            dataFim: DateTime.UtcNow.AddDays(60),
            usuarioId: adminId
        );

        clima.AdicionarPergunta(
            texto: "Como você avalia o ambiente de trabalho?",
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
            texto: "Você se sente valorizado pela empresa?",
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
        _logger.LogInformation("?? Questionário criado  ?  '{Titulo}'", clima.Titulo);
    }
}
