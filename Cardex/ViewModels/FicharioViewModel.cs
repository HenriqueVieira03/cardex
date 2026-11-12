using System.Collections.ObjectModel;
using Cardex.Data;
using Cardex.Models;
using Cardex.Services;
using Cardex.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

public partial class FicharioViewModel : ObservableObject, IRecebeParametro
{
    private const string Todas = "Todas", Tenho = "Tenho", Faltam = "Faltam";

    private readonly ColecaoDao _colecaoDao;
    private readonly CartaDao _cartaDao;
    private readonly ExemplarDao _exemplarDao;
    private readonly NavegacaoService _navegacao;

    private int _colecaoId;
    private Colecao? _colecao;
    private Dictionary<int, int> _quantidades = []; // CartaId → total de exemplares
    private List<CartaFichario> _resultadoBusca = [];
    private int _ultimaBusca; // descarta respostas de buscas antigas

    public FicharioViewModel(ColecaoDao colecaoDao, CartaDao cartaDao, ExemplarDao exemplarDao, NavegacaoService navegacao)
    {
        _colecaoDao = colecaoDao;
        _cartaDao = cartaDao;
        _exemplarDao = exemplarDao;
        _navegacao = navegacao;
    }

    // Lista de grupos (a CollectionView usa IsGrouped="True").
    public ObservableCollection<GrupoCartas> Grupos { get; } = [];

    [ObservableProperty]
    public partial string TextoBusca { get; set; } = "";

    // Ligado ao RadioButtonGroup.SelectedValue: "Todas", "Tenho" ou "Faltam".
    [ObservableProperty]
    public partial string Situacao { get; set; } = Todas;

    [ObservableProperty]
    public partial string Resumo { get; set; } = "";

    [ObservableProperty]
    public partial bool SemResultados { get; set; }

    // Altura da carta (proporção 63 × 88), calculada pela View a partir da largura da tela.
    [ObservableProperty]
    public partial double AlturaCarta { get; set; } = 170;

    public Task ReceberParametroAsync(object? parametro)
    {
        if (parametro is int colecaoId)
            _colecaoId = colecaoId;
        return Task.CompletedTask; // os dados são carregados no OnAppearing da aba
    }

    partial void OnTextoBuscaChanged(string value) => _ = BuscarAsync();

    partial void OnSituacaoChanged(string value) => Agrupar();

    // Largura da grade dividida em 3 colunas, descontando os 2 espaços de 8 entre elas.
    public void AjustarTamanho(double larguraGrade)
    {
        double largura = (larguraGrade - 2 * 8) / 3;
        if (largura > 0)
            AlturaCarta = largura * 88 / 63;
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        _colecao = await _colecaoDao.ObterPorIdAsync(_colecaoId);
        if (_colecao is null)
            return;

        // Soma as quantidades por carta (várias linhas de exemplar podem ser da mesma carta).
        var exemplares = await _exemplarDao.ObterPorColecaoAsync(_colecaoId);
        _quantidades = exemplares
            .GroupBy(e => e.CartaId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Quantidade));

        await BuscarAsync();
    }

    // A busca vai ao banco (CartaDao.BuscarPorNomeOuNumeroAsync) a cada letra digitada.
    private async Task BuscarAsync()
    {
        int estaBusca = ++_ultimaBusca;
        var cartas = await _cartaDao.BuscarPorNomeOuNumeroAsync(_colecaoId, TextoBusca);
        if (estaBusca != _ultimaBusca)
            return; // o usuário já digitou outra letra; esta resposta ficou velha

        _resultadoBusca = cartas
            .Select(c => new CartaFichario(c, _quantidades.GetValueOrDefault(c.Id)))
            .ToList();
        Agrupar();
    }

    // Aplica o filtro Todas/Tenho/Faltam e separa nos grupos do fichário.
    private void Agrupar()
    {
        if (_colecao is null)
            return;

        var filtradas = Situacao switch
        {
            Tenho => _resultadoBusca.Where(c => c.Tenho),
            Faltam => _resultadoBusca.Where(c => !c.Tenho),
            _ => _resultadoBusca,
        };
        var lista = filtradas.ToList();

        int principal = _colecao.TotalPrincipal;
        var grupos = new[]
        {
            new GrupoCartas($"Coleção principal (001–{principal:000})",
                            lista.Where(c => !c.Carta.EhPromo && !c.Carta.EhSecreta)),
            new GrupoCartas($"Secret rares ({principal + 1:000}–{_colecao.TotalCartas:000})",
                            lista.Where(c => c.Carta.EhSecreta)),
            new GrupoCartas("Promos", lista.Where(c => c.Carta.EhPromo)),
        };

        Grupos.Clear();
        foreach (var grupo in grupos.Where(g => g.Count > 0)) // grupo vazio não aparece
            Grupos.Add(grupo);

        int tenho = _resultadoBusca.Count(c => c.Tenho);
        Resumo = $"{tenho} de {_resultadoBusca.Count} cartas";
        SemResultados = lista.Count == 0;
    }

    [RelayCommand]
    private Task AbrirCartaAsync(CartaFichario item) => _navegacao.IrParaAsync<CartaDetalhePage>(item.Carta.Id);

    [RelayCommand]
    private Task NovaCartaAsync() => _navegacao.AbrirModalAsync<CartaFormPage>(new CartaFormParametro(_colecaoId));
}
