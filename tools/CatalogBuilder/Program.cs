// CatalogBuilder: gera Cardex/Resources/Raw/me05.json a partir da TCGdex (tcgdex.dev).
// Uso: dotnet run --project tools/CatalogBuilder [caminho-de-saida]
//
// Passos:
//  1. baixa os dados da coleção me05 em português ("pt") e em inglês ("en");
//  2. baixa cada carta (é o detalhe da carta que traz raridade, tipo e variantes);
//  3. junta: nome/imagem em português quando houver, inglês como reserva;
//  4. adiciona as promos conhecidas (Zarude e Slowpoke);
//  5. valida e grava o JSON.

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

Console.OutputEncoding = Encoding.UTF8;

const string Api = "https://api.tcgdex.net/v2";
const string SetId = "me05";

// Promos da coleção: ficam no set "mep" (MEP Black Star Promos) da TCGdex.
string[] promos = ["mep-088", "mep-086"]; // Zarude (Coleção Treinador Avançado), Slowpoke (blister com 3 boosters)

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("CardexCatalogBuilder/1.0");

var setPt = await BaixarAsync($"{Api}/pt/sets/{SetId}");
var setEn = await BaixarAsync($"{Api}/en/sets/{SetId}");
if (setPt is null || setEn is null)
    return Falhar("Não foi possível baixar a coleção.");

int totalPrincipal = setEn["cardCount"]!["official"]!.GetValue<int>();
int totalCartas = setEn["cardCount"]!["total"]!.GetValue<int>();

var cartas = new List<CartaJson>();
int nomesPt = 0, nomesEn = 0, imagensPt = 0, imagensEn = 0;

for (int numero = 1; numero <= totalCartas; numero++)
{
    string id = $"{SetId}-{numero:000}";
    Console.Write($"\rBaixando {id}...");
    var pt = await BaixarAsync($"{Api}/pt/cards/{id}");
    var en = await BaixarAsync($"{Api}/en/cards/{id}");
    if (en is null && pt is null)
        return Falhar($"Carta {id} não encontrada em nenhum idioma.");

    var carta = Montar(numero, pt, en, ehPromo: false);
    if (Texto(pt, "name") is not null) nomesPt++; else nomesEn++;
    if (Texto(pt, "image") is not null) imagensPt++; else if (carta.ImagemUrl is not null) imagensEn++;
    cartas.Add(carta);
}
Console.WriteLine();

foreach (var id in promos)
{
    var pt = await BaixarAsync($"{Api}/pt/cards/{id}");
    var en = await BaixarAsync($"{Api}/en/cards/{id}");
    int numero = int.Parse(id.Split('-')[1]);
    var carta = Montar(numero, pt, en, ehPromo: true);
    Console.WriteLine($"Promo {id}: {carta.Nome} ({(pt is null ? "só inglês" : "português")}, imagem: {(carta.ImagemUrl is null ? "não" : "sim")})");
    cartas.Add(carta);
}

// ---------- validação ----------
var erros = new List<string>();
var principais = cartas.Where(c => !c.EhPromo).ToList();
if (principais.Count != totalCartas) erros.Add($"Esperava {totalCartas} cartas, veio {principais.Count}.");
if (principais.Select(c => c.Numero).Distinct().Count() != principais.Count) erros.Add("Números repetidos na coleção.");
foreach (var c in cartas)
{
    if (string.IsNullOrWhiteSpace(c.Nome)) erros.Add($"Carta {c.Numero} sem nome.");
    if (string.IsNullOrWhiteSpace(c.Raridade)) erros.Add($"Carta {c.Numero} sem raridade.");
    if (!c.TemNormal && !c.TemFoil && !c.TemReverse) erros.Add($"Carta {c.Numero} sem nenhuma variante.");
}
if (erros.Count > 0)
    return Falhar(string.Join(Environment.NewLine, erros));

// ---------- relatório ----------
Console.WriteLine($"Nomes: {nomesPt} em português, {nomesEn} em inglês.");
Console.WriteLine($"Imagens: {imagensPt} em português, {imagensEn} em inglês.");
Console.WriteLine("Raridades:");
foreach (var g in principais.GroupBy(c => c.Raridade).OrderBy(g => g.Min(c => c.Numero)))
    Console.WriteLine($"  {g.Key,-24} {g.Count(),3}  ({g.Min(c => c.Numero):000}–{g.Max(c => c.Numero):000})");
Console.WriteLine("Variantes (normal/foil/reverse):");
foreach (var g in principais.GroupBy(c => $"{c.Raridade}: {(c.TemNormal ? "N" : "-")}{(c.TemFoil ? "F" : "-")}{(c.TemReverse ? "R" : "-")}"))
    Console.WriteLine($"  {g.Key,-32} {g.Count(),3}");

