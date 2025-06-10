using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ThemeManager))]
public class ThemeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ThemeManager themeManager = (ThemeManager)target;

        if (GUILayout.Button("Force Theme Update"))
        {
            themeManager.ForceThemeUpdate();
        }
    }
}