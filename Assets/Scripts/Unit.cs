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
    Selectable selectable;
    Map map;
    bool selectedForMovement;

    // Start is called before the first frame update
    void Start()
    {
        map = FindObjectOfType<Map>();
        attack = GetComponent<Attack>();
        movement = GetComponent<Movement>();
        selectable = GetComponent<Selectable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(bool on)
    {
        if (on)
        {
            movement.RefreshTilesInRange(map);
            movement.Highlight(true);
            selectedForMovement = true;
            movement.EnablePath(true);
        }

        else
        {
            movement.Highlight(false);
            attack.HighlightTilesInRange(false);
            selectable.Select(SelectState.OFF);
            movement.EnablePath(false);
        }
    }
   

    public void NextAction()
    {
        if(selectedForMovement)
        {
            movement.Highlight(false);
            attack.RefreshTilesInRange(map, movement.TileDestination);
            attack.HighlightTilesInRange(true);
            selectedForMovement = false;
        }

        else
        {            
            attack.HighlightTilesInRange(false);
            movement.Highlight(true);
            selectedForMovement = true;
        }
    }

    public bool ProcessAction(GameObject actionObject)
    {
        bool leftMouseClick = Input.GetMouseButtonDown(0);

        Tile tile = actionObject.GetComponent<Tile>();
        if(tile)
        {
            if(selectedForMovement)
            {
                if (leftMouseClick)
                {
                    return movement.SetDestination(tile, map);
                }

                else
                {
                    movement.HighlightPotentialPath(tile, map);
                }
            }

            else
            {
                attack.SetAttack(tile, movement.GetCurrentTileLocation());
            }
            return true;

        }

        if (leftMouseClick) return false;
        else return true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Tile tileSteppedOn = collider.GetComponent<Tile>();
        Debug.Log("Unit Stepped On: " + collider.gameObject);

        if (tileSteppedOn)
        {
            movement.SetCurrentTileLocation(tileSteppedOn);
            attack.RefreshTilesInRange(map, movement.GetCurrentTileLocation());
        }
    }

}
