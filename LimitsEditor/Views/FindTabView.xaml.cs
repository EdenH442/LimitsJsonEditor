using System.Windows.Controls;
using System.Windows.Input;
using LimitsEditor.Models;
using LimitsEditor.ViewModels;

namespace LimitsEditor.Views;

public partial class FindTabView : UserControl
{
    public FindTabView()
    {
        InitializeComponent();
    }

    private void TestsListBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox ||
            DataContext is not FindTabViewModel viewModel ||
            ItemsControl.ContainerFromElement(listBox, e.OriginalSource as System.Windows.DependencyObject) is not ListBoxItem item ||
            item.DataContext is not Step clickedTest ||
            !ReferenceEquals(clickedTest, viewModel.SelectedTest))
        {
            return;
        }

        viewModel.SelectedTest = null;
        e.Handled = true;
    }
}
