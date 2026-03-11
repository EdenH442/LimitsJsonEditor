using System.Windows;
using System.Windows.Controls;

namespace LimitsEditor.Views.Editors;

public partial class SubTestFieldsEditor : UserControl
{
    public static readonly DependencyProperty ShowSubTestNameProperty = DependencyProperty.Register(
        nameof(ShowSubTestName),
        typeof(bool),
        typeof(SubTestFieldsEditor),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsEditorEnabledProperty = DependencyProperty.Register(
        nameof(IsEditorEnabled),
        typeof(bool),
        typeof(SubTestFieldsEditor),
        new PropertyMetadata(false));

    public static readonly DependencyProperty SubTestNameProperty = DependencyProperty.Register(
        nameof(SubTestName),
        typeof(string),
        typeof(SubTestFieldsEditor),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty LimitEditorProperty = DependencyProperty.Register(
        nameof(LimitEditor),
        typeof(object),
        typeof(SubTestFieldsEditor),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public SubTestFieldsEditor()
    {
        InitializeComponent();
    }

    public bool ShowSubTestName
    {
        get => (bool)GetValue(ShowSubTestNameProperty);
        set => SetValue(ShowSubTestNameProperty, value);
    }

    public bool IsEditorEnabled
    {
        get => (bool)GetValue(IsEditorEnabledProperty);
        set => SetValue(IsEditorEnabledProperty, value);
    }

    public string SubTestName
    {
        get => (string)GetValue(SubTestNameProperty);
        set => SetValue(SubTestNameProperty, value);
    }

    public object? LimitEditor
    {
        get => GetValue(LimitEditorProperty);
        set => SetValue(LimitEditorProperty, value);
    }
}
