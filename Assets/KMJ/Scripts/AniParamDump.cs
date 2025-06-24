#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public class AniParamDump : MonoBehaviour
{
    [ContextMenu("Dump Animator Params")]
    void Dump()
    {
        var anim = GetComponent<Animator>();
        foreach (var p in anim.parameters)
        {
            Debug.Log($"{p.name}  ({p.type})  hash={Animator.StringToHash(p.name)}");
        }
    }
}
#endif