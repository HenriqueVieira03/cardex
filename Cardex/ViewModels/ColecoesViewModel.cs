using System.Collections.ObjectModel;
using Cardex.Data;
using Cardex.Services;
using Cardex.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

public partial class ColecoesViewModel : ObservableObject
{
    private readonly ColecaoDao _colecaoDao;
    private readonly CartaDao _cartaDao;
    private readonly ExemplarDao _exemplarDao;
    private readonly ProgressoService _progresso;
    private readonly NavegacaoService _navegacao;
    private readonly IAlertaService _alerta;

    // Lista completa vinda do banco; "Itens" é a parte filtrada que aparece na tela.
    private List<ColecaoItem> _todas = [];

    public ColecoesViewModel(ColecaoDao colecaoDao, CartaDao cartaDao, ExemplarDao exemplarDao,
                             ProgressoService progresso, NavegacaoService navegacao, IAlertaService alerta)
    {
        _colecaoDao = colecaoDao;
        _cartaDao = cartaDao;
        _exemplarDao = exemplarDao;
        _progresso = progresso;
        _navegacao = navegacao;
        _alerta = alerta;
    }

    // ObservableCollection avisa a ListView quando itens entram ou saem.
    public ObservableCollection<ColecaoItem> Itens { get; } = [];

    [ObservableProperty]
    public partial string TextoBusca { get; set; } = "";

    [ObservableProperty]
    public partial bool SemResultados { get; set; }

    [ObservableProperty]
    public partial string MensagemVazia { get; set; } = "";

    // Gerado pelo [ObservableProperty]: chamado a cada letra digitada na SearchBar.
    partial void OnTextoBuscaChanged(string value) => Filtrar();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        var colecoes = await _colecaoDao.ObterTodosAsync();
        var itens = new List<ColecaoItem>();
        foreach (var colecao in colecoes)
        {
            var cartas = await _cartaDao.ObterPorColecaoAsync(colecao.Id);
            var exemplares = await _exemplarDao.ObterPorColecaoAsync(colecao.Id);
            var completo = _progresso.SetCompleto(colecao, cartas, exemplares);
            itens.Add(new ColecaoItem(colecao, completo.Texto));
        }
        _todas = itens;
        Filtrar();
    }

    // Filtro em memória por nome ou código, sem diferenciar maiúsculas/minúsculas.
    private void Filtrar()
    {
        string termo = TextoBusca.Trim();
        var filtradas = _todas.Where(i =>
            termo.Length == 0 ||
            i.Nome.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ||
            i.Codigo.Contains(termo, StringComparison.CurrentCultureIgnoreCase));

        Itens.Clear();
        foreach (var item in filtradas)
            Itens.Add(item);

        SemResultados = Itens.Count == 0;
        MensagemVazia = _todas.Count == 0
            ? "Nenhuma coleção cadastrada. Toque em \"Nova coleção\" para começar."
            : $"Nenhuma coleção encontrada para \"{termo}\".";
    }

    [RelayCommand]
    private Task NovaAsync() => _navegacao.AbrirModalAsync<ColecaoFormPage>();

    // O parâmetro é o Id: o formulário carrega a coleção e entra em modo edição.
    [RelayCommand]
    private Task EditarAsync(ColecaoItem item) => _navegacao.AbrirModalAsync<ColecaoFormPage>(item.Colecao.Id);

    [RelayCommand]
    private async Task ExcluirAsync(ColecaoItem item)
    {
        var (cartas, exemplares) = await _colecaoDao.ContarDependentesAsync(item.Colecao.Id);
        bool confirmou = await _alerta.ConfirmarAsync(
            "Excluir coleção",
            $"Excluir \"{item.Nome}\"?\n\nSerão apagados também {cartas} cartas e {exemplares} exemplares. " +
            "Esta ação não pode ser desfeita.",
            sim: "Excluir");
        if (!confirmou)
            return;

        await _colecaoDao.ExcluirAsync(item.Colecao);
        await _alerta.MostrarToastAsync("Coleção excluída");
        await CarregarAsync();
    }

    [RelayCommand]
    private Task AbrirAsync(ColecaoItem item)
    {
        // A Coleção (TabbedPage: Fichário · Repetidas · Estatísticas) chega na etapa 3.
        return _alerta.MostrarToastAsync($"O fichário de {item.Codigo} chega na etapa 3.");
    }
}
