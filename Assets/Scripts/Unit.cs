using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Attack))]
[RequireComponent(typeof(Movement))]
public class Unit : MonoBehaviour
{
    Tile tileLocation;
    int teamNum;
    Movement movement;
    Attack attack;
    Map map;
    bool selectedForMovement;

    // Start is called before the first frame update
    void Start()
    {
        map = FindObjectOfType<Map>();
        attack = GetComponent<Attack>();
        movement = GetComponent<Movement>();
     
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        movement.Highlight(true);
        selectedForMovement = true;
    }

    public void NextAction()
    {
        if(selectedForMovement)
        {
            movement.Highlight(false);
            attack.Highlight(true);
            selectedForMovement = false;
        }

        else
        {            
            attack.Highlight(false);
            movement.Highlight(true);
            selectedForMovement = true;
        }
    }

    public bool ProcessAction(GameObject actionObject)
    {
        Tile tile = actionObject.GetComponent<Tile>();
        if(tile)
        {
            if(selectedForMovement)
            {
                movement.SetDestination(tile.transform.position);
            }

            else
            {
           
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Tile tileSteppedOn = collider.GetComponent<Tile>();
        Debug.Log("Unit Stepped On: " + collider.gameObject);

        if (tileSteppedOn)
        {
            movement.SetCurrentTileLocation(tileSteppedOn);
            movement.RefreshTilesInRange(map);
            attack.RefreshTilesInRange(map, movement.GetCurrentTileLocation());
        }
    }

}
