using Cardex.Data;
using Cardex.Models;
using Cardex.Services;
using Cardex.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

public partial class InicioViewModel : ObservableObject
{
    private readonly SeedService _seed;
    private readonly ColecaoDao _colecaoDao;
    private readonly CartaDao _cartaDao;
    private readonly ExemplarDao _exemplarDao;
    private readonly ProgressoService _progresso;
    private readonly NavegacaoService _navegacao;
    private readonly IAlertaService _alerta;

    private Colecao? _favorita;

    public InicioViewModel(SeedService seed, ColecaoDao colecaoDao, CartaDao cartaDao, ExemplarDao exemplarDao,
                           ProgressoService progresso, NavegacaoService navegacao, IAlertaService alerta)
    {
        _seed = seed;
        _colecaoDao = colecaoDao;
        _cartaDao = cartaDao;
        _exemplarDao = exemplarDao;
        _progresso = progresso;
        _navegacao = navegacao;
        _alerta = alerta;
    }

    [ObservableProperty]
    public partial bool Carregando { get; set; }

    // ---- Cartão da coleção favorita ----
    [ObservableProperty]
    public partial bool TemFavorita { get; set; }

    [ObservableProperty]
    public partial string NomeFavorita { get; set; } = "";

    [ObservableProperty]
    public partial string CartasUnicasFavorita { get; set; } = "";

    [ObservableProperty]
    public partial Progresso ProgressoBase { get; set; } = new(0, 0);

    [ObservableProperty]
    public partial Progresso ProgressoCompleto { get; set; } = new(0, 0);

    [ObservableProperty]
    public partial Progresso ProgressoMaster { get; set; } = new(0, 0);

    // ---- Resumo geral (todas as coleções) ----
    [ObservableProperty]
    public partial int TotalExemplares { get; set; }

    [ObservableProperty]
    public partial int CartasUnicas { get; set; }

    [ObservableProperty]
    public partial int Repetidas { get; set; }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            await _seed.ImportarSeNecessarioAsync(); // só faz algo na primeira execução

            _favorita = await _colecaoDao.ObterFavoritaAsync();
            TemFavorita = _favorita is not null;
            if (_favorita is not null)
            {
                var cartas = await _cartaDao.ObterPorColecaoAsync(_favorita.Id);
                var exemplares = await _exemplarDao.ObterPorColecaoAsync(_favorita.Id);

                NomeFavorita = _favorita.Nome;
                CartasUnicasFavorita = $"{_progresso.CartasUnicas(exemplares)} cartas únicas de {_favorita.TotalCartas}";
                ProgressoBase = _progresso.SetBase(_favorita, cartas, exemplares);
                ProgressoCompleto = _progresso.SetCompleto(_favorita, cartas, exemplares);
                ProgressoMaster = _progresso.MasterSet(_favorita, cartas, exemplares);
            }

            var todos = await _exemplarDao.ObterTodosAsync();
            TotalExemplares = _progresso.TotalExemplares(todos);
            CartasUnicas = _progresso.CartasUnicas(todos);
            Repetidas = _progresso.Repetidas(todos);
        }
        catch (Exception ex)
        {
            await _alerta.AvisarAsync("Erro ao carregar", ex.Message);
        }
        finally
        {
            Carregando = false;
        }
    }

    // ---- Atalhos (Grid 2x2) ----
    [RelayCommand]
    private Task AbrirColecoesAsync() => _navegacao.IrParaAsync<ColecoesPage>();

    [RelayCommand]
    private Task AbrirMinhasCartasAsync() => _navegacao.IrParaAsync<MinhasCartasPage>();

    [RelayCommand]
    private Task AbrirRepetidasAsync()
    {
        // A aba Repetidas da coleção (TabbedPage) chega nas etapas 3 e 5.
        return _alerta.MostrarToastAsync("As repetidas chegam na etapa 5.");
    }

    [RelayCommand]
    private Task NovaColecaoAsync() => _navegacao.AbrirModalAsync<ColecaoFormPage>();
}
