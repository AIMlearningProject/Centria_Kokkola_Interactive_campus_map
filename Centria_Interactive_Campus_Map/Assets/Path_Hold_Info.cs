using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Hold_Info : MonoBehaviour
{
    [SerializeField] public GameObject SelectedStairs; //Chose here floor 1 path to stairs. 

    public GameObject GetStairs() //Alternatively you can use => { get; set; } after SelectedStairs, but it was not displaying inside Unity editor so that is why I chose this method. (Maybe error? who knows...)
    {
        if(SelectedStairs == null) { Debug.Log(gameObject.name + " Does not have selected stairs!"); return null; }
        return SelectedStairs;
    }
}
