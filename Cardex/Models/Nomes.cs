namespace Cardex.Models;

// Nomes em português dos enums, para exibir na tela.
// São "métodos de extensão": permitem escrever carta.Raridade.Nome().
public static class Nomes
{
    public static string Nome(this Raridade raridade) => raridade switch
    {
        Raridade.Comum => "Comum",
        Raridade.Incomum => "Incomum",
        Raridade.Rara => "Rara",
        Raridade.RaraDupla => "Rara Dupla",
        Raridade.IlustracaoRara => "Ilustração Rara",
        Raridade.RaraUltra => "Rara Ultra",
        Raridade.IlustracaoRaraEspecial => "Ilustração Rara Especial",
        Raridade.RaraMegaHiper => "Rara Mega Hiper",
        Raridade.Promo => "Promo",
        _ => raridade.ToString(),
    };

    public static string Nome(this TipoCarta tipo) => tipo switch
    {
        TipoCarta.Pokemon => "Pokémon",
        TipoCarta.Treinador => "Treinador",
        TipoCarta.Energia => "Energia",
        _ => tipo.ToString(),
    };

    public static string Nome(this Variante variante) => variante switch
    {
        Variante.Normal => "Normal",
        Variante.Foil => "Foil",
        Variante.Reverse => "Reverse",
        _ => variante.ToString(),
    };

    public static string Nome(this Condicao condicao) => condicao switch
    {
        Condicao.NM => "NM (quase nova)",
        Condicao.SP => "SP (pouco usada)",
        Condicao.MP => "MP (usada)",
        Condicao.HP => "HP (muito usada)",
        Condicao.Danificada => "Danificada",
        _ => condicao.ToString(),
    };

    public static string Nome(this Idioma idioma) => idioma switch
    {
        Idioma.Portugues => "Português",
        Idioma.Ingles => "Inglês",
        Idioma.Japones => "Japonês",
        Idioma.Outro => "Outro",
        _ => idioma.ToString(),
    };
}
