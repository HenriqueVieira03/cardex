using Cardex.Data;
using Cardex.Models;
using Cardex.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

// Item do Picker de raridade: o valor do enum + o nome em português que aparece na tela.
public record OpcaoRaridade(Raridade Valor, string Nome);

// Formulário modal de carta (criar promo/carta personalizada, ou editar qualquer carta).
public partial class CartaFormViewModel : ObservableObject, IRecebeParametro
{
    private readonly CartaDao _cartaDao;
    private readonly NavegacaoService _navegacao;
    private readonly IAlertaService _alerta;

    private int _colecaoId;
    private Carta? _emEdicao;
    private (int Numero, bool EhPromo)? _numeroDuplicado;
    private readonly HashSet<string> _editados = [];

    public CartaFormViewModel(CartaDao cartaDao, NavegacaoService navegacao, IAlertaService alerta)
    {
        _cartaDao = cartaDao;
        _navegacao = navegacao;
        _alerta = alerta;
        Raridade = Raridades[0];
    }

    public List<OpcaoRaridade> Raridades { get; } =
        Enum.GetValues<Raridade>().Select(r => new OpcaoRaridade(r, r.Nome())).ToList();

    [ObservableProperty]
    public partial string Titulo { get; set; } = "Nova carta";

    [ObservableProperty]
    public partial string Nome { get; set; } = "";

    [ObservableProperty]
    public partial string Numero { get; set; } = "";

    [ObservableProperty]
    public partial OpcaoRaridade Raridade { get; set; }

    // Ligado ao RadioButtonGroup.SelectedValue (os RadioButtons têm Value = cada TipoCarta).
    [ObservableProperty]
    public partial object Tipo { get; set; } = TipoCarta.Pokemon;

    [ObservableProperty]
    public partial bool TemNormal { get; set; } = true;

    [ObservableProperty]
    public partial bool TemFoil { get; set; }

    [ObservableProperty]
    public partial bool TemReverse { get; set; }

    [ObservableProperty]
    public partial bool EhPromo { get; set; }

    [ObservableProperty]
    public partial string ImagemUrl { get; set; } = "";

    // Só tem valor quando a URL é válida; a pré-visualização usa esta propriedade.
    [ObservableProperty]
    public partial string? PreVisualizacao { get; set; }

    [ObservableProperty]
    public partial string ErroNome { get; set; } = "";

    [ObservableProperty]
    public partial string ErroNumero { get; set; } = "";

    [ObservableProperty]
    public partial string ErroVariantes { get; set; } = "";

    [ObservableProperty]
    public partial string ErroImagem { get; set; } = "";

    public async Task ReceberParametroAsync(object? parametro)
    {
        if (parametro is not CartaFormParametro p)
            return;

        _colecaoId = p.ColecaoId;
        if (p.CartaId is not int cartaId)
            return;

        _emEdicao = await _cartaDao.ObterPorIdAsync(cartaId);
        if (_emEdicao is null)
            return;

        Titulo = "Editar carta";
        Nome = _emEdicao.Nome;
        Numero = _emEdicao.Numero.ToString();
        Raridade = Raridades.First(r => r.Valor == _emEdicao.Raridade);
        Tipo = _emEdicao.Tipo;
        TemNormal = _emEdicao.TemNormal;
        TemFoil = _emEdicao.TemFoil;
        TemReverse = _emEdicao.TemReverse;
        EhPromo = _emEdicao.EhPromo;
        ImagemUrl = _emEdicao.ImagemUrl ?? "";
        Validar();
    }

    partial void OnNomeChanged(string value) => Editou(nameof(Nome));
    partial void OnNumeroChanged(string value) => Editou(nameof(Numero));
    partial void OnTemNormalChanged(bool value) => Editou("Variantes");
    partial void OnTemFoilChanged(bool value) => Editou("Variantes");
    partial void OnTemReverseChanged(bool value) => Editou("Variantes");
    partial void OnEhPromoChanged(bool value) => Editou(nameof(Numero)); // muda o grupo do número único

    partial void OnImagemUrlChanged(string value)
    {
        PreVisualizacao = UrlValida(value) ? value.Trim() : null;
        Editou(nameof(ImagemUrl));
    }

    private void Editou(string campo)
    {
        _editados.Add(campo);
        Validar();
    }

    // ---- Regras ----
    private string RegraNome() => string.IsNullOrWhiteSpace(Nome) ? "Informe o nome da carta." : "";

    private string RegraNumero()
    {
        if (!int.TryParse(Numero, out int n) || n <= 0)
            return "Informe um número inteiro maior que zero.";
        if (_numeroDuplicado == (n, EhPromo))
            return EhPromo ? "Já existe uma promo com este número." : "Já existe uma carta com este número nesta coleção.";
        return "";
    }

    private string RegraVariantes() =>
        TemNormal || TemFoil || TemReverse ? "" : "Marque pelo menos uma variante.";

    private string RegraImagem() =>
        ImagemUrl.Trim().Length == 0 || UrlValida(ImagemUrl) ? "" : "Informe um endereço que comece com http:// ou https://.";

    private static bool UrlValida(string texto) =>
        Uri.TryCreate(texto.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private void Validar()
    {
        ErroNome = Mostrar(nameof(Nome), RegraNome());
        ErroNumero = Mostrar(nameof(Numero), RegraNumero());
        ErroVariantes = Mostrar("Variantes", RegraVariantes());
        ErroImagem = Mostrar(nameof(ImagemUrl), RegraImagem());
        SalvarCommand.NotifyCanExecuteChanged();
    }

    private string Mostrar(string campo, string erro) => _editados.Contains(campo) || _emEdicao is not null ? erro : "";

    private bool PodeSalvar() =>
        RegraNome() == "" && RegraNumero() == "" && RegraVariantes() == "" && RegraImagem() == "";

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        int numero = int.Parse(Numero);

        // Número único na coleção (por grupo: promos têm numeração própria). Precisa do banco.
        if (await _cartaDao.NumeroExisteAsync(_colecaoId, numero, EhPromo, ignorarId: _emEdicao?.Id ?? 0))
        {
            _numeroDuplicado = (numero, EhPromo);
            _editados.Add(nameof(Numero));
            Validar();
            return;
        }

        var carta = _emEdicao ?? new Carta { ColecaoId = _colecaoId, Personalizada = true };
        carta.Nome = Nome.Trim();
        carta.Numero = numero;
        carta.Raridade = Raridade.Valor;
        carta.Tipo = (TipoCarta)Tipo;
        carta.TemNormal = TemNormal;
        carta.TemFoil = TemFoil;
        carta.TemReverse = TemReverse;
        carta.EhPromo = EhPromo;
        carta.ImagemUrl = string.IsNullOrWhiteSpace(ImagemUrl) ? null : ImagemUrl.Trim();

        if (_emEdicao is null)
            await _cartaDao.InserirAsync(carta);   // o DAO calcula EhSecreta
        else
            await _cartaDao.AtualizarAsync(carta);

        await _navegacao.FecharModalAsync();
        await _alerta.MostrarToastAsync("Carta salva");
    }

    [RelayCommand]
    private Task CancelarAsync() => _navegacao.FecharModalAsync();
}
