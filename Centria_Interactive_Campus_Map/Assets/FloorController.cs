using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloorController : MonoBehaviour // This component is expected to be put to Object taht holds all floor and floors have rooms.
{
    [SerializeField] public GameObject Cam; // Not the main camera, but it's parent object attach here.

    [SerializeField] public TextMeshProUGUI FloorText; //Just the text that display user, what floor is currently showing.

    private Vector3 Cam_Center = new Vector3(0,0,0); //This is a reset point what happens when floor is changed.

    private int floorCount; //To keep current floor what user sees.

    public int Get_FloorCount() { return floorCount; }
    void Awake()
    {
        floorCount = 1; //Means that user starts from floor 1. This can set to any floor if wanted.
        FloorText.text = floorCount.ToString() + "F";
        DisableAllOtherFloors();
        transform.GetChild(floorCount).gameObject.SetActive(true); //This activates that set floor.
    }


    //These are functions that happens when floor changes. Only difference is that other descreases and other increases floorCount.
    public void FloorUp()
    {
        DisableAllOtherFloors();
        if (floorCount < 2)
        {
            Cam.transform.position = Cam_Center;
            transform.GetChild(floorCount).gameObject.SetActive(false);
            floorCount++;
            transform.GetChild(floorCount).gameObject.SetActive(true);
            FloorText.text = floorCount.ToString() + "F";
        }
    }

    public void FloorDown()
    {
        DisableAllOtherFloors();
        if (floorCount > 0)
        {
            Cam.transform.position = Cam_Center;
            transform.GetChild(floorCount).gameObject.SetActive(false);
            floorCount--;
            transform.GetChild(floorCount).gameObject.SetActive(true);
            FloorText.text = floorCount.ToString() + "F";
        }
    }

    public void SetFloorToBe(int floor) //This is only meant for functions, that need to path user from floor to floor like 0 to 2 or 2 to 0. I (Juhani) think Centria is not adding anymore floors to the building, but it's good to be always prepared.
    {
        Debug.Log("Setting floor to be " + floor);
        Cam.transform.position = Cam_Center;
        DisableAllOtherFloors();
        transform.GetChild(floor).gameObject.SetActive(true);
        floorCount = floor;
        FloorText.text = floorCount.ToString() + "F";
    }

    void DisableAllOtherFloors() //Disables all other floors if let open by accident.
    {
        for (int i = 0; i < transform.childCount; i++) { transform.GetChild(i).gameObject.SetActive(false); }
    }
}
