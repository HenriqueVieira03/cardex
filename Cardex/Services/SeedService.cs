using System.Text.Json;
using System.Text.Json.Serialization;
using Cardex.Data;
using Cardex.Models;

namespace Cardex.Services;

// Na primeira execução, importa o catálogo (Resources/Raw/me05.json) para o banco.
public class SeedService
{
    private const string ArquivoCatalogo = "me05.json";

    private readonly ColecaoDao _colecaoDao;
    private readonly CartaDao _cartaDao;

    public SeedService(ColecaoDao colecaoDao, CartaDao cartaDao)
    {
        _colecaoDao = colecaoDao;
        _cartaDao = cartaDao;
    }

    // Devolve true se importou agora; false se a coleção já existia.
    public async Task<bool> ImportarSeNecessarioAsync()
    {
        await using var arquivo = await FileSystem.OpenAppPackageFileAsync(ArquivoCatalogo);
        var catalogo = await JsonSerializer.DeserializeAsync<CatalogoJson>(arquivo, OpcoesJson)
            ?? throw new InvalidOperationException("Catálogo vazio.");

        // Nunca roda de novo: se a coleção já está no banco, não faz nada.
        if (await _colecaoDao.ObterPorCodigoAsync(catalogo.Codigo) is not null)
            return false;

        var colecao = new Colecao
        {
            Nome = catalogo.Nome,
            NomeIngles = catalogo.NomeIngles,
            Codigo = catalogo.Codigo,
            TotalPrincipal = catalogo.TotalPrincipal,
            TotalCartas = catalogo.TotalCartas,
            DataLancamento = catalogo.DataLancamento,
            Favorita = true, // a coleção que vem pronta já aparece no Início
        };
        await _colecaoDao.InserirAsync(colecao); // depois disso, colecao.Id está preenchido

        var cartas = catalogo.Cartas.Select(c =>
        {
            var carta = new Carta
            {
                ColecaoId = colecao.Id,
                Numero = c.Numero,
                Nome = c.Nome,
                Raridade = c.Raridade,
                Tipo = c.Tipo,
                ImagemUrl = c.ImagemUrl,
                TemNormal = c.TemNormal,
                TemFoil = c.TemFoil,
                TemReverse = c.TemReverse,
                EhPromo = c.EhPromo,
                Personalizada = false,
            };
            carta.CalcularSecreta(colecao.TotalPrincipal);
            return carta;
        });
        await _cartaDao.InserirVariasAsync(cartas);
        return true;
    }

    // O JSON usa camelCase e os enums vêm como texto ("RaraDupla").
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    // Classes de transporte (DTO): espelham o formato do me05.json.
    private record CatalogoJson(string Codigo, string Nome, string NomeIngles, int TotalPrincipal,
                                int TotalCartas, DateTime DataLancamento, List<CartaJson> Cartas);

    private record CartaJson(int Numero, string Nome, Raridade Raridade, TipoCarta Tipo, string? ImagemUrl,
                             bool TemNormal, bool TemFoil, bool TemReverse, bool EhPromo);
}
