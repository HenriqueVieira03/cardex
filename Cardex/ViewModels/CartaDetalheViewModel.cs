using System.Collections.ObjectModel;
using Cardex.Data;
using Cardex.Models;
using Cardex.Services;
using Cardex.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

// Indicador de uma variante que existe para a carta, e se eu tenho.
public record VarianteInfo(string Nome, bool Tenho)
{
    public string Texto => Tenho ? $"✓ {Nome}" : Nome;
}

// Uma linha da lista "Meus exemplares".
public record ExemplarItem(Exemplar Exemplar)
{
    public string Descricao => $"{Exemplar.Variante.Nome()} · {Exemplar.Condicao} · {Exemplar.Idioma.Nome()}";

    public string Quantidade => $"x{Exemplar.Quantidade}";

    public bool ParaTroca => Exemplar.ParaTroca;
}

public partial class CartaDetalheViewModel : ObservableObject, IRecebeParametro
{
    private readonly CartaDao _cartaDao;
    private readonly ExemplarDao _exemplarDao;
    private readonly NavegacaoService _navegacao;
    private readonly IAlertaService _alerta;

    private int _cartaId;
    private Carta? _carta;

    public CartaDetalheViewModel(CartaDao cartaDao, ExemplarDao exemplarDao, NavegacaoService navegacao, IAlertaService alerta)
    {
        _cartaDao = cartaDao;
        _exemplarDao = exemplarDao;
        _navegacao = navegacao;
        _alerta = alerta;
    }

    [ObservableProperty]
    public partial string Titulo { get; set; } = "";

    [ObservableProperty]
    public partial string Nome { get; set; } = "";

    [ObservableProperty]
    public partial string Numero { get; set; } = "";

    [ObservableProperty]
    public partial string Raridade { get; set; } = "";

    [ObservableProperty]
    public partial string Tipo { get; set; } = "";

    [ObservableProperty]
    public partial string? ImagemUrl { get; set; }

    [ObservableProperty]
    public partial string Origem { get; set; } = "";

    // Só cartas personalizadas podem ser excluídas.
    [ObservableProperty]
    public partial bool PodeExcluir { get; set; }

    [ObservableProperty]
    public partial bool SemExemplares { get; set; }

    public ObservableCollection<VarianteInfo> Variantes { get; } = [];

    public ObservableCollection<ExemplarItem> Exemplares { get; } = [];

    public Task ReceberParametroAsync(object? parametro)
    {
        if (parametro is int cartaId)
            _cartaId = cartaId;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        _carta = await _cartaDao.ObterPorIdAsync(_cartaId);
        if (_carta is null)
            return;

        Numero = _carta.EhPromo ? $"Promo {_carta.NumeroFormatado}" : _carta.NumeroFormatado;
        Titulo = $"{Numero} · {_carta.Nome}";
        Nome = _carta.Nome;
        Raridade = _carta.Raridade.Nome() + (_carta.EhSecreta ? " · secret rare" : "");
        Tipo = _carta.Tipo.Nome();
        ImagemUrl = _carta.ImagemUrl;
        PodeExcluir = _carta.Personalizada;
        Origem = _carta.Personalizada
            ? "Carta personalizada (cadastrada por você)."
            : "Carta do catálogo: pode ser editada, mas não excluída.";

        var exemplares = await _exemplarDao.ObterPorCartaAsync(_cartaId);

        Variantes.Clear();
        foreach (var variante in _carta.VariantesExistentes())
            Variantes.Add(new VarianteInfo(variante.Nome(), exemplares.Any(e => e.Variante == variante)));

        Exemplares.Clear();
        foreach (var exemplar in exemplares)
            Exemplares.Add(new ExemplarItem(exemplar));
        SemExemplares = Exemplares.Count == 0;
    }

    [RelayCommand]
    private Task EditarCartaAsync()
    {
        if (_carta is null)
            return Task.CompletedTask;
        return _navegacao.AbrirModalAsync<CartaFormPage>(new CartaFormParametro(_carta.ColecaoId, _carta.Id));
    }

    [RelayCommand]
    private async Task ExcluirCartaAsync()
    {
        if (_carta is null || !_carta.Personalizada)
            return;

        int exemplares = await _cartaDao.ContarExemplaresAsync(_carta.Id);
        bool confirmou = await _alerta.ConfirmarAsync(
            "Excluir carta",
            $"Excluir \"{_carta.Nome}\"? {(exemplares > 0 ? $"Os seus {exemplares} exemplares desta carta também serão apagados. " : "")}" +
            "Esta ação não pode ser desfeita.",
            sim: "Excluir");
        if (!confirmou)
            return;

        await _cartaDao.ExcluirAsync(_carta);
        await _navegacao.VoltarAsync();
        await _alerta.MostrarToastAsync("Carta excluída");
    }
}
