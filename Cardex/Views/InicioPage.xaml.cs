using Cardex.ViewModels;

namespace Cardex.Views;

public partial class InicioPage : ContentPage
{
    private readonly InicioViewModel _viewModel;

    public InicioPage(InicioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Recarrega sempre que a tela aparece (inclusive ao voltar de um modal ou de outra tela).
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.CarregarCommand.CanExecute(null))
            _viewModel.CarregarCommand.Execute(null);
    }
}
