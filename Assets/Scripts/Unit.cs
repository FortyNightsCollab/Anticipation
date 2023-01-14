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

        if(map)
        {
            map.RegisterUnit(this);
        }
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
            movement.RefreshTilesInRange(map, tileLocation);
            movement.ShowTilesInRange(true);
            movement.ShowPath(true);
            selectedForMovement = true;
            
        }

        else
        {
            movement.ShowTilesInRange(false);
            movement.ShowPath(false);
            attack.ShowTilesInRange(false);
            attack.ShowTargets(false);
            selectable.Select(SelectState.OFF);        
        }
    }
   

    public void NextAction()
    {
        if(selectedForMovement)
        {
            movement.ShowTilesInRange(false);
            attack.ShowTilesInRange(true);
            attack.ShowTargets(true);
            selectedForMovement = false;
        }

        else
        {
            attack.ShowTilesInRange(false);
            attack.ShowTargets(false);
            movement.ShowTilesInRange(true);
            selectedForMovement = true;
        }
    }

    public void NewTurn()
    {
        attack.ArmTargets(false);
    }

    public bool ExecuteTurn(int phase)
    {
        switch(phase)
        {
            case 0:
                if(!movement.QueuedForMovement && attack.QueuedForAttack)
                {
                    attack.ArmTargets(true);
                }

                else if(movement.QueuedForMovement)
                {
                    movement.Move();
                }
                return false;

            case 1:
                return movement.Enroute;

            case 2:
                if (movement.QueuedForMovement && attack.QueuedForAttack)
                {
                    attack.ArmTargets(true);
                }
                return false;

        }
        return false;
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
                    bool returnTrue = movement.SetDestination(tile, map);
                    attack.RefreshTilesInRange(map, movement.TileDestination);
                    return returnTrue;
                }

                else
                {
                    movement.HighlightPotentialPath(tile, map);
                }
            }

            else
            {
                if (leftMouseClick)
                {
                    attack.SetAttack();
                }

                else
                {
                    attack.HighlightPotentialTargets(tile, map);
                }
            }
            return true;

        }

        if (leftMouseClick) return false;
        else return true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Tile tileSteppedOn = collider.GetComponent<Tile>();
        TargetMarker targetMarker = collider.GetComponent<TargetMarker>();
        

        if (tileSteppedOn)
        {
            tileLocation = tileSteppedOn;
            movement.RefreshTilesInRange(map, tileSteppedOn);
            attack.RefreshTilesInRange(map, tileSteppedOn);
        }

        else if(targetMarker)
        {
            if(movement.QueuedForMovement && ((Mathf.Abs(targetMarker.transform.position.x - transform.position.x) < 0.2f) && ((Mathf.Abs(targetMarker.transform.position.z - transform.position.z) < 0.2f))))
            {
                return;
            }
            movement.StopAtNextPoint();
            targetMarker.TriggerAttack(this);
        }
    }

}
