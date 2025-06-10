using UnityEngine;
using System.Collections.Generic;

public class OutlineController : MonoBehaviour
{
    [SerializeField] Material _outlineMat;
    Material[] _originMats;    // 현재는 단일 머테리얼에만 정상적으로 적용

    Renderer[] _renderers;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originMats = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originMats[i] = _renderers[i].material;
        }
    }

    public void SetOutline(bool enabled)
    {
        if (enabled)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetMaterials(
                    new List<Material> { _originMats[i], _outlineMat }
                );
                gameObject.layer = LayerMask.NameToLayer("Outline");
            }
        }
        else
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetMaterials(new List<Material> { _originMats[i] });
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }
}
