using PixelCrushers.DialogueSystem;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class PlayerData
{
    public string save;
}

public class SaveDatas : MonoBehaviour
{
    public PlayerData playerData = new PlayerData();

    private string savePath;
    private string fileName = "/saveData.txt";

    private void Start()
    {
        savePath = Application.persistentDataPath + fileName;
        LoadData();
    }

    public void SaveData()
    {
        playerData.save = PersistentDataManager.GetSaveData();

        string data = JsonUtility.ToJson(playerData);
        File.WriteAllText(savePath, data);
    }

    private void LoadData()
    {
        if(!File.Exists(savePath))
        {
            SaveData();
        }
        string data = File.ReadAllText(savePath);
        playerData = JsonUtility.FromJson<PlayerData>(data);
        PersistentDataManager.ApplySaveData(playerData.save);
    }

    private void OnEnable()
    {
        Lua.RegisterFunction("Save", this, SymbolExtensions.GetMethodInfo(()=>SaveData()));
    }
    private void OnDisable()
    {
        Lua.UnregisterFunction("Save");
    }
}
