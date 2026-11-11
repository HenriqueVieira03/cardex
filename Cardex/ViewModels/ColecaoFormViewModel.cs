using Cardex.Data;
using Cardex.Models;
using Cardex.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

// Formulário modal de coleção: o mesmo ViewModel serve para criar e para editar.
public partial class ColecaoFormViewModel : ObservableObject, IRecebeParametro
{
    private readonly ColecaoDao _colecaoDao;
    private readonly NavegacaoService _navegacao;
    private readonly IAlertaService _alerta;

    private Colecao? _emEdicao;              // null = criando uma coleção nova
    private string? _codigoDuplicado;        // último código recusado por já existir
    private readonly HashSet<string> _editados = []; // campos que o usuário já mexeu

    public ColecaoFormViewModel(ColecaoDao colecaoDao, NavegacaoService navegacao, IAlertaService alerta)
    {
        _colecaoDao = colecaoDao;
        _navegacao = navegacao;
        _alerta = alerta;
    }

    [ObservableProperty]
    public partial string Titulo { get; set; } = "Nova coleção";

    // ---- Campos (Entry trabalha com texto; os números são convertidos na validação) ----
    [ObservableProperty]
    public partial string Nome { get; set; } = "";

    [ObservableProperty]
    public partial string NomeIngles { get; set; } = "";

    [ObservableProperty]
    public partial string Codigo { get; set; } = "";

    [ObservableProperty]
    public partial string TotalPrincipal { get; set; } = "";

    [ObservableProperty]
    public partial string TotalCartas { get; set; } = "";

    [ObservableProperty]
    public partial DateTime? DataLancamento { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial bool Favorita { get; set; }

    // ---- Mensagens de erro (Label vermelho abaixo de cada campo) ----
    [ObservableProperty]
    public partial string ErroNome { get; set; } = "";

    [ObservableProperty]
    public partial string ErroCodigo { get; set; } = "";

    [ObservableProperty]
    public partial string ErroTotalPrincipal { get; set; } = "";

    [ObservableProperty]
    public partial string ErroTotalCartas { get; set; } = "";

    // Chamado pelo NavegacaoService antes de abrir: parâmetro = Id da coleção (edição) ou null (nova).
    public async Task ReceberParametroAsync(object? parametro)
    {
        if (parametro is not int id)
            return;

        _emEdicao = await _colecaoDao.ObterPorIdAsync(id);
        if (_emEdicao is null)
            return;

        Titulo = "Editar coleção";
        Nome = _emEdicao.Nome;
        NomeIngles = _emEdicao.NomeIngles;
        Codigo = _emEdicao.Codigo;
        TotalPrincipal = _emEdicao.TotalPrincipal.ToString();
        TotalCartas = _emEdicao.TotalCartas.ToString();
        DataLancamento = _emEdicao.DataLancamento;
        Favorita = _emEdicao.Favorita;
        _editados.Clear(); // carregar os dados não conta como "mexer"
        Validar();
    }

    // Os métodos On...Changed são gerados pelo [ObservableProperty] e rodam a cada alteração.
    partial void OnNomeChanged(string value) => Editou(nameof(Nome));
    partial void OnCodigoChanged(string value) => Editou(nameof(Codigo));
    partial void OnTotalPrincipalChanged(string value) => Editou(nameof(TotalPrincipal));
    partial void OnTotalCartasChanged(string value) => Editou(nameof(TotalCartas));

    private void Editou(string campo)
    {
        _editados.Add(campo);
        Validar();
    }

    // ---- Regras de validação (cada uma devolve "" quando está tudo certo) ----
    private string RegraNome() =>
        string.IsNullOrWhiteSpace(Nome) ? "Informe o nome da coleção." : "";

    private string RegraCodigo()
    {
        string codigo = NormalizarCodigo(Codigo);
        if (codigo.Length == 0) return "Informe o código (ex.: ME05).";
        if (codigo == _codigoDuplicado) return "Já existe uma coleção com este código.";
        return "";
    }

    private string RegraTotalPrincipal() =>
        int.TryParse(TotalPrincipal, out int n) && n > 0 ? "" : "Informe um número inteiro maior que zero.";

    private string RegraTotalCartas()
    {
        if (!int.TryParse(TotalCartas, out int total) || total <= 0)
            return "Informe um número inteiro maior que zero.";
        if (int.TryParse(TotalPrincipal, out int principal) && total < principal)
            return "O total de cartas não pode ser menor que o total principal.";
        return "";
    }

    // Atualiza as mensagens (só dos campos já editados) e o estado do botão Salvar.
    private void Validar()
    {
        ErroNome = Mostrar(nameof(Nome), RegraNome());
        ErroCodigo = Mostrar(nameof(Codigo), RegraCodigo());
        ErroTotalPrincipal = Mostrar(nameof(TotalPrincipal), RegraTotalPrincipal());
        ErroTotalCartas = Mostrar(nameof(TotalCartas), RegraTotalCartas());
        SalvarCommand.NotifyCanExecuteChanged(); // reavalia PodeSalvar
    }

    private string Mostrar(string campo, string erro) => _editados.Contains(campo) || _emEdicao is not null ? erro : "";

    // O botão fica desabilitado enquanto QUALQUER regra falhar (mesmo de campo ainda não mexido).
    private bool PodeSalvar() =>
        RegraNome() == "" && RegraCodigo() == "" && RegraTotalPrincipal() == "" && RegraTotalCartas() == "";

    private static string NormalizarCodigo(string codigo) => codigo.Trim().ToUpperInvariant();

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        string codigo = NormalizarCodigo(Codigo);

        // Código único: só dá para conferir no banco, por isso fica aqui e não em RegraCodigo.
        if (await _colecaoDao.CodigoExisteAsync(codigo, ignorarId: _emEdicao?.Id ?? 0))
        {
            _codigoDuplicado = codigo;
            _editados.Add(nameof(Codigo));
            Validar();
            return;
        }

        var colecao = _emEdicao ?? new Colecao();
        colecao.Nome = Nome.Trim();
        colecao.NomeIngles = NomeIngles.Trim();
        colecao.Codigo = codigo;
        colecao.TotalPrincipal = int.Parse(TotalPrincipal);
        colecao.TotalCartas = int.Parse(TotalCartas);
        colecao.DataLancamento = DataLancamento ?? DateTime.Today;
        colecao.Favorita = Favorita;

        if (_emEdicao is null)
            await _colecaoDao.InserirAsync(colecao);
        else
            await _colecaoDao.AtualizarAsync(colecao);

        await _navegacao.FecharModalAsync();
        await _alerta.MostrarToastAsync("Coleção salva");
    }

    [RelayCommand]
    private Task CancelarAsync() => _navegacao.FecharModalAsync();
}
