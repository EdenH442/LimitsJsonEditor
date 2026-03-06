using System.Windows;

namespace LimitsEditor.Behaviors;

/// <summary>
/// TODO: Replace with concrete attached behaviors for dynamic UI interactions.
/// </summary>
public static class PlaceholderBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(PlaceholderBehavior),
            new PropertyMetadata(false));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
}
