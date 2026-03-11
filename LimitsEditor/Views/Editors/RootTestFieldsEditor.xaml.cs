using System.Windows;
using System.Windows.Controls;

namespace LimitsEditor.Views.Editors;

public partial class RootTestFieldsEditor : UserControl
{
    public static readonly DependencyProperty TestNameProperty = DependencyProperty.Register(
        nameof(TestName),
        typeof(string),
        typeof(RootTestFieldsEditor),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty TestTypeProperty = DependencyProperty.Register(
        nameof(TestType),
        typeof(string),
        typeof(RootTestFieldsEditor),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public RootTestFieldsEditor()
    {
        InitializeComponent();
    }

    public string TestName
    {
        get => (string)GetValue(TestNameProperty);
        set => SetValue(TestNameProperty, value);
    }

    public string TestType
    {
        get => (string)GetValue(TestTypeProperty);
        set => SetValue(TestTypeProperty, value);
    }
}
