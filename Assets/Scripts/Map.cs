using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{

    [SerializeField] GameObject flatTile;
    [SerializeField] int width;
    public int Width { get { return width; } }

    [SerializeField] int height;
    public int Height { get { return height; } }

    [SerializeField] MapCamera mapCamera;

    GameObject First;
    Dictionary<int, Tile> mapData = new Dictionary<int, Tile>();
    public Dictionary<int, Tile> MapData { get { return mapData; } }

    GameObject selectedObject;
    Vector2 tileSize;
    List<Tile> surroundingTiles = new List<Tile>();
    List<GameObject> unitsToMove = new List<GameObject>();
    List<GameObject> unitsToAttack = new List<GameObject>();
    CenteredSelection surroundingSelection;
    Highlightable tempHighlight;
    Highlightable confirmAction;
    List<Selectable> hoverSelect = new List<Selectable>();
    Vector3 previousMousePosition;
    bool bMovementSelection;
    bool runningTurn = false;
    
    

    // Start is called before the first frame update
    void Start()
    {
        
        surroundingSelection = new CenteredSelection(2, 1, 2, 1, 2, 1, 2, 1);
        GameObject createdObject;
        Tile createdTile;
        BoxCollider collider = flatTile.GetComponent<BoxCollider>();
        float xSize = collider.size.x * 1.01f;
        float zSize = collider.size.z * 1.01f;
        tileSize = new Vector2(xSize, zSize);

        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                createdObject = Instantiate(flatTile, new Vector3(xSize * x, 0.0f, zSize * y), Quaternion.identity);
                createdTile = createdObject.GetComponent<Tile>();
                int createdTileLocation;


                if (createdTile!=null)
                {
                    createdTileLocation = (y << 16);              
                    createdTileLocation |= x;
                                     
                    mapData.Add(createdTileLocation, createdTile);
                    createdTile.Location = createdTileLocation;
                }

            }
        }
      
    }

    // Update is called once per frame
    void Update()
    {
        if(!runningTurn) SetupTurn();

        else ProcessTurn();
    }

    void ProcessTurn()
    {
        foreach(GameObject objectToMove in unitsToMove)
        {
            Movement movement = objectToMove.GetComponent<Movement>();
            if(movement.Enroute) { return; }
            movement.SetNewTurnPosition();
        }    

        foreach (GameObject attackObject in unitsToAttack)
        {
            Attack attack = attackObject.GetComponent<Attack>();
            if(unitsToMove.Contains(attackObject))
            {
                attack.AttackAvailableTargets();
            }
            attack.ClearAttack();
        }

        unitsToAttack.Clear();
        unitsToMove.Clear();
        runningTurn = false;

    }

    void SetupTurn()
    {
        RaycastHit hitData = mapCamera.GetRayHitResult();
        GameObject hitObject;

        if (hoverSelect.Count > 0)                                                            //Check for a previous hover selection
        {
          
            Tile hoverTile = hoverSelect[0].gameObject.GetComponent<Tile>();

            if (hoverTile)                                                          //If previous hover selection part of 
            {                                                                       //potential action selections
                foreach (Selectable selectable in hoverSelect)
                {
                    if (selectable.CurrentState != SelectState.ATTACK) selectable.Select(SelectState.INITIATE);

                    else selectable.Select(SelectState.HOVEROFF);
                }
            }

            else                                                                   //Otherwise turn selection completely off
            {
                foreach (Selectable selectable in hoverSelect)
                {
                    selectable.Select(SelectState.HOVEROFF);
                }
            }
            hoverSelect.Clear();                                                   //hover selection deselected every frame 
        }

        if (hitData.collider)                                                      //If raycast hit something
        {
            hitObject = hitData.collider.gameObject;                               
           
            Selectable selectable = hitObject.GetComponent<Selectable>();           

            if (selectable)                                                        //If hit object is selectable      
            {
                Tile selectedTile = selectable.gameObject.GetComponent<Tile>();

                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (selectedObject != null)
                    {
                        ClearSelection(false);
                        Unit selectedUnit = selectedObject.GetComponent<Unit>();

                        if (selectedUnit)
                        {

                            if (bMovementSelection)
                            {

                                Attack attack = selectedUnit.GetComponent<Attack>();
                                Movement movement = selectedObject.GetComponent<Movement>();

                                selectedUnit.Select();
                            
                            }


                            else
                            {

                                Movement movement = selectedObject.GetComponent<Movement>();

                                selectedUnit.Select();

                                /*
                                if (movement)
                                {
                                    for (int i = 0; i < 8; i++)
                                    {
                                        TileHighlight(movement.TileStartPosition, surroundingTiles, movement.MovementSelection, SelectState.INITIATE, false);
                                    }
                                }
                                */

                            }

                            bMovementSelection = !bMovementSelection;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.T))
                {
                    foreach(GameObject attackObject in unitsToAttack)
                    {
                        if (!unitsToMove.Contains(attackObject))
                        {
                            Attack attack = attackObject.GetComponent<Attack>();
                            attack.ArmTilesForAttack();
                        }
                    }

                    foreach(GameObject moveObject in unitsToMove)
                    {
                        Movement movement = moveObject.GetComponent<Movement>();
                        Debug.Log("Move units");
                        movement.ReturnToStart();
                        movement.Move();
                    }

                    runningTurn = true;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Movement movement;
                        
                    if (selectedTile && surroundingTiles.Contains(selectedTile))   //surroundTiles only populated if
                    {                                                              //selectable object was selected in previous frame
                        Attack unitAttack = selectedObject.GetComponent<Attack>();
                        movement = selectedObject.GetComponent<Movement>();
                        Selectable selectedObjectSelectable = selectedObject.GetComponent<Selectable>();

                        if (bMovementSelection)
                        {
                            movement = selectedObject.GetComponent<Movement>();                          
                            movement.SetDestination(selectedTile.transform.position);
                            ClearSelection(false);
                            selectedObjectSelectable.Select(SelectState.OFF);
                            selectedObject = null;                                     //previous selected object performed action... deselect
                            unitAttack.ClearAttack();
                            unitsToMove.Add(movement.gameObject);
                            movement.TileDestination = selectedTile;
                        }

                        else
                        {

                            MapSelection attackTiles = GenerateSelectionBetweenTwoTiles(movement.GetCurrentTileLocation(), selectedTile);
                            
                            if (unitAttack)
                            {
                                unitAttack.ClearAttack();
                                List<Tile> tempAttackTiles = new List<Tile>();                                 

                                ClearSelection(false);
                                selectedObjectSelectable.Select(SelectState.OFF);
                                selectedObject = null;
                                
                                TileHighlight(movement.GetCurrentTileLocation(), tempAttackTiles, attackTiles, SelectState.ATTACK, false);
                                
                                foreach(Tile tile in tempAttackTiles)
                                {
                                    unitAttack.AddCombatTile(tile);
                                }

                                if (!unitsToAttack.Contains(unitAttack.gameObject)) unitsToAttack.Add(unitAttack.gameObject);
                            }
                        }

                        
                    }                                                              

                    else                                                              
                    {                                                                  
                        movement = selectable.gameObject.GetComponent<Movement>();
                        Unit unitToSelect = selectable.gameObject.GetComponent<Unit>();
                                                                                       
                        ClearSelection(false);

                        

                        if (selectedObject)
                        {
                            Selectable selectedObjectSelectable = selectedObject.GetComponent<Selectable>();
                            selectedObjectSelectable.Select(SelectState.OFF);
                        }

                        if(unitToSelect)
                        {
                            unitToSelect.Select();
                            selectedObject = hitObject;                                             //new object selected
                            bMovementSelection = true;
                            mapCamera.ChangeFocalPoint(selectedObject.transform.position);
                            selectable.Select(SelectState.HOVERON);
                        }

                   

                        else selectedObject = null;                                     //an unselectable object was clicked clear current selection
                    }                                

                }
                    
                else
                {
                    if(selectedTile)
                    {
                        if(surroundingTiles.Contains(selectedTile))
                        {
                            selectable.Select(SelectState.HOVERON);                     //current hover object acceptable as action for 
                            hoverSelect.Add(selectable);                                //currently selected object

                            if (selectedObject && !bMovementSelection)
                            {
                                Movement movement = selectedObject.GetComponent<Movement>();

                                if (movement)
                                {
                                    Tile startingTile = movement.GetCurrentTileLocation();

                                    TileHighlight(startingTile, surroundingTiles, GenerateSelectionBetweenTwoTiles(startingTile, selectedTile), SelectState.HOVERON, true);

                                }

                            }
                        }

                     
                    }

                    else
                    {
                                                  
                        hoverSelect.Add(selectable);
                        if (hoverSelect[0].gameObject != selectedObject)
                        {
                            selectable.Select(SelectState.INITIATE);                            
                        }

                        
                    }
                }
            }          
        }
    }

    /*
    void SetupTurn()
    {
        RaycastHit hitData = mapCamera.GetRayHitResult();
        GameObject hitObject;

        if (hoverSelect.Count > 0)                                                            //Check for a previous hover selection
        {

            Tile hoverTile = hoverSelect[0].gameObject.GetComponent<Tile>();

            if (hoverTile)                                                          //If previous hover selection part of 
            {                                                                       //potential action selections
                foreach (Selectable selectable in hoverSelect)
                {
                    if (selectable.CurrentState != SelectState.ATTACK) selectable.Select(SelectState.INITIATE);

                    else selectable.Select(SelectState.HOVEROFF);
                }
            }

            else                                                                   //Otherwise turn selection completely off
            {
                foreach (Selectable selectable in hoverSelect)
                {
                    selectable.Select(SelectState.HOVEROFF);
                }
            }
            hoverSelect.Clear();                                                   //hover selection deselected every frame 
        }

        if (hitData.collider)                                                      //If raycast hit something
        {
            hitObject = hitData.collider.gameObject;

            Selectable selectable = hitObject.GetComponent<Selectable>();

            if (selectable)                                                        //If hit object is selectable      
            {
                Tile selectedTile = selectable.gameObject.GetComponent<Tile>();

                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (selectedObject != null)
                    {
                        ClearSelection(false);
                        if (bMovementSelection)
                        {

                            Attack attack = selectedObject.GetComponent<Attack>();
                            Movement movement = selectedObject.GetComponent<Movement>();

                            if (attack)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    TileHighlight(movement.GetCurrentTileLocation(), surroundingTiles, attack.GetMapSelection(i), SelectState.INITIATE, false);
                                }
                            }


                        }


                        else
                        {

                            Movement movement = selectedObject.GetComponent<Movement>();

                            if (movement)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    TileHighlight(movement.TileStartPosition, surroundingTiles, movement.MovementSelection, SelectState.INITIATE, false);
                                }
                            }

                        }

                        bMovementSelection = !bMovementSelection;
                    }
                }

                if (Input.GetKeyDown(KeyCode.T))
                {
                    foreach (GameObject attackObject in unitsToAttack)
                    {
                        if (!unitsToMove.Contains(attackObject))
                        {
                            Attack attack = attackObject.GetComponent<Attack>();
                            attack.ArmTilesForAttack();
                        }
                    }

                    foreach (GameObject moveObject in unitsToMove)
                    {
                        Movement movement = moveObject.GetComponent<Movement>();
                        Debug.Log("Move units");
                        movement.ReturnToStart();
                        movement.Move();
                    }

                    runningTurn = true;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Movement movement;

                    if (selectedTile && surroundingTiles.Contains(selectedTile))   //surroundTiles only populated if
                    {                                                              //selectable object was selected in previous frame
                        Attack unitAttack = selectedObject.GetComponent<Attack>();
                        movement = selectedObject.GetComponent<Movement>();
                        Selectable selectedObjectSelectable = selectedObject.GetComponent<Selectable>();

                        if (bMovementSelection)
                        {
                            movement = selectedObject.GetComponent<Movement>();
                            movement.SetDestination(selectedTile.transform.position);
                            ClearSelection(false);
                            selectedObjectSelectable.Select(SelectState.OFF);
                            selectedObject = null;                                     //previous selected object performed action... deselect
                            unitAttack.ClearAttack();
                            unitsToMove.Add(movement.gameObject);
                            movement.TileDestination = selectedTile;
                        }

                        else
                        {

                            MapSelection attackTiles = GenerateSelectionBetweenTwoTiles(movement.GetCurrentTileLocation(), selectedTile);

                            if (unitAttack)
                            {
                                unitAttack.ClearAttack();
                                List<Tile> tempAttackTiles = new List<Tile>();

                                ClearSelection(false);
                                selectedObjectSelectable.Select(SelectState.OFF);
                                selectedObject = null;

                                TileHighlight(movement.GetCurrentTileLocation(), tempAttackTiles, attackTiles, SelectState.ATTACK, false);

                                foreach (Tile tile in tempAttackTiles)
                                {
                                    unitAttack.AddCombatTile(tile);
                                }

                                if (!unitsToAttack.Contains(unitAttack.gameObject)) unitsToAttack.Add(unitAttack.gameObject);
                            }
                        }


                    }

                    else
                    {
                        movement = selectable.gameObject.GetComponent<Movement>();

                        ClearSelection(false);



                        if (selectedObject)
                        {
                            Selectable selectedObjectSelectable = selectedObject.GetComponent<Selectable>();
                            selectedObjectSelectable.Select(SelectState.OFF);
                        }

                        if (movement)
                        {
                            selectedObject = hitObject;                                             //new object selected
                            bMovementSelection = true;
                            mapCamera.ChangeFocalPoint(selectedObject.transform.position);
                            selectable.Select(SelectState.HOVERON);
                            TileHighlight(movement.TileStartPosition, surroundingTiles, movement.MovementSelection, SelectState.INITIATE, false);
                        }

                        else selectedObject = null;                                     //an unselectable object was clicked clear current selection
                    }

                }

                else
                {
                    if (selectedTile)
                    {
                        if (surroundingTiles.Contains(selectedTile))
                        {
                            selectable.Select(SelectState.HOVERON);                     //current hover object acceptable as action for 
                            hoverSelect.Add(selectable);                                //currently selected object

                            if (selectedObject && !bMovementSelection)
                            {
                                Movement movement = selectedObject.GetComponent<Movement>();

                                if (movement)
                                {
                                    Tile startingTile = movement.GetCurrentTileLocation();

                                    TileHighlight(startingTile, surroundingTiles, GenerateSelectionBetweenTwoTiles(startingTile, selectedTile), SelectState.HOVERON, true);

                                }

                            }
                        }


                    }

                    else
                    {

                        hoverSelect.Add(selectable);
                        if (hoverSelect[0].gameObject != selectedObject)
                        {
                            selectable.Select(SelectState.INITIATE);
                        }


                    }
                }
            }
        }
    }
    */
    void ClearSelection(bool allowAttackOverride)
    {
        foreach (Tile tile in surroundingTiles)
        {           
            Selectable tileSelectable = tile.GetComponent<Selectable>();

            if (tileSelectable)
            {
                if (tileSelectable.CurrentState != SelectState.ATTACK ||
                   (tileSelectable.CurrentState == SelectState.ATTACK && allowAttackOverride))
                {
                    tileSelectable.Select(SelectState.OFF);
                }
            }
        }
        surroundingTiles.Clear();
    }

    void TileHighlight(Tile start, CenteredSelection selection)
    {
        Tile selectedTile = start;
        int tileLocation = selectedTile.Location;
        int tileIndexX = tileLocation & 0x0000FFFF;
        int tileIndexY = (tileLocation & 0x7FFF0000) >> 16;
        int[] directions = selection.GetDirections();
        Tile surroundingTile;
        Vector3 previousTileLocation;
        int adjustedTileLocation = -1;

        for (int i = 0; i < directions.Length; i++)
        {
            previousTileLocation = selectedTile.transform.position;
            for (int j = 0, offset = 1; j < directions[i]; j++, offset++)
            {
                switch (i)
                {
                    case 0:      //North one up
                        adjustedTileLocation = ((tileIndexY + offset) * width) + tileIndexX;
                        break;

                    case 1:     //Northeast one up, one right
                        adjustedTileLocation = ((tileIndexY + offset) * width) + tileIndexX + offset;
                        break;

                    case 2:     //East one right
                        adjustedTileLocation = (tileIndexY * width) + tileIndexX + offset;
                        break;

                    case 3:     //Southeast one down, one right
                        adjustedTileLocation = ((tileIndexY - offset) * width) + tileIndexX + offset;
                        break;

                    case 4:     //South one down
                        adjustedTileLocation = ((tileIndexY - offset) * width) + tileIndexX;
                        break;

                    case 5:     //Southwest one down, one left
                        adjustedTileLocation = ((tileIndexY - offset) * width) + tileIndexX - offset;
                        break;

                    case 6:     //West one left
                        adjustedTileLocation = (tileIndexY * width) + tileIndexX - offset;
                        break;

                    case 7:     //Northest one up, one left
                        adjustedTileLocation = ((tileIndexY + offset) * width) + tileIndexX - offset;
                        break;
                }


                if (mapData.ContainsKey(adjustedTileLocation))
                {

                    surroundingTile = mapData[adjustedTileLocation];
                    if (Vector3.Distance(surroundingTile.transform.position, previousTileLocation) < (tileSize.x * 2.0f))
                    {
                        Selectable selectable = surroundingTile.GetComponent<Selectable>();

                        if (selectable)
                        {
                            selectable.Select(SelectState.INITIATE);
                        }
                        surroundingTiles.Add(surroundingTile);
                        previousTileLocation = surroundingTile.transform.position;
                    }
                }

                else
                {
                    Debug.Log("Tile not found");
                }
            }

        }
        
    }

    MapSelection GenerateSelectionBetweenTwoTiles(Tile startTile, Tile stopTile)
    {
        int tileLocation = startTile.Location;
        int objectIndexX = tileLocation & 0x0000FFFF;
        int objectIndexY = (tileLocation & 0x7FFF0000) >> 16;

        tileLocation = stopTile.Location;
        int mapIndexX = tileLocation & 0x0000FFFF;
        int mapIndexY = (tileLocation & 0x7FFF0000) >> 16;

        MapSelection mapSelection;

        if (objectIndexY < mapIndexY && objectIndexX == mapIndexX)
        {
            mapSelection = new MapSelection(new RowOffest(0, 1), 1, mapIndexY - objectIndexY);
        }

        else if(objectIndexY > mapIndexY && objectIndexX == mapIndexX)
        {
            mapSelection = new MapSelection(new RowOffest(0, mapIndexY - objectIndexY), 1, objectIndexY - mapIndexY);
        }

        else if(objectIndexX < mapIndexX && objectIndexY == mapIndexY)
        {
            mapSelection = new MapSelection(new RowOffest(1, 0), mapIndexX - objectIndexX, 1);
        }

        else if(objectIndexX > mapIndexX && objectIndexY == mapIndexY)
        {
            mapSelection = new MapSelection(new RowOffest(mapIndexX - objectIndexX, 0), objectIndexX - mapIndexX, 1);
        }

        else
        {
            mapSelection = new MapSelection(new RowOffest(0, 0), 1, 1);
        }

        return mapSelection;
    }


    public void TileHighlight(Tile start, List<Tile> tileGroup, MapSelection selection, SelectState selectStateToUse, bool allowAttackOverride = true)
    {
        Tile selectedTile = start;

        int tileLocation = selectedTile.Location;
        int tileIndexX = tileLocation & 0x0000FFFF;
        int tileIndexY = (tileLocation & 0x7FFF0000) >> 16;
        Tile surroundingTile;
        Vector3 previousTileLocation;
        int adjustedTileLocation = -1;
        int currentRow = 0;
        SelectionRow row = selection.GetRow(currentRow);

        while(row.width > 0)
        {
            previousTileLocation = selectedTile.transform.position;


            for (int i = 0; i < row.width; i++)
            {
              
                int mapIndexX = tileIndexX + row.offset.x + i;
                int mapIndexY = tileIndexY + row.offset.y;

                if (mapIndexX < 0 || mapIndexY < 0 || mapIndexX >= width || mapIndexY >= height) continue;

                adjustedTileLocation = (mapIndexY << 16) | mapIndexX;               

                if (mapData.ContainsKey(adjustedTileLocation))
                {

                    surroundingTile = mapData[adjustedTileLocation];
             //       if (Vector3.Distance(surroundingTile.transform.position, previousTileLocation) < (tileSize.x * 2.0f))
              //      {
                        Selectable selectable = surroundingTile.GetComponent<Selectable>();

                        if (selectable)
                        {
                            if (selectable.CurrentState != SelectState.ATTACK ||
                                (selectable.CurrentState == SelectState.ATTACK && allowAttackOverride))
                            {
                                selectable.Select(selectStateToUse);
                            }         
                            
                            if(selectStateToUse == SelectState.HOVERON) hoverSelect.Add(selectable);
                        }
                        tileGroup.Add(surroundingTile);
                        previousTileLocation = surroundingTile.transform.position;
              //      }
                }

                else
                {
                    Debug.Log("Tile not found");
                }


            }

            
            currentRow++;
            row = selection.GetRow(currentRow);
        }

    }
}
   
