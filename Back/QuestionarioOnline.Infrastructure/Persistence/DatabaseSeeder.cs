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
    var questionarios = new List<Questionario>();

    // Questionário 1
    var torcida = Questionario.Criar(
        titulo: "Pesquisa da Torcida Tricolor",
        descricao: "Queremos entender a percepção da torcida sobre o momento do Fluminense, desempenho do time e expectativas para a temporada.",
        dataInicio: DateTime.UtcNow,
        dataFim: DateTime.UtcNow.AddDays(30),
        usuarioId: adminId
    );

    torcida.AdicionarPergunta(
        texto: "Como você avalia o desempenho recente do Fluminense em campo?",
        ordem: 1,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Excelente", 1),
            ("Bom", 2),
            ("Regular", 3),
            ("Ruim", 4)
        }
    );

    torcida.AdicionarPergunta(
        texto: "Qual setor do time mais precisa de reforços?",
        ordem: 2,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Defesa", 1),
            ("Meio-campo", 2),
            ("Ataque", 3),
            ("Banco / elenco", 4)
        }
    );

    torcida.AdicionarPergunta(
        texto: "Você acredita que o clube está competitivo para disputar títulos nesta temporada?",
        ordem: 3,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Sim, totalmente", 1),
            ("Tem chances", 2),
            ("Difícil, mas possível", 3),
            ("Não", 4)
        }
    );

    questionarios.Add(torcida);

    // Questionário 2
    var saf = Questionario.Criar(
        titulo: "Opinião da Torcida sobre a SAF",
        descricao: "Pesquisa sobre a visão do torcedor em relação à SAF, gestão do futebol, investimentos e futuro do Fluminense.",
        dataInicio: DateTime.UtcNow,
        dataFim: DateTime.UtcNow.AddDays(45),
        usuarioId: adminId
    );

    saf.AdicionarPergunta(
        texto: "Você é favorável à adoção de um modelo de SAF no Fluminense?",
        ordem: 1,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Sim", 1),
            ("Talvez, depende do projeto", 2),
            ("Não", 3),
            ("Não tenho opinião formada", 4)
        }
    );

    saf.AdicionarPergunta(
        texto: "Qual seria o principal benefício de uma SAF no clube?",
        ordem: 2,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Mais investimento no futebol", 1),
            ("Gestão profissional", 2),
            ("Redução de dívidas", 3),
            ("Melhor planejamento de longo prazo", 4)
        }
    );

    saf.AdicionarPergunta(
        texto: "Qual é sua maior preocupação em relação à SAF?",
        ordem: 3,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Perda de identidade do clube", 1),
            ("Falta de transparência", 2),
            ("Controle externo do futebol", 3),
            ("Promessas sem resultado", 4)
        }
    );

    questionarios.Add(saf);

    // Questionário 3
    var futebol = Questionario.Criar(
        titulo: "Avaliação do Futebol do Fluminense",
        descricao: "Pesquisa voltada para analisar elenco, treinador, estilo de jogo e principais dificuldades do time.",
        dataInicio: DateTime.UtcNow,
        dataFim: DateTime.UtcNow.AddDays(60),
        usuarioId: adminId
    );

    futebol.AdicionarPergunta(
        texto: "Como você avalia o trabalho da comissão técnica até aqui?",
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

    futebol.AdicionarPergunta(
        texto: "Qual tem sido o maior problema do time atualmente?",
        ordem: 2,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Falta de regularidade", 1),
            ("Baixo poder ofensivo", 2),
            ("Problemas defensivos", 3),
            ("Elenco curto", 4)
        }
    );

    futebol.AdicionarPergunta(
        texto: "O estilo de jogo atual do time te agrada?",
        ordem: 3,
        obrigatoria: true,
        opcoes: new[]
        {
            ("Sim, bastante", 1),
            ("Em parte", 2),
            ("Pouco", 3),
            ("Não", 4)
        }
    );

    questionarios.Add(futebol);

    foreach (var questionario in questionarios)
    {
        var filtro = Builders<Questionario>.Filter.Eq(q => q.Titulo, questionario.Titulo);
        var existente = await _context.Questionarios.Find(filtro).AnyAsync();

        if (existente)
        {
            _logger.LogInformation("Questionário já existe, pulando: {Titulo}", questionario.Titulo);
            continue;
        }

        await _context.Questionarios.InsertOneAsync(questionario);
        _logger.LogInformation("Questionário criado com sucesso: {Titulo}", questionario.Titulo);
    }
}
}
