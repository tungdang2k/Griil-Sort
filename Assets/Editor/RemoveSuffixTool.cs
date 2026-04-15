using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class RenameItemsTool
{
    [MenuItem("Tools/Rename Items")]
    public static void RenameItems()
    {
        string folderPath = "Assets/Resources/items"; // 👉 sửa path nếu cần

        string[] files = Directory.GetFiles(folderPath);

        int index = 1;
            
        foreach (string file in files)
        {
            // Bỏ qua meta file
            if (file.EndsWith(".meta")) continue;

            string extension = Path.GetExtension(file);

            string newName = $"item-{index}{extension}";

            string newPath = Path.Combine(folderPath, newName);

            AssetDatabase.MoveAsset(file, newPath);

            index++;
        }

        AssetDatabase.Refresh();

        Debug.Log("Rename Done!");
    }
}