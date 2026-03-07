using System.Windows;
using LimitsEditor.ViewModels;

namespace LimitsEditor.Views;

public partial class MainView : Window
{
    public MainView(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
