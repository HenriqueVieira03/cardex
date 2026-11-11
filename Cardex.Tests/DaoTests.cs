using Cardex.Data;
using Cardex.Models;

namespace Cardex.Tests;

// Testa os DAOs contra um arquivo SQLite temporário (um banco novo por teste).
public class DaoTests : IAsyncLifetime
{
    private readonly string _caminho = Path.Combine(Path.GetTempPath(), $"cardex-teste-{Guid.NewGuid()}.db3");
    private ColecaoDao _colecaoDao = null!;
    private CartaDao _cartaDao = null!;
    private ExemplarDao _exemplarDao = null!;
    private Colecao _colecao = null!;

    public async Task InitializeAsync()
    {
        var database = new CardexDatabase(_caminho);
        _colecaoDao = new ColecaoDao(database);
        _cartaDao = new CartaDao(database);
        _exemplarDao = new ExemplarDao(database);

        _colecao = new Colecao { Nome = "Teste", Codigo = "TST", TotalPrincipal = 84, TotalCartas = 120 };
        await _colecaoDao.InserirAsync(_colecao);
    }

    public Task DisposeAsync()
    {
        SQLite.SQLiteAsyncConnection.ResetPool(); // fecha as conexões para poder apagar o arquivo
        File.Delete(_caminho);
        return Task.CompletedTask;
    }

    private async Task<Carta> NovaCartaAsync(int numero, bool personalizada = false, bool ehPromo = false)
    {
        var carta = new Carta { ColecaoId = _colecao.Id, Numero = numero, Nome = $"Carta {numero}", TemNormal = true, Personalizada = personalizada, EhPromo = ehPromo };
        await _cartaDao.InserirAsync(carta);
        return carta;
    }

    [Fact]
    public async Task Inserir_preenche_Id_e_calcula_EhSecreta()
    {
        var comum = await NovaCartaAsync(10);
        var secreta = await NovaCartaAsync(100);

        Assert.True(comum.Id > 0);
        Assert.False((await _cartaDao.ObterPorIdAsync(comum.Id))!.EhSecreta);
        Assert.True((await _cartaDao.ObterPorIdAsync(secreta.Id))!.EhSecreta);
    }

    [Fact]
    public async Task Excluir_colecao_apaga_cartas_e_exemplares_em_cascata()
    {
        var carta = await NovaCartaAsync(1);
        await _exemplarDao.InserirAsync(new Exemplar { CartaId = carta.Id, Quantidade = 2 });
        await _exemplarDao.InserirAsync(new Exemplar { CartaId = carta.Id, Variante = Variante.Reverse });

        var dependentes = await _colecaoDao.ContarDependentesAsync(_colecao.Id);
        int apagados = await _colecaoDao.ExcluirAsync(_colecao);

        Assert.Equal((1, 2), dependentes);
        Assert.Equal(4, apagados); // 2 exemplares + 1 carta + 1 coleção
        Assert.Empty(await _cartaDao.ObterTodosAsync());
        Assert.Empty(await _exemplarDao.ObterTodosAsync());
    }

    [Fact]
    public async Task Carta_do_catalogo_nao_pode_ser_excluida()
    {
        var importada = await NovaCartaAsync(1, personalizada: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _cartaDao.ExcluirAsync(importada));
    }

    [Fact]
    public async Task Carta_personalizada_e_excluida_com_seus_exemplares()
    {
        var minha = await NovaCartaAsync(200, personalizada: true);
        await _exemplarDao.InserirAsync(new Exemplar { CartaId = minha.Id });

        await _cartaDao.ExcluirAsync(minha);

        Assert.Null(await _cartaDao.ObterPorIdAsync(minha.Id));
        Assert.Empty(await _exemplarDao.ObterPorCartaAsync(minha.Id));
    }

    [Fact]
    public async Task Numero_e_unico_por_grupo_promo_tem_numeracao_propria()
    {
        var carta88 = await NovaCartaAsync(88);

        Assert.True(await _cartaDao.NumeroExisteAsync(_colecao.Id, 88, ehPromo: false));
        Assert.False(await _cartaDao.NumeroExisteAsync(_colecao.Id, 88, ehPromo: true));
        Assert.False(await _cartaDao.NumeroExisteAsync(_colecao.Id, 88, ehPromo: false, ignorarId: carta88.Id));
    }

    [Fact]
    public async Task Busca_por_numero_encontra_045_e_por_nome_ignora_maiusculas()
    {
        await NovaCartaAsync(45);
        await NovaCartaAsync(46);

        var porNumero = await _cartaDao.BuscarPorNomeOuNumeroAsync(_colecao.Id, "45");
        var porNome = await _cartaDao.BuscarPorNomeOuNumeroAsync(_colecao.Id, "carta 4");

        Assert.Equal(45, Assert.Single(porNumero).Numero);
        Assert.Equal(2, porNome.Count);
    }

    [Fact]
    public async Task So_uma_colecao_fica_favorita()
    {
        var a = new Colecao { Nome = "A", Codigo = "A", Favorita = true };
        var b = new Colecao { Nome = "B", Codigo = "B", Favorita = true };
        await _colecaoDao.InserirAsync(a);
        await _colecaoDao.InserirAsync(b);

        var favorita = await _colecaoDao.ObterFavoritaAsync();

        Assert.Equal("B", favorita!.Codigo);
        Assert.False((await _colecaoDao.ObterPorIdAsync(a.Id))!.Favorita);
    }

    [Fact]
    public async Task Editar_TotalPrincipal_recalcula_as_secretas()
    {
        var carta = await NovaCartaAsync(80);
        Assert.False((await _cartaDao.ObterPorIdAsync(carta.Id))!.EhSecreta);

        _colecao.TotalPrincipal = 70;
        await _colecaoDao.AtualizarAsync(_colecao);

        Assert.True((await _cartaDao.ObterPorIdAsync(carta.Id))!.EhSecreta);
    }

    [Fact]
    public async Task CodigoExiste_ignora_a_propria_colecao_na_edicao()
    {
        Assert.True(await _colecaoDao.CodigoExisteAsync("TST"));
        Assert.False(await _colecaoDao.CodigoExisteAsync("TST", ignorarId: _colecao.Id));
    }

    [Fact]
    public async Task ObterIgual_acha_exemplar_com_mesma_carta_variante_condicao_e_idioma()
    {
        var carta = await NovaCartaAsync(1);
        await _exemplarDao.InserirAsync(new Exemplar { CartaId = carta.Id, Variante = Variante.Normal, Condicao = Condicao.NM, Idioma = Idioma.Portugues });

        Assert.NotNull(await _exemplarDao.ObterIgualAsync(carta.Id, Variante.Normal, Condicao.NM, Idioma.Portugues));
        Assert.Null(await _exemplarDao.ObterIgualAsync(carta.Id, Variante.Normal, Condicao.SP, Idioma.Portugues));
    }
}
