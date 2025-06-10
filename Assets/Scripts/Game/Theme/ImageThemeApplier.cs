using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageThemeApplier : ThemeApplier
{
    public enum ImageColorType
    {
        Background,
        Primary,
        Secondary,
        PanelPrimary,
        PanelSecondary,
    }

    public ImageColorType ColorType;

    private Image m_Image;

    public override Color CurrentColor => m_Image != null ? m_Image.color : Color.clear;

    protected override void OnApplyTheme(ThemeColorPalette palette)
    {
        if (m_Image == null || palette == null) return;

        switch (ColorType)
        {
            case ImageColorType.Background:
                m_Image.color = palette.Void;
                break;
            case ImageColorType.Primary:
                m_Image.color = palette.CoreAccent;
                break;
            case ImageColorType.Secondary:
                m_Image.color = palette.SubAccent;
                break;
            case ImageColorType.PanelPrimary:
                m_Image.color = palette.PanelDark;
                break;
            case ImageColorType.PanelSecondary:
                m_Image.color = palette.PanelLight;
                break;
        }
    }

    protected override void OnEnable()
    {
        m_Image = GetComponent<Image>();
        base.OnEnable();
    }
}
