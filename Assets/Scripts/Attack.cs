using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] int attackSpaces;
    [SerializeField] AttackShape attackShape;
    [SerializeField] GameObject attackEffect;
    [SerializeField] GameObject targetMarker;

    MapSelection[] attackSelection;

    bool queuedForAttack = false;
    public bool QueuedForAttack { get { return queuedForAttack; } }

    List<Tile> combatTiles = new List<Tile>();
    public List<Tile> CombatTiles { get { return combatTiles; } }

    public List<Tile> tilesInRange = new List<Tile>();

    List<GameObject> targets = new List<GameObject>();
    List<Tile> potentialTargets = new List<Tile>();

    Vector3 destination;
    float destinationDistance;

    Tile tileLocation;
    bool enroute;

    // Start is called before the first frame update
    void Start()
    {        
        GenerateDirectionSelections();       
    }

    public void ShowTargets(bool enable)
    {      
        foreach (GameObject target in targets)
        {
            MeshRenderer targetMeshRenderer = target.GetComponent<MeshRenderer>();
            targetMeshRenderer.enabled = enable;
        }
    }

    public void ShowTilesInRange(bool on)
    {
        foreach (Tile tile in tilesInRange)
        {
            if (!tile.IsMarkedForAttack())
            {
                if (on) tile.Select(SelectState.ATTACKPOTENTIAL);
                else tile.Select(SelectState.OFF);
            }
        }
    }

    public void RefreshTilesInRange(Map map, Tile currentLocation)
    {
        tileLocation = currentLocation;

        if(potentialTargets.Count > 0)
        {
            foreach(Tile potentialTarget in potentialTargets)
            {
                Destroy(potentialTarget);
            }
            potentialTargets.Clear();
        }

        tilesInRange.Clear();
        for(int i = 0; i < attackSelection.Length; i++)
            map.TileHighlight(currentLocation, tilesInRange, attackSelection[i], SelectState.NOCHANGE);
    }

    void AddCombatTile(Tile tileToAdd)
    {
        combatTiles.Add(tileToAdd);      
    }

    void ArmTilesForAttack()
    {
        foreach(Tile tile in combatTiles)
        {
            tile.Mark(this);
        }
    }

    void AttackAvailableTargets()
    {    
        foreach (Tile tile in combatTiles)
        {
            tile.Mark(this);
            tile.AttackAvailableTargets();
        }
    }

    public void ArmTargets(bool arm)
    {
        foreach(GameObject target in targets)
        {
            TargetMarker targetMarker = target.GetComponent<TargetMarker>();

            if(targetMarker)
            {
                targetMarker.EnableCollision(arm);
            }
        }
    }

    public void SteppedOnTile(Tile tile)
    {
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

    public void HighlightPotentialTargets(Tile tile, Map map)
    {

        if(potentialTargets.Count > 0)
        {
            foreach (Tile target in potentialTargets)
            {
                Selectable selectable = target.GetComponent<Selectable>();

                if (selectable)
                {
                    selectable.Select(SelectState.HOVEROFF);
                }
            }
            potentialTargets.Clear();
        }

      
        potentialTargets = CalculateTargets(tile, map);

        foreach(Tile target in potentialTargets)
        {
            Selectable selectable = target.GetComponent<Selectable>();
            
            if (selectable)
            {
                selectable.Select(SelectState.HOVERON);
            }
        }
    }

    public MapSelection GetMapSelection(int direction)
    {
        return attackSelection[direction];
    }

    /*
    public bool SetAttack(Tile targetTile, Tile attackStart)
    {
        if(tilesInRange.Contains(targetTile))
        {
            int targetTileX = targetTile.Location & 0x0000FFFF;
            int targetTileY = (targetTile.Location & 0x7FFF0000) >> 16;
            int attackStartX = attackStart.Location & 0x0000FFFF;
            int attackStartY = (attackStart.Location & 0x7FFF0000) >> 16;

            switch(attackShape)
            {
                case AttackShape.LINE:
                    int differenceX = targetTileX - attackStartX;
                    int differenceY = targetTileY - attackStartY;

                    if(Mathf.Abs(differenceY) == 0)
                    {
                        foreach (Tile tile in tilesInRange)
                        {
                            int tileX = tile.Location & 0x0000FFFF;
                            int tileY = (tile.Location & 0x7FFF0000) >> 16;

                            if (tileY == attackStartY)
                            {
                                if (differenceX > 0)
                                {
                                    if (tileX > attackStartX && tileX <= targetTileX)
                                    {
                                        AddCombatTile(tile);
                                        tile.Select(SelectState.INITIATE);
                                        Debug.Log("Added Tile to Combat Tiles");
                                    }
                                }

                                else
                                {
                                    if (tileX < attackStartX && tileX >= targetTileX)
                                    {
                                        AddCombatTile(tile);
                                        tile.Select(SelectState.INITIATE);
                                        Debug.Log("Added Tile to Combat Tiles");
                                    }
                                }

                                
                            }
                        }
                    }
                    break;
            }        
            return true;
        }

        return false;
    }
    */

    public void TriggerAttackAtMarker(GameObject marker)
    {
        if(targets.Contains(marker))
        {
            GameObject spawnedSystem = Instantiate(attackEffect, marker.transform.position, Quaternion.identity);
            Destroy(spawnedSystem, 2.0f);
        }
    }

    public bool SetAttack()
    {
        if(targets.Count > 0)
        {
            foreach(GameObject target in targets)
            {
                Destroy(target);
            }
            targets.Clear();
        }

        foreach(Tile tile in potentialTargets)
        {
            Vector3 targetPosition = tile.transform.position;
            targetPosition.y = transform.position.y;

            GameObject target = Instantiate(targetMarker, targetPosition, Quaternion.identity);

            if(target)
            {
                targets.Add(target);

                TargetMarker targetMarker = target.GetComponent<TargetMarker>();
                Unit unitOwner = gameObject.GetComponent<Unit>();

                if (targetMarker && unitOwner)
                {
                    targetMarker.AttackToTrigger = this;
                    targetMarker.EnableCollision(false);
                    
                }
            }
        }

        if (targets.Count > 0) queuedForAttack = true;

        return true;
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

    private List<Tile> CalculateTargets(Tile destinationTile, Map map)
    {
        List<Tile> calculatedTargets = new List<Tile>();

        if (tilesInRange.Contains(destinationTile))
        {
            //Get map indices of start/destination tiles
            int destinationTileX = destinationTile.Location & 0x0000FFFF;
            int destinationTileY = (destinationTile.Location & 0x7FFF0000) >> 16;
            int startTileX = tileLocation.Location & 0x0000FFFF;
            int startTileY = (tileLocation.Location & 0x7FFF0000) >> 16;

            //Find out how far apart in columns and rows
            int differenceX = destinationTileX - startTileX;
            int differenceY = destinationTileY - startTileY;

            int nextTileX = startTileX;
            int nextTileY = startTileY;
            bool tileFoundInRange = true;

            switch (attackShape)
            {
                case AttackShape.LINE:
                    while (tileFoundInRange)
                    {
                        tileFoundInRange = false;

                        if (differenceX > 0)
                        {
                            nextTileX++;
                        }

                        else if (differenceX < 0)
                        {
                            nextTileX--;
                        }

                        else if (differenceY > 0)
                        {
                            nextTileY++;
                        }

                        else if (differenceY < 0)
                        {
                            nextTileY--;
                        }

                        if ((nextTileX >= map.Width) || (nextTileX < 0) || (nextTileY >= map.Height) || (nextTileY < 0)) break;

                        foreach (Tile tile in tilesInRange)
                        {
                            int tileX = tile.Location & 0x0000FFFF;
                            int tileY = (tile.Location & 0x7FFF0000) >> 16;

                            if (nextTileX == tileX && nextTileY == tileY)
                            {
                                if (!calculatedTargets.Contains(tile))
                                {
                                    tileFoundInRange = true;
                                    calculatedTargets.Add(tile);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        return calculatedTargets;
    }


}

public enum AttackShape
{
    LINE,
    CONE,
    BOX
}

