using Cardex.Models;

namespace Cardex.Tests;

public class CartaTests
{
    [Theory]
    [InlineData(84, false, false)] // última da coleção principal
    [InlineData(85, false, true)]  // primeira secret rare
    [InlineData(88, true, false)]  // promo MEP 088: numeração própria, nunca é secreta
    public void CalcularSecreta_usa_TotalPrincipal_e_ignora_promos(int numero, bool ehPromo, bool esperado)
    {
        var carta = new Carta { Numero = numero, EhPromo = ehPromo };

        carta.CalcularSecreta(totalPrincipal: 84);

        Assert.Equal(esperado, carta.EhSecreta);
    }

    [Fact]
    public void VariantesExistentes_lista_so_as_marcadas()
    {
        var carta = new Carta { TemNormal = false, TemFoil = true, TemReverse = true };

        Assert.Equal([Variante.Foil, Variante.Reverse], carta.VariantesExistentes());
    }

    [Fact]
    public void NumeroFormatado_tem_tres_digitos()
    {
        Assert.Equal("045", new Carta { Numero = 45 }.NumeroFormatado);
    }
}
