using UnityEngine;
using UnityEditor;
using System.IO;

public class RemoveSuffixTool
{
    [MenuItem("Tools/Remove _0 Suffix")]
    public static void RemoveSuffix()
    {
        string folderPath = "Assets/imagetest"; // đổi path nếu cần

        string[] assetPaths = AssetDatabase.FindAssets("", new[] { folderPath });

        foreach (string guid in assetPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            // chỉ xử lý file kết thúc bằng _0
            if (fileName.EndsWith("_0"))
            {
                string newName = fileName.Substring(0, fileName.Length - 2);
                string newPath = Path.Combine(Path.GetDirectoryName(path), newName + Path.GetExtension(path));

                // tránh lỗi trùng tên
                if (File.Exists(newPath))
                {
                    Debug.LogWarning($"Skip (already exists): {newPath}");
                    continue;
                }

                AssetDatabase.RenameAsset(path, newName);
                Debug.Log($"Renamed: {fileName} → {newName}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Done removing _0 suffix!");
    }
}