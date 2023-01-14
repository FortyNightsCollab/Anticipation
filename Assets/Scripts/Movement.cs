using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] int movementSpaces;
    [SerializeField] float speed;
    [SerializeField] GameObject pathPoint;

    bool queuedForMovement = false;
    public bool QueuedForMovement { get { return queuedForMovement; } }

    MapSelection movementSelection;
    public MapSelection MovementSelection { get { return movementSelection; } }

    Tile tileStartPosition;
    public Tile TileStartPosition { get { return tileStartPosition; } }

    Tile tileDestination;
    public Tile TileDestination { get { return tileDestination; } set { tileDestination = value; } }

    List<Tile> tilesInRange = new List<Tile>();

    List<GameObject> path = new List<GameObject>();

    List<Tile> potentialPath = new List<Tile>();

    Vector3 destination;
    float destinationDistance;
  

    bool enroute;
    public bool Enroute { get { return enroute; } }

    // Start is called before the first frame update
    void Start()
    {
        movementSelection = new MapSelection(new RowOffest(-movementSpaces, -movementSpaces), (movementSpaces * 2) + 1, (movementSpaces * 2) + 1);
    }

    public void RefreshTilesInRange(Map map, Tile location)
    {
        tileStartPosition = location;
        tilesInRange.Clear();
        map.TileHighlight(location, tilesInRange, movementSelection, SelectState.NOCHANGE);
    }

    public void StopAtNextPoint()
    {
        if(path.Count > 1)
        {       
            while(path.Count > 1)
            {
                Destroy(path[1]);
                path.RemoveAt(1);
            }
        }
    }

    private void Update()
    {
        if(enroute && path.Count > 0)
        {
            Vector3 currentPosition = gameObject.transform.position;

            Vector3 movementVector = path[0].transform.position - currentPosition;

            float distanceToDestination = movementVector.magnitude;
            movementVector.Normalize();
       
            Vector3 frameDistance = movementVector * speed * Time.deltaTime;

            if(distanceToDestination < 0.01f)
            {
                Destroy(path[0]);
                path.RemoveAt(0);
                if (path.Count <= 0)
                    enroute = false;
            }

            else
                gameObject.transform.Translate(frameDistance);
        }
    }


    public void ShowPath(bool enable)
    {
        if(path.Count > 0 )
        {
            foreach(GameObject point in path)
            {
                point.SetActive(enable);
            }
        }
    }

    public bool SetDestination(Tile newDestination, Map map)
    {
        if (tilesInRange.Contains(newDestination))
        {
            TileDestination = newDestination;
            destination = newDestination.transform.position;
            destination.y = transform.position.y;

            foreach (GameObject point in path)
            {
                Destroy(point);
            }

            path.Clear();
            StartCoroutine(RenderPath(CalculatePath(newDestination, map)));

            queuedForMovement = true;

            return true;
        }
        return false;
    }

    public bool HighlightPotentialPath(Tile newDestination, Map map)
    {
        if (tilesInRange.Contains(newDestination))
        {
            if (potentialPath.Count > 0)
            {
                foreach (Tile tile in potentialPath)
                {
                    Selectable select = tile.GetComponent<Selectable>();
                    select.Select(SelectState.HOVEROFF);
                }
                potentialPath.Clear();
            }
            potentialPath = CalculatePath(newDestination, map);

            foreach(Tile tile in potentialPath)
            {
                Selectable select = tile.GetComponent<Selectable>();
                select.Select(SelectState.HOVERON);
            }

            return true;
        }

        else
        {
            foreach (Tile tile in potentialPath)
            {
                Selectable select = tile.GetComponent<Selectable>();
                select.Select(SelectState.HOVEROFF);
            }
            potentialPath.Clear();
        }
        return false;
    }

    public void Move()
    {
        Collider movementCollider = gameObject.GetComponent<BoxCollider>();
        movementCollider.enabled = false;
        
   //     SetDestination(tileDestination);
        movementCollider.enabled = true;

        enroute = true;
    }

    public void SetNewTurnPosition()
    {
        tileStartPosition = tileDestination;
    
    }

    public void ShowTilesInRange(bool on)
    {
        foreach(Tile tile in tilesInRange)
        {        
            if (on) tile.Select(SelectState.INITIATE);
            else tile.Select(SelectState.OFF);              
        }
    }

    IEnumerator RenderPath(List<Tile> calculatedPath)
    {
        foreach (Tile tile in calculatedPath)
        {
            Vector3 pathPointPosition = tile.transform.position;
            pathPointPosition.y = transform.position.y;

            GameObject generatedPoint = Instantiate(pathPoint, pathPointPosition, Quaternion.identity);
            if (generatedPoint)
            {
                path.Add(generatedPoint);
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private List<Tile> CalculatePath(Tile destinationTile, Map map)
    {
        List<Tile> calculatedPath = new List<Tile>();

        if (tilesInRange.Contains(destinationTile))
        {
            //Get map indices of start/destination tiles
            int destinationTileX = destinationTile.Location & 0x0000FFFF;
            int destinationTileY = (destinationTile.Location & 0x7FFF0000) >> 16;
            int startTileX = tileStartPosition.Location & 0x0000FFFF;
            int startTileY = (tileStartPosition.Location & 0x7FFF0000) >> 16;

            //Find out how far apart in columns and rows
            int differenceX = destinationTileX - startTileX;
            int differenceY = destinationTileY - startTileY;

            int nextTileX = startTileX;
            int nextTileY = startTileY;


            while (Mathf.Abs(differenceX) > 0 || Mathf.Abs(differenceY) > 0)
            {
                bool xMotion = false;
                bool yMotion = false;

                if (Mathf.Abs(differenceX) > Mathf.Abs(differenceY))
                {
                    if (differenceX > 0) nextTileX++;
                    else nextTileX--;

                    xMotion = true;
                }

                else if (Mathf.Abs(differenceX) < Mathf.Abs(differenceY))
                {
                    if (differenceY > 0) nextTileY++;
                    else nextTileY--;

                    yMotion = true;
                }

                else
                {
                    if (differenceX > 0) nextTileX++;
                    else nextTileX--;

                    if (differenceY > 0) nextTileY++;
                    else nextTileY--;

                    xMotion = true;
                    yMotion = true;
                }

                foreach (Tile tile in tilesInRange)
                {
                    int tileX = tile.Location & 0x0000FFFF;
                    int tileY = (tile.Location & 0x7FFF0000) >> 16;

                    if (tileX == nextTileX && tileY == nextTileY)
                    {
                   
                        calculatedPath.Add(tile);

                        if (xMotion)
                        {
                            if (differenceX > 0) differenceX--;
                            else differenceX++;
                        }

                        if (yMotion)
                        {
                            if (differenceY > 0) differenceY--;
                            else differenceY++;
                        }
                        break;
                    }
                }
            }
        }

        return calculatedPath;
        
      
    }

}
