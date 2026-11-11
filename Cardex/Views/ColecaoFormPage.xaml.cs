using Cardex.ViewModels;

namespace Cardex.Views;

public partial class ColecaoFormPage : ContentPage
{
    public ColecaoFormPage(ColecaoFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
