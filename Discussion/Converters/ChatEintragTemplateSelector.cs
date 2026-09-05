using System.Windows;
using System.Windows.Controls;
using Discussion.Models;

namespace Discussion.Converters;

public class ChatEintragTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NachrichtTemplate { get; set; }
    public DataTemplate? TrennerTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container) =>
        item is ChatEintrag { Sprecher: Sprecher.Trenner } ? TrennerTemplate : NachrichtTemplate;
}
