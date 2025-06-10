using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme Color Palette", menuName = "Theme/Color Palette")]
public class ThemeColorPalette : ScriptableObject
{
    public Color Background = new Color32(29, 0, 51, 0255);
    public Color Primary = new Color32(46, 26, 75, 255);
    public Color Secondary = new Color32(65, 40, 100, 255);
    public Color Tertiary = new Color32(85, 55, 125, 255);
    public Color Surface = new Color32(80, 80, 150, 255);
    public Color Highlight = new Color32(60, 140, 120, 255);
    public Color Text = new Color32(255, 255, 255, 255);
    public Color Hover = new Color32(200, 200, 255, 255);
    public Color Pressed = new Color32(150, 150, 200, 255);

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

        target.Background = source.Background;
        target.Primary = source.Primary;
        target.Secondary = source.Secondary;
        target.Surface = source.Surface;
        target.Highlight = source.Highlight;
        target.Text = source.Text;
        target.Hover = source.Hover;
        target.Pressed = source.Pressed;
    }

    public bool Equals(ThemeColorPalette other)
    {
        if (other == null) return false;

        if (other == this) return true;

        return Background == other.Background &&
               Primary == other.Primary &&
               Secondary == other.Secondary &&
               Surface == other.Surface &&
               Highlight == other.Highlight &&
               Text == other.Text &&
               Hover == other.Hover &&
               Pressed == other.Pressed;
    }
}
