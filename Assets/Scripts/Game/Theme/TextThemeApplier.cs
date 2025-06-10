using TMPro;
using UnityEngine;

public class TextThemeApplier : ThemeApplier
{
    public enum TextColorType
    {
        CoreGlyphs,
        SubGlyphs,
        AltGlyphs
    }

    public TextColorType ColorType;
    public TextMeshProUGUI targetText;

    public override Color CurrentColor => targetText != null ? targetText.color : Color.clear;

    protected override void OnApplyTheme(ThemeColorPalette palette)
    {
        if (targetText == null || palette == null) return;

        switch (ColorType)
        {
            case TextColorType.CoreGlyphs:
                targetText.color = palette.CoreGlyphs;
                break;
            case TextColorType.SubGlyphs:
                targetText.color = palette.SubGlyphs;
                break;
            case TextColorType.AltGlyphs:
                targetText.color = palette.AltGlyphs;
                break;
        }
    }

    protected override void OnEnable()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }
        base.OnEnable();
    }
}