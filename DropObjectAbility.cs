using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/DropObjectAbility")]
public class DropObjectAbility : Ability
{
    public GameObject objectToDrop;

    public override void Activate(GameObject parent)
    {
        Instantiate(objectToDrop, parent.transform.position, Quaternion.identity);
    }
}
