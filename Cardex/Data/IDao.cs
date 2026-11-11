namespace Cardex.Data;

// Contrato do padrão DAO (Data Access Object): as operações de CRUD de uma tabela.
// As Views/ViewModels nunca usam o SQLite diretamente, só os DAOs.
public interface IDao<T> where T : new()
{
    Task<List<T>> ObterTodosAsync();

    Task<T?> ObterPorIdAsync(int id);

    Task<int> InserirAsync(T item);

    Task<int> AtualizarAsync(T item);

    Task<int> ExcluirAsync(T item);
}
