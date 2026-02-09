using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomActByTouch : MonoBehaviour
{
    [SerializeField] public GameObject Canvas;
    private Canvas_Controller canvasC;

    private string This_name;

    private void Awake()
    {
        This_name = gameObject.name;
        canvasC = Canvas.GetComponent<Canvas_Controller>();
    }

    void OnMouseOver()
    {
        //Debug.Log("Currently mouse is on " + This_name);

        if (Input.GetMouseButtonDown(0))
        {
            if (!canvasC.OnMap())
            {
                Debug.Log("Activating room: " + This_name);
                canvasC.SetRoomNameAtCanvas(This_name);
                canvasC.PlayFade(9);
            }
            else Debug.Log("Failed to open room info");
        }
    }
}
