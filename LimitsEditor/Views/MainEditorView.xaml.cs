using System.Windows;
using System.Windows.Controls;
using LimitsEditor.ViewModels;

namespace LimitsEditor.Views;

public partial class MainEditorView : UserControl
{
    public MainEditorView()
    {
        InitializeComponent();
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
