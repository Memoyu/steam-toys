using System.Windows;
using System.Windows.Controls;

namespace SteamToys.Shared.Helper;

public class RichTextBoxHelper : DependencyObject
{
    public static string GetRichText(DependencyObject obj)
    {
        return (string)obj.GetValue(RichTextProperty);
    }

    public static void SetRichText(DependencyObject obj, string value)
    {
        obj.SetValue(RichTextProperty, value);
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RichTextProperty =
        DependencyProperty.RegisterAttached("RichText", typeof(string), typeof(RichTextBoxHelper), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            PropertyChangedCallback = (obj, e) =>
            {
                var richTextBox = (RichTextBox)obj;
                var text = GetRichText(richTextBox);
                richTextBox.AppendText(text);
                richTextBox.AppendText(Environment.NewLine);
                richTextBox.ScrollToEnd();
            }

        });
}