using System.Text.Json;
using Cardex.Models;

namespace Cardex.Tests;

// Confere o me05.json que vai empacotado no app (gerado pelo CatalogBuilder).
public class CatalogoTests
{
    private static JsonElement Ler()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "App", "me05.json")));
        return doc.RootElement.Clone();
    }

    [Fact]
    public void Catalogo_tem_120_cartas_com_numeros_1_a_120_e_as_promos()
    {
        var raiz = Ler();
        var cartas = raiz.GetProperty("cartas").EnumerateArray().ToList();
        var principais = cartas.Where(c => !c.GetProperty("ehPromo").GetBoolean()).ToList();

        Assert.Equal(84, raiz.GetProperty("totalPrincipal").GetInt32());
        Assert.Equal(120, raiz.GetProperty("totalCartas").GetInt32());
        Assert.Equal(Enumerable.Range(1, 120), principais.Select(c => c.GetProperty("numero").GetInt32()).Order());
        Assert.Equal(2, cartas.Count - principais.Count);
    }

    [Fact]
    public void Toda_carta_tem_nome_raridade_valida_e_alguma_variante()
    {
        foreach (var c in Ler().GetProperty("cartas").EnumerateArray())
        {
            Assert.False(string.IsNullOrWhiteSpace(c.GetProperty("nome").GetString()));
            Assert.True(Enum.TryParse<Raridade>(c.GetProperty("raridade").GetString(), out _));
            Assert.True(Enum.TryParse<TipoCarta>(c.GetProperty("tipo").GetString(), out _));
            Assert.True(c.GetProperty("temNormal").GetBoolean() || c.GetProperty("temFoil").GetBoolean() || c.GetProperty("temReverse").GetBoolean());
        }
    }
}
