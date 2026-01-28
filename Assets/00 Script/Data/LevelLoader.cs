using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData Load(int level)
    {
        string path = $"campaign level/level_{level}"; 
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            Debug.LogError("Không tìm thấy level " + level);
            return null;
        }

        return JsonUtility.FromJson<LevelData>(jsonFile.text);
    }
    
}
