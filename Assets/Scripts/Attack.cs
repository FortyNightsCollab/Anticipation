using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] int attackSpaces;
    [SerializeField] AttackShape attackShape;
    [SerializeField] GameObject attackEffect;

    MapSelection[] attackSelection;
    List<Tile> combatTiles = new List<Tile>();
    public List<Tile> CombatTiles { get { return combatTiles; } }

    public List<Tile> tilesInRange = new List<Tile>();


Vector3 destination;
    float destinationDistance;

    Tile tileLocation;
    bool enroute;

    // Start is called before the first frame update
    void Start()
    {
        
        GenerateDirectionSelections();
        
    }

    private void Update()
    {
       
    }

    public void RefreshTilesInRange(Map map, Tile currentLocation)
    {
        for(int i = 0; i < attackSelection.Length; i++)
            map.TileHighlight(currentLocation, tilesInRange, attackSelection[i], SelectState.NOCHANGE);
    }

    public void AddCombatTile(Tile tileToAdd)
    {
        combatTiles.Add(tileToAdd);      
    }

    public void ArmTilesForAttack()
    {
        foreach(Tile tile in combatTiles)
        {
            tile.Mark(this);
        }
    }

    public void AttackAvailableTargets()
    {    
        foreach (Tile tile in combatTiles)
        {
            tile.Mark(this);
            tile.AttackAvailableTargets();
        }
    }

    public void SteppedOnTile(Tile tile)
    {
        Debug.Log("Step");
        tileLocation = tile;
    }

    public Tile GetTileLocation()
    {
        return tileLocation;
    }

    public void ClearAttack()
    {
        foreach(Tile tile in combatTiles)
        {
            Selectable tileSelectable = tile.GetComponent<Selectable>();
            tileSelectable.Select(SelectState.OFF);
            tile.ClearTarget(this);
        }
        combatTiles.Clear();
    }

    public void SetDestination(Vector3 newDestination)
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        Tile tileSteppedOn = collider.GetComponent<Tile>();

        if (tileSteppedOn)
        {
            tileLocation = tileSteppedOn;
        }
    }

    public MapSelection GetMapSelection(int direction)
    {
        return attackSelection[direction];
    }

    void GenerateDirectionSelections()
    {
        attackSelection = new MapSelection[8];
        switch(attackShape)
        {
            case AttackShape.LINE:
                attackSelection[0] = new MapSelection(new RowOffest(0, 1), 1, attackSpaces);
                attackSelection[1] = new MapSelection(new RowOffest(0, 1), 1, attackSpaces);
                attackSelection[2] = new MapSelection(new RowOffest(1, 0), attackSpaces, 1);
                attackSelection[3] = new MapSelection(new RowOffest(0, -attackSpaces), 1, attackSpaces);
                attackSelection[4] = new MapSelection(new RowOffest(0, -attackSpaces), 1, attackSpaces);
                attackSelection[5] = new MapSelection(new RowOffest(0, -attackSpaces), 1, attackSpaces);
                attackSelection[6] = new MapSelection(new RowOffest(-attackSpaces, 0), attackSpaces, 1);
                attackSelection[7] = new MapSelection(new RowOffest(0, 0), 1, attackSpaces);

                break;

            case AttackShape.BOX:
                break;

            case AttackShape.CONE:
                break;
        }
    }

    public void Use(Tile tileToAttack)
    {
        if (combatTiles.Contains(tileToAttack))
        {
            Vector3 position = tileToAttack.transform.position;
            GameObject spawnedSystem = Instantiate(attackEffect, position, Quaternion.identity);
            Destroy(spawnedSystem, 2.0f);
        }
    }
}

public enum AttackShape
{
    LINE,
    CONE,
    BOX
}

