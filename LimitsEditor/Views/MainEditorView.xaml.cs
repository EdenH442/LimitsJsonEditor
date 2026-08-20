using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LimitsEditor.ViewModels;

namespace LimitsEditor.Views;

public partial class MainEditorView : UserControl
{
    public MainEditorView()
    {
        InitializeComponent();
    }

    private void TestsListBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox ||
            DataContext is not MainEditorViewModel viewModel ||
            FindAncestor<Button>(e.OriginalSource as DependencyObject) is not null ||
            ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) is not ListBoxItem item ||
            item.DataContext is not TestNavigationItemViewModel clickedTest ||
            !ReferenceEquals(clickedTest, viewModel.SelectedTestItem))
        {
            return;
        }

        viewModel.SelectedTestItem = null;
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void SequenceNameTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is SequenceItemViewModel sequence)
        {
            sequence.CommitEdit();
        }
    }

    private void SequenceNameTextBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.DataContext is not SequenceItemViewModel sequence)
        {
            return;
        }

        if (!sequence.IsEditing)
        {
            return;
        }

        textBox.Dispatcher.BeginInvoke(() =>
        {
            textBox.Focus();
            textBox.SelectAll();
        });
    }
}
