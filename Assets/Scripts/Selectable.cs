using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    [SerializeField] Material initiateMaterial;
    [SerializeField] Material confirmMaterial;
    [SerializeField] Material attackMaterial;

    MeshRenderer meshRenderer;
    Material defaultMaterial;
    Material storedMaterial;

    SelectState currentState;
    public SelectState CurrentState { get { return currentState; } }

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        defaultMaterial = meshRenderer.material;
        storedMaterial = defaultMaterial;
        currentState = SelectState.OFF;
    }

    public void Select(SelectState selectState)
    {
    
        switch (selectState)
        {
            case SelectState.NOCHANGE:
                return;

            case SelectState.OFF:
                currentState = SelectState.OFF;
                meshRenderer.material = defaultMaterial;
                break;

            case SelectState.INITIATE:
                currentState = SelectState.INITIATE;
                meshRenderer.material = initiateMaterial;
                break;

            case SelectState.ATTACK:
                currentState = SelectState.ATTACK;
                meshRenderer.material = attackMaterial;
                break;

            case SelectState.HOVERON:
                storedMaterial = meshRenderer.material;
                meshRenderer.material = confirmMaterial;
                break;

            case SelectState.HOVEROFF:
                meshRenderer.material = storedMaterial;
                break;
        }
    }
   
}

public enum SelectState
{
    OFF,
    NOCHANGE,
    INITIATE,
    ATTACK,
    HOVERON,
    HOVEROFF
}
