using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] int movementSpaces;
    [SerializeField] float speed;

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

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        destination.y = transform.position.y;
        Vector3 currentPosition = gameObject.transform.position;

        destinationDistance = Vector3.Distance(destination, currentPosition);

        enroute = true;
    }

    public void Move()
    {
        Collider movementCollider = gameObject.GetComponent<BoxCollider>();
        movementCollider.enabled = false;
        
        SetDestination(tileDestination.gameObject.transform.position);
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

                SetDestination(tileSteppedOn.transform.position);
            }

            tileLocation = tileSteppedOn;
        }
    }

}
