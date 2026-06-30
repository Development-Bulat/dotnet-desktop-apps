using Avalonia.Controls;

namespace Fitness_Club_01.Services;

public static class ShellWindowHelper
{
    public static void ApplyUserCaption(TextBlock captionBlock)
    {
        var role = Session.CurrentRole?.RoleName ?? "";
        var name = Session.GetDisplayName();
        captionBlock.Text = string.IsNullOrWhiteSpace(name) ? role : $"{role} · {name}";
    }
}
