using UnityEngine;

[ExecuteInEditMode]
public abstract class ThemeApplier : MonoBehaviour
{
    public abstract Color CurrentColor { get; }

    protected virtual void OnEnable()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.RegisterApplier(this);
        }
        ApplyTheme();
    }

    protected virtual void OnDisable()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.UnregisterApplier(this);
        }
    }

    protected virtual void OnValidate()
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (ThemeManager.Instance == null || ThemeManager.Instance.CurrentTheme == null)
        {
            Debug.LogWarning("ThemeManager or CurrentTheme is not set. Cannot apply theme.");
            return;
        }
        OnApplyTheme(ThemeManager.Instance.CurrentTheme);
    }
    protected abstract void OnApplyTheme(ThemeColorPalette palette);
}