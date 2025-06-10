using TMPro;
using UnityEngine;

public class TextThemeApplier : ThemeApplier
{
    private TextMeshProUGUI m_Text;
    public override Color CurrentColor => m_Text != null ? m_Text.color : Color.clear;

    protected override void OnApplyTheme(ThemeColorPalette palette)
    {
        if (m_Text == null || palette == null) return;

        m_Text.color = palette.Text;
    }

    protected override void OnEnable()
    {
        m_Text = GetComponent<TextMeshProUGUI>();
        base.OnEnable();
    }
}