// ---------- gravação ----------
var serie = Texto(setPt, "serie", "name") ?? "Megaevolução";
var colecao = new ColecaoJson(
    Codigo: SetId.ToUpperInvariant(),
    Nome: $"{serie} — {Texto(setPt, "name")}",
    NomeIngles: Texto(setEn, "name") ?? "",
    TotalPrincipal: totalPrincipal,
    TotalCartas: totalCartas,
    DataLancamento: Texto(setEn, "releaseDate") ?? Texto(setPt, "releaseDate") ?? "",
    Cartas: cartas);

string saida = args.Length > 0 ? args[0] : Path.Combine(AcharRaiz(), "Cardex", "Resources", "Raw", "me05.json");
var opcoes = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // mantém acentos legíveis no arquivo
};
await File.WriteAllTextAsync(saida, JsonSerializer.Serialize(colecao, opcoes), new UTF8Encoding(false));
Console.WriteLine($"OK: {cartas.Count} cartas gravadas em {saida}");
return 0;

// ================= funções auxiliares =================

CartaJson Montar(int numero, JsonNode? pt, JsonNode? en, bool ehPromo)
{
    // Raridade e tipo: usa o inglês (nomes estáveis) e cai para o português se faltar.
    string? raridadeApi = Texto(en, "rarity") ?? Texto(pt, "rarity");
    string? categoria = Texto(en, "category") ?? Texto(pt, "category");
    var variantes = (en ?? pt)!["variants"];

    // A URL da TCGdex vem sem extensão; é preciso acrescentar /{qualidade}.{formato}.
    string? imagemBase = Texto(pt, "image") ?? Texto(en, "image");

    return new CartaJson(
        Numero: numero,
        Nome: Texto(pt, "name") ?? Texto(en, "name") ?? "",
        Raridade: MapearRaridade(raridadeApi, numero),
        Tipo: MapearTipo(categoria, numero),
        ImagemUrl: imagemBase is null ? null : imagemBase + "/high.webp",
        TemNormal: variantes?["normal"]?.GetValue<bool>() ?? false,
        TemFoil: variantes?["holo"]?.GetValue<bool>() ?? false,
        TemReverse: variantes?["reverse"]?.GetValue<bool>() ?? false,
        EhPromo: ehPromo);
}

static string MapearRaridade(string? api, int numero) => api?.Trim().ToLowerInvariant() switch
{
    "common" or "comum" => "Comum",
    "uncommon" or "incomum" => "Incomum",
    "rare" or "rara" => "Rara",
    "double rare" or "rara dupla" => "RaraDupla",
    "illustration rare" or "ilustração rara" => "IlustracaoRara",
    "ultra rare" or "rara ultra" => "RaraUltra",
    "special illustration rare" or "ilustração rara especial" => "IlustracaoRaraEspecial",
    "mega hyper rare" or "rara mega hiper" => "RaraMegaHiper",
    "promo" => "Promo",
    _ => throw new InvalidOperationException($"Raridade desconhecida na carta {numero}: '{api}'. Adicione no MapearRaridade."),
};

static string MapearTipo(string? api, int numero) => api?.Trim().ToLowerInvariant() switch
{
    "pokemon" or "pokémon" => "Pokemon",
    "trainer" or "treinador" => "Treinador",
    "energy" or "energia" => "Energia",
    _ => throw new InvalidOperationException($"Categoria desconhecida na carta {numero}: '{api}'."),
};

static string? Texto(JsonNode? no, params string[] caminho)
{
    foreach (var parte in caminho) no = no?[parte];
    var s = no?.GetValue<string>();
    return string.IsNullOrWhiteSpace(s) ? null : s;
}

// Baixa um JSON; devolve null se for 404. Tenta de novo em erros temporários.
async Task<JsonNode?> BaixarAsync(string url)
{
    for (int tentativa = 1; ; tentativa++)
    {
        try
        {
            using var resp = await http.GetAsync(url);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return JsonNode.Parse(await resp.Content.ReadAsStringAsync());
        }
        catch (Exception) when (tentativa < 5)
        {
            await Task.Delay(1000 * tentativa); // a API às vezes responde "no available server"
        }
    }
}

static string AcharRaiz()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Cardex.slnx")))
        dir = dir.Parent;
    return dir?.FullName ?? throw new InvalidOperationException("Não achei a raiz da solução (Cardex.slnx).");
}

static int Falhar(string mensagem)
{
    Console.Error.WriteLine("ERRO: " + mensagem);
    return 1;
}

record CartaJson(int Numero, string Nome, string Raridade, string Tipo, string? ImagemUrl,
                 bool TemNormal, bool TemFoil, bool TemReverse, bool EhPromo);

record ColecaoJson(string Codigo, string Nome, string NomeIngles, int TotalPrincipal, int TotalCartas,
                   string DataLancamento, List<CartaJson> Cartas);
