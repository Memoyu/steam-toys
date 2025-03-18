using System.Diagnostics.CodeAnalysis;

namespace SteamToys.Core.Excel.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class HeaderCellAttribute : Attribute
{
    protected string _title { get; set; }
    public virtual string Title => _title;

    protected int _order { get; set; }
    public virtual int Order => _order;

    public HeaderCellAttribute()
    {
        _title = string.Empty;
    }

    public HeaderCellAttribute(string title)
    {
        _title = title;
    }

    public HeaderCellAttribute(string title, int order)
    {
        _title = title;
        _order = order;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) =>
    obj is DescriptionAttribute other && other.Description == Title;

    public override int GetHashCode() => Title?.GetHashCode() ?? 0;
}
