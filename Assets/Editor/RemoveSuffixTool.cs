using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class RenameRemoveCopy
{
    [MenuItem("Tools/Rename/Remove Copy Format")]
    public static void RemoveCopyFormat()
    {
        var selected = Selection.objects;

        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string oldName = obj.name;

            // item3 copy_1 -> item3_1
            string newName = Regex.Replace(oldName, @" copy_(\d+)", "_$1");

            if (newName != oldName)
            {
                AssetDatabase.RenameAsset(path, newName);
                Debug.Log($"Renamed: {oldName} → {newName}");
            }
        }

        AssetDatabase.Refresh();
    }
}