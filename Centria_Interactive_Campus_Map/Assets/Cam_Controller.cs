using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam_Controller : MonoBehaviour //This component should be place directly to cam object.
{
    [SerializeField] public float sens;
    [SerializeField] public float MaxX; //Maximum X value where cam is able to move.
    [SerializeField] public float MinX; //Minimum X value where cam is able to move.

    private float h; //Used for getting input from mouse/touch.

    void Update()
    {
        CamControl();
    }

    void CamControl()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            h = (Input.GetAxis("Mouse X") * sens);
            //transform.Rotate(Vector3.up * h * 1); //Use this to rotate Cam if needed.
            if (h > 0 && transform.position.x < MaxX)
            {
                transform.position += new Vector3(h, 0, 0);
                if (transform.position.x > MaxX) { transform.position = new Vector3(MaxX, 0, 0); } //This is set to MaxX position. If this obj uses different Y and Z values: Create variables for them and put those here.
            }

            if (h < 0 && transform.position.x > MinX)
            {
                transform.position += new Vector3(h, 0, 0);
                if (transform.position.x < MinX) { transform.position = new Vector3(MinX, 0, 0); } //This is set to MinX position. If this obj uses different Y and Z values: Create variables for them and put those here.
            }
        }
    }
}
