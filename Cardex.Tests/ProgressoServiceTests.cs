using Cardex.Models;
using Cardex.Services;

namespace Cardex.Tests;

public class ProgressoServiceTests
{
    private readonly ProgressoService _servico = new();

    // Coleção pequena para os testes: 3 cartas principais, 1 secreta, 1 promo.
    private static readonly Colecao Colecao = new() { Id = 1, TotalPrincipal = 3, TotalCartas = 4 };

    private static readonly List<Carta> Cartas =
    [
        new() { Id = 1, Numero = 1, TemNormal = true, TemReverse = true },                // comum: N + R
        new() { Id = 2, Numero = 2, TemNormal = true, TemReverse = true },                // comum: N + R
        new() { Id = 3, Numero = 3, TemFoil = true },                                     // rara dupla: só F
        new() { Id = 4, Numero = 4, TemFoil = true, EhSecreta = true },                   // secreta: só F
        new() { Id = 5, Numero = 88, TemFoil = true, EhPromo = true },                    // promo: só F
    ];

    private static Exemplar Ex(int cartaId, Variante variante, int quantidade = 1,
                               Condicao condicao = Condicao.NM, Idioma idioma = Idioma.Portugues)
        => new() { CartaId = cartaId, Variante = variante, Quantidade = quantidade, Condicao = condicao, Idioma = idioma };

    [Fact]
    public void Sem_exemplares_tudo_zero()
    {
        Assert.Equal(new Progresso(0, 3), _servico.SetBase(Colecao, Cartas, []));
        Assert.Equal(new Progresso(0, 4), _servico.SetCompleto(Colecao, Cartas, []));
        Assert.Equal(new Progresso(0, 7), _servico.MasterSet(Colecao, Cartas, [])); // 2+2+1+1 + promo 1
    }

    [Fact]
    public void Set_base_conta_qualquer_variante_e_ignora_secretas_e_promos()
    {
        List<Exemplar> exemplares = [Ex(1, Variante.Reverse), Ex(2, Variante.Normal), Ex(4, Variante.Foil), Ex(5, Variante.Foil)];

        var progresso = _servico.SetBase(Colecao, Cartas, exemplares);

        Assert.Equal(new Progresso(2, 3), progresso);
        Assert.False(progresso.Completo);
    }

    [Fact]
    public void Set_completo_inclui_as_secretas()
    {
        List<Exemplar> exemplares = [Ex(1, Variante.Normal), Ex(2, Variante.Normal), Ex(3, Variante.Foil), Ex(4, Variante.Foil)];

        Assert.True(_servico.SetBase(Colecao, Cartas, exemplares).Completo);
        Assert.Equal(new Progresso(4, 4), _servico.SetCompleto(Colecao, Cartas, exemplares));
    }

    [Fact]
    public void Master_set_exige_cada_variante_e_as_promos()
    {
        List<Exemplar> semReverseNemPromo = [Ex(1, Variante.Normal), Ex(2, Variante.Normal), Ex(3, Variante.Foil), Ex(4, Variante.Foil)];
        List<Exemplar> tudo = [.. semReverseNemPromo, Ex(1, Variante.Reverse), Ex(2, Variante.Reverse), Ex(5, Variante.Foil)];

        Assert.Equal(new Progresso(4, 7), _servico.MasterSet(Colecao, Cartas, semReverseNemPromo));
        Assert.True(_servico.MasterSet(Colecao, Cartas, tudo).Completo);
    }

    [Fact]
    public void Master_set_ignora_variante_que_a_carta_nao_tem()
    {
        // A carta 3 não tem versão normal: um exemplar "normal" dela não conta.
        Assert.Equal(0, _servico.MasterSet(Colecao, Cartas, [Ex(3, Variante.Normal)]).Tenho);
    }

    [Fact]
    public void Repetidas_sao_copias_alem_da_primeira_por_carta_e_variante()
    {
        // Exemplo da especificação: 3 cópias da 045 normal = 2 repetidas,
        // mesmo divididas em condições e idiomas diferentes.
        List<Exemplar> exemplares =
        [
            Ex(45, Variante.Normal, 2, Condicao.NM),
            Ex(45, Variante.Normal, 1, Condicao.SP, Idioma.Ingles),
            Ex(45, Variante.Reverse, 1), // variante diferente: é a primeira dela, não é repetida
        ];

        Assert.Equal(2, _servico.Repetidas(exemplares));
        Assert.Equal(4, _servico.TotalExemplares(exemplares));
        Assert.Equal(1, _servico.CartasUnicas(exemplares));
    }

    [Fact]
    public void Progresso_formata_texto_e_fracao()
    {
        var p = new Progresso(57, 120);

        Assert.Equal("57/120", p.Texto);
        Assert.Equal(0.475, p.Fracao, precision: 3);
        Assert.Equal(0, new Progresso(0, 0).Fracao);
    }
}
