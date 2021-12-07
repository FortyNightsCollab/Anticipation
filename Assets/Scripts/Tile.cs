using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] Material selectMaterial;

    int location;
    public int Location { get { return location; } set { location = value; } }
    List<Tile> surroundings;
    MeshRenderer meshRenderer;
    Material defaultMaterial;
    List<Attack> targetForAttacks = new List<Attack>();
    BoxCollider boxCollider;
    Selectable selectable;

    private void Start()
    {
        selectable = GetComponent<Selectable>();
        selectable.IsSelectable = false;
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.material;
    }

    private void Update()
    {
        
    }

    IEnumerator FlipCollision()
    {
        while(true)
        {
            boxCollider.enabled = !boxCollider.enabled;
            boxCollider.enabled = !boxCollider.enabled;
            yield return new WaitForSeconds(5f);
        }
    }

    public void Mark(Attack attack)
    {
        Debug.Log("Tile Marked for Attack");
        targetForAttacks.Add(attack);
    }

    public void AttackAvailableTargets()
    {
        boxCollider.enabled = false;
        boxCollider.enabled = true;
    }
   
    public void ClearTarget(Attack attack)
    {
        if(targetForAttacks.Contains(attack)) targetForAttacks.Remove(attack);
        Debug.Log("Attack cleared");
    }

    public List<Attack> GetAttacks(int team)
    {
        return targetForAttacks;
    }

    public void Select(SelectState newSelectState)
    {
        switch(newSelectState)
        {
            case SelectState.INITIATE:               
                selectable.IsSelectable = true;
                selectable.Select(newSelectState);
                break;

            case SelectState.ATTACK:
                selectable.IsSelectable = true;
                selectable.Select(newSelectState);
                break;

            case SelectState.HOVEROFF:
                selectable.Select(SelectState.HOVEROFF);
                break;

            case SelectState.OFF:
                selectable.Select(newSelectState);
                selectable.IsSelectable = false;
                break;
        }

        
    }

    public bool SelectableForAction()
    {
        if (selectable.CurrentState == SelectState.INITIATE) return true;
        else return false;
    }
    
    public void IsSelectable(bool on)
    {
        selectable.enabled = on;
    }
}

public class TileLocation
{
    public int xIndex;
    public int yIndex;

    public TileLocation(int xIndex, int yIndex)
    {
        this.xIndex = xIndex;
        this.yIndex = yIndex;
    }
}

public enum TileDirection: byte
{
    NORTH = 0x01,
    NORTHEAST = 0x02,
    EAST = 0x04,
    SOUTHEAST = 0x08,
    SOUTH = 0x10,
    SOUTHWEST = 0x20,
    WEST = 0x40,
    NORTHWEST = 0x80,
    ALL = 0xFF,
    NONE = 0x00

}



