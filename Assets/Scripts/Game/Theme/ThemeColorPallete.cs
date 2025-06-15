using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme Color Palette", menuName = "Theme/Color Palette")]
public class ThemeColorPalette : ScriptableObject
{
    // Core background for the scene/main UI elements.
    public Color Void = new Color32(0x1D, 0x00, 0x33, 0xFF); // HEX: #1D0033

    // The main accent color for interactive elements like primary buttons, important icons, or main character highlights.
    public Color CoreAccent = new Color32(0x41, 0x28, 0x64, 0xFF); // HEX: #412864

    // A secondary accent color, perhaps for less critical interactive elements, secondary UI elements, or subtle distinctions.
    public Color SubAccent = new Color32(0x55, 0x37, 0x7D, 0xFF); // HEX: #55377D
    public Color AltAccent = new Color32(0x6A, 0x4B, 0x96, 0xFF); // HEX: #6A4B96

    // Darker surface for base panels, backgrounds of UI elements, or deep structural components.
    public Color PanelDark = new Color32(0x2E, 0x1A, 0x4B, 0xFF); // HEX: #2E1A4B

    // Lighter surface for interactive areas within panels, selected states, or foreground UI elements.
    public Color PanelLight = new Color32(0x50, 0x50, 0x96, 0xFF); // HEX: #505096

    // A vibrant color for interactive elements when the user hovers over them.
    public Color InteractiveHover = new Color32(0xC8, 0xC8, 0xFF, 0xFF); // HEX: #C8C8FF

    // A distinct color for interactive elements when they are actively pressed or clicked.
    public Color InteractivePressed = new Color32(0x96, 0x96, 0xC8, 0xFF); // HEX: #9696C8 

    // Used for progress bars, alerts, or elements that need to stand out. Can also be a player's health bar or power-up indicator.
    public Color Energized = new Color32(0x3C, 0x8C, 0x78, 0xFF); // HEX: #3C8C78

    // Primary text color for readability.
    public Color CoreGlyphs = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // HEX: #FFFFFF
    public Color SubGlyphs = new Color32(0xB0, 0xB0, 0xB0, 0xFF); // HEX: #B0B0B0
    public Color GoodGlpyhs = new Color32(0xA0, 0xFF, 0xA0, 0xFF); // HEX: #A0FFA0
    public Color BadGlyphs = new Color32(0xFF, 0xA0, 0xA0, 0xFF); // HEX:rgb(241, 114, 114)
    public Color AltGlyphs = new Color32(0x30, 0x30, 0x30, 0xFF); // HEX: #303030

    public static void Copy(in ThemeColorPalette source, ref ThemeColorPalette target)
    {
        if (target == null)
        {
            target = CreateInstance<ThemeColorPalette>();
        }

        if (source == null)
        {
            return;
        }

        target.Void = source.Void;
        target.CoreAccent = source.CoreAccent;
        target.SubAccent = source.SubAccent;
        target.PanelDark = source.PanelDark;
        target.PanelLight = source.PanelLight;
        target.Energized = source.Energized;
        target.CoreGlyphs = source.CoreGlyphs;
        target.InteractiveHover = source.InteractiveHover;
        target.InteractivePressed = source.InteractivePressed;
    }

    public bool Equals(ThemeColorPalette other)
    {
        if (other == null) return false;

        if (other == this) return true;

        return Void == other.Void &&
            CoreAccent == other.CoreAccent &&
            SubAccent == other.SubAccent &&
            AltAccent == other.AltAccent &&
            PanelDark == other.PanelDark &&
            PanelLight == other.PanelLight &&
            Energized == other.Energized &&
            CoreGlyphs == other.CoreGlyphs &&
            SubGlyphs == other.SubGlyphs &&
            AltGlyphs == other.AltGlyphs &&
            InteractiveHover == other.InteractiveHover &&
            InteractivePressed == other.InteractivePressed;
    }
}