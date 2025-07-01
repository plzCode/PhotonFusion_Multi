#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(ZombieSpawnPivot))]
public class AlignToNavMesh : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Align to NavMesh"))
        {
            var pivot = (ZombieSpawnPivot)target;
            if (NavMesh.SamplePosition(pivot.transform.position,
                                       out var hit, 2f, NavMesh.AllAreas))
            {
                Undo.RecordObject(pivot.transform, "Align Pivot");
                pivot.transform.position = hit.position;
            }
        }
    }
}
#endif
