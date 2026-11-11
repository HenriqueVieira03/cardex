namespace Cardex.Services;

// Mensagens para o usuário. Os ViewModels usam esta interface, e não as páginas,
// por isso podem ser testados trocando a implementação por uma falsa.
public interface IAlertaService
{
    // Mensagem curta que some sozinha (CommunityToolkit.Maui).
    Task MostrarToastAsync(string mensagem);

    // Barra no rodapé com um botão de ação opcional (ex.: "Desfazer").
    Task MostrarSnackbarAsync(string mensagem, string? textoAcao = null, Func<Task>? aoTocar = null);

    // Caixa de diálogo Sim/Não. Devolve true se o usuário confirmou.
    Task<bool> ConfirmarAsync(string titulo, string mensagem, string sim = "Sim", string nao = "Cancelar");

    Task AvisarAsync(string titulo, string mensagem);
}
