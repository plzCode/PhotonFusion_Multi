using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class OutlineController : MonoBehaviour
{
    [SerializeField] Material _outlineMat;
    List<List<Material>> _originMats;    // 현재는 단일 머테리얼에만 정상적으로 적용
    Renderer[] _renderers;

    public bool OutlineActive = false;
    bool _wasOutlineActive = false;
    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originMats = new List<List<Material>>(_renderers.Length);

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originMats.Add(new List<Material>(_renderers[i].materials));
        }
    }

    void Update()
    {
        bool hasChanged = OutlineActive != _wasOutlineActive;
        if (hasChanged)
        {
            _wasOutlineActive = OutlineActive;
            SetOutline(OutlineActive);
        }
    }

    public void SetOutline(bool enabled)
    {
        if (enabled)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                List<Material> currentMats = _originMats[i];

                if (currentMats[^1] != _outlineMat)
                    currentMats.Add(_outlineMat);

                _renderers[i].SetMaterials(
                    currentMats
                );
                _renderers[i].gameObject.layer = LayerMask.NameToLayer("Outline");
            }
        }
        else
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                List<Material> currentMats = _originMats[i];

                if (currentMats[^1] == _outlineMat)
                    currentMats.RemoveAt(currentMats.Count - 1);

                _renderers[i].SetMaterials(currentMats);
                _renderers[i].gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }
}
