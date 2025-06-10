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
        Tertiary,
        Surface,
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
                m_Image.color = palette.Background;
                break;
            case ImageColorType.Primary:
                m_Image.color = palette.Primary;
                break;
            case ImageColorType.Secondary:
                m_Image.color = palette.Secondary;
                break;
            case ImageColorType.Tertiary:
                m_Image.color = palette.Tertiary;
                break;
            case ImageColorType.Surface:
                m_Image.color = palette.Surface;
                break;
        }
    }

    protected override void OnEnable()
    {
        m_Image = GetComponent<Image>();
        base.OnEnable();
    }
}
