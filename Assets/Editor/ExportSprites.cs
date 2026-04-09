using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportSprites
{
    [MenuItem("Tools/Export Selected Sprites")]
    static void Export()
    {
        foreach (Object obj in Selection.objects)
        {
            Sprite sprite = obj as Sprite;
            if (sprite == null) continue;

            Texture2D tex = sprite.texture;
            Rect r = sprite.rect;

            Texture2D newTex = new Texture2D((int)r.width, (int)r.height);
            Color[] pixels = tex.GetPixels(
                (int)r.x,
                (int)r.y,
                (int)r.width,
                (int)r.height
            );

            newTex.SetPixels(pixels);
            newTex.Apply();

            byte[] png = newTex.EncodeToPNG();
            File.WriteAllBytes("Assets/Resources/items/" + sprite.name + ".png", png);
        }

        AssetDatabase.Refresh();
    }
}