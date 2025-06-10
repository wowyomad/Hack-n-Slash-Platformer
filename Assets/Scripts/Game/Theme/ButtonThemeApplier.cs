using UnityEngine;
using UnityEngine.UI;

public class ButtonThemeApplier : ThemeApplier
{
    public enum ButtonColorType
    {
        Primary,
        Secondary,
        Tertiary,
    }

    public ButtonColorType ColorType;
    private Button m_Button;
    private Image m_Image;

    public override Color CurrentColor => m_Button != null ? m_Button.colors.normalColor : Color.clear;

    protected override void OnApplyTheme(ThemeColorPalette palette)
    {
        if (m_Button == null || m_Image == null || palette == null) return;

        ColorBlock colors = m_Button.colors;

        switch (ColorType)
        {
            case ButtonColorType.Primary:
                m_Image.color = palette.Primary;
                break;
            case ButtonColorType.Secondary:
                m_Image.color = palette.Secondary;
                break;
            case ButtonColorType.Tertiary:
                m_Image.color = palette.Tertiary;
                break;
        }

        m_Button.colors = colors;
    }

    protected override void OnEnable()
    {
        m_Button = GetComponent<Button>();
        m_Image = GetComponent<Image>();
        base.OnEnable();
    }
}