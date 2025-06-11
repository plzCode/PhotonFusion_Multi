using System.Collections.Generic;
using UnityEngine;

public static class ZombieConfigRegistry
{
    private static readonly List<ZombieConfig> list = new();
    private static readonly Dictionary<ZombieConfig, int> map = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BuildTable()
    {
        list.Clear(); map.Clear();
        var configs = Resources.LoadAll<ZombieConfig>("");
        for (int i = 0; i < configs.Length; i++)
        {
            list.Add(configs[i]);
            map[configs[i]] = i;  // ID = index
        }
        Debug.Log($"[Registry] Zombie configs loaded: {list.Count}");
    }

    public static int GetId(ZombieConfig cfg) => map[cfg];
    public static ZombieConfig GetConfig(int id) =>
        id >= 0 && id < list.Count ? list[id] : null;
}
