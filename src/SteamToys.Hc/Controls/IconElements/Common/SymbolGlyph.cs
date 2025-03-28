﻿namespace SteamToys.Controls;

/// <summary>
/// Set of static methods to operate on <see cref="SymbolRegular"/> and <see cref="SymbolFilled"/>.
/// </summary>
public static class SymbolGlyph
{
    /// <summary>
    /// If the icon is not found in some places, this one will be displayed.
    /// </summary>
    public const SymbolRegular DefaultIcon = SymbolRegular.BorderNone24;

    /// <summary>
    /// If the filled icon is not found in some places, this one will be displayed.
    /// </summary>
    public const SymbolFilled DefaultFilledIcon = SymbolFilled.BorderNone24;

    /// <summary>
    /// Finds icon based on name.
    /// </summary>
    /// <param name="name">Name of the icon.</param>
    public static SymbolRegular Parse(string name)
    {
        if (String.IsNullOrEmpty(name))
            return DefaultIcon;

        try
        {
            return (SymbolRegular)Enum.Parse(typeof(SymbolRegular), name);
        }
        catch (Exception e)
        {
#if DEBUG
            throw;
#else
            return DefaultIcon;
#endif
        }
    }

    /// <summary>
    /// Finds icon based on name.
    /// </summary>
    /// <param name="name">Name of the icon.</param>
    public static SymbolFilled ParseFilled(string name)
    {
        if (String.IsNullOrEmpty(name))
            return DefaultFilledIcon;

        try
        {
            return (SymbolFilled)Enum.Parse(typeof(SymbolFilled), name);
        }
        catch (Exception e)
        {
#if DEBUG
            throw;
#else
            return DefaultFilledIcon;
#endif
        }
    }
}
