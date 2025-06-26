using System.Collections.Generic;
using UnityEngine;

public class RemoveHallwayInvisibleWalls : MonoBehaviour
{
    public List<string> nameFilters = new List<string> { "HH_Sector_Big_01_Collider", "HH_Sector_Big_02_Collider" };
    public float maxY = 25f;

    private List<GameObject> removedWalls = new List<GameObject>();

    void Start()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            foreach (var filter in nameFilters)
            {
                if (obj.name.Contains(filter))
                {
                    Collider col = obj.GetComponent<Collider>();
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    float yPos = obj.transform.position.y;

                    Debug.Log($"[체크중] {obj.name} | Y={yPos} | Collider={(col != null)} | Renderer={(renderer != null)}");

                    if (col != null && renderer == null && yPos <= maxY)
                    {
                        col.enabled = false;
                        removedWalls.Add(obj);
                        Debug.Log($"🧹 비활성화된 투명벽: {obj.name} @ Y={yPos}");
                        count++;
                    }

                    break; // 필터 하나에만 해당되면 더 안 봐도 됨
                }
            }
        }

        Debug.Log($"✅ 복도 투명벽 비활성화 완료: {count}개");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var wall in removedWalls)
            {
                if (wall != null)
                {
                    Collider col = wall.GetComponent<Collider>();
                    if (col != null)
                        col.enabled = true;
                }
            }

            Debug.Log("🔁 비활성화된 투명벽 복원 완료");
        }
    }
}