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
