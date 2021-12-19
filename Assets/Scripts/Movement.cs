using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] int movementSpaces;
    [SerializeField] float speed;
    [SerializeField] GameObject pathPoint;
    
    MapSelection movementSelection;
    public MapSelection MovementSelection { get { return movementSelection; } }
    List<Tile> motionTiles = new List<Tile>();
    public List<Tile> MotionTiles { get { return motionTiles; } }

    Tile tileStartPosition;
    public Tile TileStartPosition { get { return tileStartPosition; } }

    Tile tileDestination;
    public Tile TileDestination { get { return tileDestination; } set { tileDestination = value; } }

    List<Tile> tilesInRange = new List<Tile>();

    Vector3 destination;
    float destinationDistance;

    Vector3 turnStartPosition;

    List<GameObject> path = new List<GameObject>();

    Tile tileLocation;
    bool enroute;
    public bool Enroute { get { return enroute; } }

    // Start is called before the first frame update
    void Start()
    {
        movementSelection = new MapSelection(new RowOffest(-movementSpaces, -movementSpaces), (movementSpaces * 2) + 1, (movementSpaces * 2) + 1);
    }

    public void RefreshTilesInRange(Map map)
    {
        tilesInRange.Clear();
        map.TileHighlight(tileLocation, tilesInRange, movementSelection, SelectState.NOCHANGE);
    }

    public List<Tile> GetTilesInRange()
    {
        return tilesInRange;
    }

    private void Update()
    {
        if(destinationDistance > 0.0f)
        {
            Vector3 currentPosition = gameObject.transform.position;

            Vector3 movementVector = destination - currentPosition;

            movementVector.Normalize();
            Debug.Log("Speed: " + speed);
            Vector3 frameDistance = movementVector * speed * Time.deltaTime;
            destinationDistance -= frameDistance.magnitude;

            if(destinationDistance < 0.0f)
            {
                enroute = false;
            }

            gameObject.transform.Translate(frameDistance);
        }
    }

    public void AddMotionTile(Tile tileToAdd)
    {
        motionTiles.Add(tileToAdd);
    }

    public void SteppedOnTile(Tile tile)
    {
        Debug.Log("Step");      

        tileLocation = tile;
    }

    public void ReturnToStart()
    {
        transform.position = turnStartPosition;      
    }

    public Tile GetCurrentTileLocation()
    {
        return tileLocation;
    }

    public void SetCurrentTileLocation(Tile newTileLocation)
    {
        tileLocation = newTileLocation;
    }

    public void SetDestination(Tile newDestination, Map map)
    {
        destination = newDestination.transform.position;
        destination.y = transform.position.y;

        foreach(GameObject point in path)
        {
            Destroy(point);
        }

        path.Clear();
        CalculatePath(newDestination, map);
    }

    public void Move()
    {
        Collider movementCollider = gameObject.GetComponent<BoxCollider>();
        movementCollider.enabled = false;
        
   //     SetDestination(tileDestination);
        movementCollider.enabled = true;
    }

    public void SetNewTurnPosition()
    {
        tileStartPosition = tileLocation;
        turnStartPosition = gameObject.transform.position;
        Debug.Log("Setting new start location");
    }

    public void Highlight(bool on)
    {
        foreach(Tile tile in tilesInRange)
        {        
            if (on) tile.Select(SelectState.INITIATE);
            else tile.Select(SelectState.OFF);              
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        Tile tileSteppedOn = collider.GetComponent<Tile>();
        Debug.Log("Stepped On: " + collider.gameObject);

        if (tileSteppedOn)
        {          
            if (tileLocation == null)
            {
                tileStartPosition = tileSteppedOn;
                turnStartPosition = transform.position;
            }

            List<Attack> attacksQueued = tileSteppedOn.GetAttacks(0);
            Debug.Log("number of attacks assigned: " + attacksQueued.Count);
            if(attacksQueued.Count > 0)
            {
                foreach(Attack attack in attacksQueued)
                {
                    attack.Use(tileSteppedOn);
                }

  //              SetDestination(tileSteppedOn);
            }
            tileLocation = tileSteppedOn;
        }
    }

    IEnumerator RenderPath(List<Vector3> calculatedPath)
    {
        foreach (Vector3 position in calculatedPath)
        {
           
            GameObject generatedPoint = Instantiate(pathPoint, position, Quaternion.identity);
            if (generatedPoint)
            {
                path.Add(generatedPoint);
                Debug.Log("Point added at: " + position);
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void CalculatePath(Tile destinationTile, Map map)
    {
        List<Vector3> calculatedPath = new List<Vector3>();

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
                        Vector3 pointToAdd = tile.transform.position;
                        pointToAdd.y = transform.position.y;

                        calculatedPath.Add(pointToAdd);

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

        StartCoroutine(RenderPath(calculatedPath));
      
    }

}
