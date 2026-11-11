namespace Cardex.Models;

// Os enums são gravados no SQLite como int (comportamento padrão do sqlite-net).
// Por isso NÃO mude a ordem dos valores depois que houver dados no banco.

public enum Raridade
{
    Comum,
    Incomum,
    Rara,
    RaraDupla,
    IlustracaoRara,
    RaraUltra,
    IlustracaoRaraEspecial,
    RaraMegaHiper,
    Promo,
}

public enum TipoCarta
{
    Pokemon,
    Treinador,
    Energia,
}

public enum Variante
{
    Normal,
    Foil,
    Reverse,
}

public enum Condicao
{
    NM,
    SP,
    MP,
    HP,
    Danificada,
}

public enum Idioma
{
    Portugues,
    Ingles,
    Japones,
    Outro,
}
