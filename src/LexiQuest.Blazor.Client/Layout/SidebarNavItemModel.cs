using Tempo.Blazor.Interfaces;

namespace LexiQuest.Blazor.Layout;

public class SidebarNavItemModel : ISidebarNavItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public int? BadgeCount { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<ISidebarNavItem>? Children { get; set; }
}
