using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Path_Controller : MonoBehaviour
{
    [SerializeField] public GameObject Canvas;
    [SerializeField] public GameObject Floor_0_Paths;
    [SerializeField] public GameObject Floor_1_Paths;
    [SerializeField] public GameObject Floor_2_Paths;
    [SerializeField] public TextMeshProUGUI next_prev_button_text;

    public List<GameObject> All_Floor1_Stairs_Paths = new List<GameObject>(); //Every stairs path must be also deactivated after or before loading another.

    public void Awake()
    {
        //Operation("158"); //Will activate path when entering play mode. Does not active anything at UI.
    }

    private int NextStep; //This variable is to store the floor number so UI pathing goes to correct next floor. So if room is 0, it simply stores it.
    private bool IsAtNextStep;

    public void ResetNavigationVariables()
    {
        NextStep = 0;
        IsAtNextStep = false;
    }

    public void GoToNextStep() //Floor0 = 4, Floor1 = 5, Floor2 = 6
    {
        if (IsAtNextStep) { Canvas.GetComponent<Canvas_Controller>().PlayFade(5); next_prev_button_text.text = "Next step"; }
        else { Canvas.GetComponent<Canvas_Controller>().PlayFade(NextStep + 4); next_prev_button_text.text = "Previous step"; }
        IsAtNextStep = !IsAtNextStep;
    }

    public void Operation(string room) //Will Activate path. Always founded room expected.
    {
        int floor = room[0] - '0';
        Debug.Log("This room " + room  + " is located on floor " + floor + ".");
        Deactivate_StairsPaths();
        if (floor == 0)
        {
            Debug.Log("Activating path on floor 0");
            ActivatePath_Floor0(room);
            NextStep = floor;
        }
        else if (floor == 1)
        {
            Debug.Log("Activating path on floor 1");
            ActivatePath_Floor1(room);
        }
        else if(floor == 2)
        {
            Debug.Log("Activating path on floor 2");
            ActivatePath_Floor2(room);
            NextStep = floor;
        }
    }

    private void Deactivate_StairsPaths() //Should be called when exiting pathing.
    { 
        foreach (GameObject path in All_Floor1_Stairs_Paths) { path.SetActive(false); } 
    }

    public void DeactivateAll() //Simply deactivates all paths.
    {
        Debug.Log("Deactivating all using Path_Controller");

        Deactivate_StairsPaths();

        for (int i = 0; i < Floor_1_Paths.transform.childCount; i++)
        {
            Floor_1_Paths.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private GameObject selectedPath; //When room found is from a another floor (0 or 1), this script must activate correct path to correct stairs. That info is contained in component that is found from that path gameobject. Look Activation function to floor 2 for better detail.
    private void ActivatePath_Floor1(string room)
    {
        bool Success = false;

        for (int i = 0; i < Floor_1_Paths.transform.childCount; i++)
        {
            GameObject path = Floor_1_Paths.transform.GetChild(i).gameObject;

            if (path.name.Contains(room))
            {
                path.SetActive(true);
                Success = true;
            }
            else
            {
                path.SetActive(false);
            }
        }

        if(!Success) { Debug.Log("Activating path was: Fail" ); }
        else { Debug.Log("Activating path was: Success"); }
    }

    private void ActivatePath_Floor2(string room)
    {
        bool Success = false;
        for (int i = 0; i < Floor_2_Paths.transform.childCount; i++)
        {
            GameObject path = Floor_2_Paths.transform.GetChild(i).gameObject;

            if (path.name.Contains(room))
            {
                path.SetActive(true);
                Success = true;
                selectedPath = path;
            }
            else
            {
                path.SetActive(false);
            }
        }

        if (!Success) { Debug.Log("Activating path was: Fail"); }
        else 
        {
            Debug.Log("Activating path was: Success");
            selectedPath.GetComponent<Path_Hold_Info>().GetStairs().SetActive(true);
        }
    }

    private void ActivatePath_Floor0(string room)
    {
        bool Success = false;
        for (int i = 0; i < Floor_0_Paths.transform.childCount; i++)
        {
            GameObject path = Floor_0_Paths.transform.GetChild(i).gameObject;

            if (path.name.Contains(room))
            {
                path.SetActive(true);
                Success = true;
                selectedPath = path;
            }
            else
            {
                path.SetActive(false);
            }
        }

        if (!Success) { Debug.Log("Activating path was: Fail"); }
        else
        {
            Debug.Log("Activating path was: Success");
            selectedPath.GetComponent<Path_Hold_Info>().GetStairs().SetActive(true);
        }
    }
}
