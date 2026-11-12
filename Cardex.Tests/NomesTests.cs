using Cardex.Models;

namespace Cardex.Tests;

public class NomesTests
{
    [Theory]
    [InlineData(Raridade.RaraDupla, "Rara Dupla")]
    [InlineData(Raridade.IlustracaoRara, "Ilustração Rara")]
    [InlineData(Raridade.IlustracaoRaraEspecial, "Ilustração Rara Especial")]
    [InlineData(Raridade.RaraMegaHiper, "Rara Mega Hiper")]
    public void Raridade_tem_nome_em_portugues(Raridade raridade, string esperado)
    {
        Assert.Equal(esperado, raridade.Nome());
    }

    [Fact]
    public void Todo_valor_de_enum_tem_nome_proprio()
    {
        // Se alguém acrescentar um valor no enum e esquecer o nome, o switch cai no ToString()
        // e nomes como "IlustracaoRara" (sem acento/espaço) apareceriam na tela.
        Assert.All(Enum.GetValues<Raridade>().Where(r => r is not (Raridade.Comum or Raridade.Incomum or Raridade.Rara or Raridade.Promo)),
                   r => Assert.NotEqual(r.ToString(), r.Nome()));
        Assert.Equal("Pokémon", TipoCarta.Pokemon.Nome());
        Assert.Equal("Português", Idioma.Portugues.Nome());
        Assert.Equal("Japonês", Idioma.Japones.Nome());
    }
}
