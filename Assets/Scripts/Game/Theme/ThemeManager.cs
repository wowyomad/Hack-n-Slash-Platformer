using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    public ThemeColorPalette CurrentTheme;

    public bool UpdateThemeAutomatically = false;

    private List<ThemeApplier> m_RegisteredAppliers = new List<ThemeApplier>();
    private ThemeColorPalette m_LastAppliedTheme;

    public void SetTheme(ThemeColorPalette newTheme)
    {
        CurrentTheme = newTheme;
        m_RegisteredAppliers.ForEach(applier => applier?.ApplyTheme());
    }

    public void ForceThemeUpdate()
    {
        SetTheme(CurrentTheme);
    }

    public void RegisterApplier(ThemeApplier applier)
    {
        if (applier == null || m_RegisteredAppliers.Contains(applier)) return;

        m_RegisteredAppliers.Add(applier);

        if (CurrentTheme != null)
        {
            applier.ApplyTheme();
        }
    }

    public void UnregisterApplier(ThemeApplier applier)
    {
        m_RegisteredAppliers.Remove(applier);
    }

    private void Update()
    {
        if (UpdateThemeAutomatically && CurrentTheme != null && m_LastAppliedTheme != null)
        {
            if (!CurrentTheme.Equals(m_LastAppliedTheme))
            {
                SetTheme(CurrentTheme);
                ThemeColorPalette.Copy(in CurrentTheme, ref m_LastAppliedTheme);
            }
        }
    }

    private void Start()
    {
        m_LastAppliedTheme = ScriptableObject.CreateInstance<ThemeColorPalette>();
        ThemeColorPalette.Copy(in CurrentTheme, ref m_LastAppliedTheme);
    }

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        Instance = this;

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }


    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        SetTheme(CurrentTheme);
    }
}