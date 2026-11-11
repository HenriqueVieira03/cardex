using Cardex.Models;
using SQLite;

namespace Cardex.Data;

// Responsável por abrir a conexão com o SQLite e criar as tabelas.
// É registrado como Singleton: o app inteiro usa a mesma conexão.
public class CardexDatabase
{
    private readonly string _caminho;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private SQLiteAsyncConnection? _conexao;

    public CardexDatabase(string caminho)
    {
        _caminho = caminho;
    }

    // Abre a conexão na primeira chamada (inicialização "preguiçosa") e devolve sempre a mesma.
    public async Task<SQLiteAsyncConnection> ObterConexaoAsync()
    {
        if (_conexao is not null)
            return _conexao;

        await _trava.WaitAsync(); // evita que duas telas criem a conexão ao mesmo tempo
        try
        {
            if (_conexao is null)
            {
                var conexao = new SQLiteAsyncConnection(_caminho,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                // Cria a tabela se não existir (ou acrescenta colunas novas, se o Model mudou).
                await conexao.CreateTableAsync<Colecao>();
                await conexao.CreateTableAsync<Carta>();
                await conexao.CreateTableAsync<Exemplar>();

                _conexao = conexao;
            }
            return _conexao;
        }
        finally
        {
            _trava.Release();
        }
    }
}
