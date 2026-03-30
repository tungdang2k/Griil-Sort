using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteResizeExporter : EditorWindow
{
    int targetWidth = 312;
    int targetHeight = 604;
    string exportFolder = "Assets/ResizedSprites";

    [MenuItem("Tools/Resize & Export Sprites")]
    public static void ShowWindow()
    {
        GetWindow<SpriteResizeExporter>("Resize Sprites");
    }

    void OnGUI()
    {
        GUILayout.Label("Resize Sprites Tool", EditorStyles.boldLabel);

        targetWidth = EditorGUILayout.IntField("Width", targetWidth);
        targetHeight = EditorGUILayout.IntField("Height", targetHeight);
        exportFolder = EditorGUILayout.TextField("Export Folder", exportFolder);

        if (GUILayout.Button("Process Selected Sprites"))
        {
            Process();
        }
    }

    void Process()
    {
        Object[] selected = Selection.objects;
        var sprites = new System.Collections.Generic.List<Sprite>();

        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning("No sprites selected!");
            return;
        }

        if (!Directory.Exists(exportFolder))
            Directory.CreateDirectory(exportFolder);

        foreach (var sprite in sprites)
        {
            Texture2D source = GetReadableTexture(sprite);

            Texture2D resized = ResizeWithPadding(source, targetWidth, targetHeight);

            byte[] png = resized.EncodeToPNG();
            File.WriteAllBytes($"{exportFolder}/{sprite.name}.png", png);
        }

        AssetDatabase.Refresh();
        Debug.Log("Done!");
    }

    Texture2D GetReadableTexture(Sprite sprite)
    {
        Texture2D tex = sprite.texture;

        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

        if (!importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Rect rect = sprite.rect;

        Texture2D newTex = new Texture2D((int)rect.width, (int)rect.height);
        newTex.SetPixels(tex.GetPixels(
            (int)rect.x,
            (int)rect.y,
            (int)rect.width,
            (int)rect.height
        ));
        newTex.Apply();

        return newTex;
    }

    Texture2D ResizeWithPadding(Texture2D source, int targetW, int targetH)
    {
        Texture2D result = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false);

        Color[] fill = new Color[targetW * targetH];
        for (int i = 0; i < fill.Length; i++)
            fill[i] = new Color(0, 0, 0, 0);

        result.SetPixels(fill);

        float scale = Mathf.Min(
            (float)targetW / source.width,
            (float)targetH / source.height
        );

        int newW = Mathf.RoundToInt(source.width * scale);
        int newH = Mathf.RoundToInt(source.height * scale);

        Texture2D scaled = ScaleTexture(source, newW, newH);

        int offsetX = (targetW - newW) / 2;
        int offsetY = (targetH - newH) / 2;

        result.SetPixels(offsetX, offsetY, newW, newH, scaled.GetPixels());
        result.Apply();

        return result;
    }

    Texture2D ScaleTexture(Texture2D source, int width, int height)
    {
        Texture2D result = new Texture2D(width, height, source.format, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width;
                float v = (float)y / height;
                result.SetPixel(x, y, source.GetPixelBilinear(u, v));
            }
        }

        result.Apply();
        return result;
    }
}