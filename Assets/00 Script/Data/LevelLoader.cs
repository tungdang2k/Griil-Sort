using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData Load(int level)
    {
        string path = CONSTANTS.DATA_PATH + level;

        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            
            return null;
        }

        return JsonUtility.FromJson<LevelData>(jsonFile.text);
    }

    

}
