using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    private Attack attackToTrigger;
    public Attack AttackToTrigger { get { return attackToTrigger; } set { attackToTrigger = value; } }

    BoxCollider boxCollider;

    private void Awake()
    {
        Debug.Log("Target Marker");
        boxCollider = GetComponent<BoxCollider>();
    }

    public void EnableCollision(bool enable)
    {
        Debug.Log("Collision Off");
        if (boxCollider)
        {
            boxCollider.enabled = enable;
            
        }
    }

    public void TriggerAttack(Unit unitToAttack)
    {
        if (unitToAttack)
        {
            attackToTrigger.TriggerAttackAtMarker(gameObject);
        }
    }
}
