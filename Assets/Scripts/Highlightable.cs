using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlightable : MonoBehaviour
{
    [SerializeField] Material selectMaterial;
    [SerializeField] Material attackMaterial;
    [SerializeField] Material confirmMaterial;

    MeshRenderer meshRenderer;
    Material defaultMaterial;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        defaultMaterial = meshRenderer.material;
    }

    public void Highlight(HighlightType type)
    {
        switch(type)
        {
            case HighlightType.OFF:
                meshRenderer.material = defaultMaterial;
                break;

            case HighlightType.SOLID:
                meshRenderer.material = selectMaterial;
                break;

            case HighlightType.ATTACK:
                meshRenderer.material = attackMaterial;
                break;

            case HighlightType.CONFIRM:
                meshRenderer.material = confirmMaterial;
                break;
        }
     
    }
}

public enum HighlightType
{
    OFF,
    SOLID,
    ATTACK,
    CONFIRM
}
