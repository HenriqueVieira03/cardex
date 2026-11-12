using Cardex.ViewModels;

namespace Cardex.Views;

public partial class CartaDetalhePage : ContentPage
{
    private readonly CartaDetalheViewModel _viewModel;

    public CartaDetalhePage(CartaDetalheViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Recarrega ao aparecer (inclusive depois de editar a carta no modal).
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.CarregarCommand.CanExecute(null))
            _viewModel.CarregarCommand.Execute(null);
    }
}
