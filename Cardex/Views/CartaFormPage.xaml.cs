using Cardex.ViewModels;

namespace Cardex.Views;

public partial class CartaFormPage : ContentPage
{
    public CartaFormPage(CartaFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
