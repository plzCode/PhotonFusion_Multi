using UnityEngine;
using System.Collections.Generic;

public class OutlineController : MonoBehaviour
{
    [SerializeField] Material _outlineMat;
    Material _originMat;    // ����� ���� ���׸��󿡸� ���������� ����

    Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originMat = _renderer.material;
    }

    public void SetOutline(bool enabled)
    {
        if (enabled)
        {
            _renderer.SetMaterials(
                new List<Material> { _originMat, _outlineMat }
            );
            gameObject.layer = LayerMask.NameToLayer("Outline");
        }
        else
        {
            _renderer.SetMaterials(new List<Material> { _originMat });
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
