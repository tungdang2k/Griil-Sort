using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditor : EditorWindow
{
    private string key = "";

    private string stringValue = "";
    private int intValue = 0;
    private float floatValue = 0f;

    [MenuItem("Tools/PlayerPrefs Editor")]
    public static void ShowWindow()
    {
        GetWindow<PlayerPrefsEditor>("PlayerPrefs Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Tool", EditorStyles.boldLabel);

        key = EditorGUILayout.TextField("Key", key);

        GUILayout.Space(10);

        // ===== STRING =====
        GUILayout.Label("STRING", EditorStyles.boldLabel);
        stringValue = EditorGUILayout.TextField("Value", stringValue);

        if (GUILayout.Button("Set String"))
        {
            PlayerPrefs.SetString(key, stringValue);
            PlayerPrefs.Save();
        }

        if (GUILayout.Button("Get String"))
        {
            stringValue = PlayerPrefs.GetString(key, "NULL");
        }

        GUILayout.Space(10);

        // ===== INT =====
        GUILayout.Label("INT", EditorStyles.boldLabel);
        intValue = EditorGUILayout.IntField("Value", intValue);

        if (GUILayout.Button("Set Int"))
        {
            PlayerPrefs.SetInt(key, intValue);
            PlayerPrefs.Save();
        }

        if (GUILayout.Button("Get Int"))
        {
            intValue = PlayerPrefs.GetInt(key, 0);
        }

        GUILayout.Space(10);

        // ===== FLOAT =====
        GUILayout.Label("FLOAT", EditorStyles.boldLabel);
        floatValue = EditorGUILayout.FloatField("Value", floatValue);

        if (GUILayout.Button("Set Float"))
        {
            PlayerPrefs.SetFloat(key, floatValue);
            PlayerPrefs.Save();
        }

        if (GUILayout.Button("Get Float"))
        {
            floatValue = PlayerPrefs.GetFloat(key, 0f);
        }

        GUILayout.Space(20);

        // ===== DELETE =====
        if (GUILayout.Button("Delete Key"))
        {
            PlayerPrefs.DeleteKey(key);
        }

        if (GUILayout.Button("Delete ALL"))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}