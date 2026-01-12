using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class RoomData
{
    public string room_name;
    public GameObject room_obj;
    public int status;

    public RoomData(string room_name, GameObject room_obj, int status)
    {
        this.room_name = room_name;
        this.room_obj = room_obj;
        this.status = status;
    }
}
public class Room_Controller : MonoBehaviour
{
    [SerializeField] public GameObject Cam; // Not the main camera, but it's parent object attach here.

    [SerializeField] public GameObject Canvas; // Needs connection to canvas components

    private List<RoomData> rooms = new List<RoomData>(); //Stores rooms from floors. Loads all from awake.

    [SerializeField] public List<string> Debug_Test_Rooms_To_Be_InUse = new List<string>(); //This is a debug option that is meant for testing In_use status putting to rooms. Only adds status 1 when launching, not actively while app is running.
    public enum RoomStatus //These are status codes for rooms. If some status is missing add it here.
    {
        Free = 0,
        In_use = 1
    }

    void Debug_Room_status_Set()
    {
        if (Debug_Test_Rooms_To_Be_InUse.Count > 0)
        {
            Debug.Log("Started debug option: Selected room status now turn into 1 (In_use)");
            foreach (RoomData room in rooms)
            {
                if (Debug_Test_Rooms_To_Be_InUse.Contains(room.room_name)) //Means if list contains the room, it will turn it into 1 that is "In_use".
                {
                    room.status = 1; // in use
                    room.room_obj.GetComponent<Renderer>().material = HuoneFound_Mat;
                    Debug.Log("Found room " + room.room_name + ". Turning status into 1.");
                }
                else //This makes room "free" instead if not found from the debug list.
                {
                    room.status = 0; // free
                    room.room_obj.GetComponent<Renderer>().material = Huone_Mat;
                }
            }
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) { Debug_Room_status_Set(); } //Only to test room status functionality. Is not meant for real use, but use basis to implement a real solution.
    }

    private bool ShowRooms_Choice; //Just to keep the option for showing rooms.
    public void ShowRooms()
    {
        ShowRooms_Choice = !ShowRooms_Choice; //This flips the boolean so it becomes a switch.

        foreach(RoomData room in rooms) //Only looks rooms, that were added in awake function.
        {
            if (room.room_obj.transform.childCount > 0) { room.room_obj.transform.GetChild(0).gameObject.SetActive(ShowRooms_Choice); }
            else { Debug.Log("ERROR: This room " + room + " does not have a text as child"); }
        }
    }


    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for(int j = 0; j < transform.GetChild(i).transform.childCount; j++)
            {
                if(!transform.GetChild(i).GetChild(j).gameObject.name.Contains("*")) 
                {
                    GameObject room = transform.GetChild(i).GetChild(j).gameObject;
                    rooms.Add(new RoomData(room.name, room, 0));
                    if(room.transform.childCount > 0)
                    {
                        transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<TextMeshPro>().text = room.name; //Rooms have text on top of them. If text missing or it is wrong, this fixes it.
                        transform.GetChild(i).GetChild(j).GetChild(0).gameObject.SetActive(false); //There is a option to turn these on/off
                    }
                }
            }
        }
        //Debug.Log("Room count is " + rooms.Count);

        //foreach (RoomData room in rooms) { room.room_obj.GetComponent<Renderer>().material = HuoneFound_Mat; } //Only confirms that all rooms are found without errors. This is probably commented out, but use if any problems!
    }

    IEnumerator APISlowMethod() //Old func. Used and tested, but really bad.
    {
        int id_alku = 100000; int id_loppu = 200000;
        for (int i = id_alku; i < id_loppu; i++) 
        { 
            string url = "https://lukkarit.centria.fi/rest/event/" + i; 
            using (UnityWebRequest req = UnityWebRequest.Get(url)) 
            { 
                req.SetRequestHeader("User-Agent", "Mozilla/5.0"); yield return 
                req.SendWebRequest(); 
                Debug.Log("Status " + i + ": " + req.responseCode); 
                if (req.result != UnityWebRequest.Result.Success)
                { 
                    //if(req.error != null) { Debug.Log("Virhe haettaessa ID " + i + ": " + req.error); }
                    continue;
                } 
                string vs = req.downloadHandler.text; Debug.Log("Vastaus (" + i + "): " + vs);
                if (vs.Length > 0) { Debug.Log("ID " + i + " | Status: " + req.responseCode + " | Pituus: " + vs.Length); }
            }
        }
    }

    IEnumerator API() //Little improved, but still really bad and slow.
    {
        int idStart = 119694;
        int idEnd = 200000;
        int batchSize = 20;

        for (int batch = idStart; batch < idEnd; batch += batchSize)
        {
            List<UnityWebRequestAsyncOperation> ops = new List<UnityWebRequestAsyncOperation>();

            for (int i = batch; i < batch + batchSize && i < idEnd; i++)
            {
                string url = "https://lukkarit.centria.fi/rest/event/" + i;
                UnityWebRequest req = UnityWebRequest.Get(url);
                req.SetRequestHeader("User-Agent", "Mozilla/5.0");

                ops.Add(req.SendWebRequest());
            }

            foreach (var op in ops) { yield return op; } //Must wait.

            foreach (var op in ops)
            {
                UnityWebRequest req = op.webRequest;
                string vs = req.downloadHandler.text;

                if (!string.IsNullOrEmpty(vs) && vs != "null")
                {
                    Debug.Log("DATA: " + vs);
                }
            }
        }
    }

    public void Cam_to_TargetRoom(string name)
    {
        bool roomIsFound = false; int id = 0;

        foreach (RoomData room in rooms)
        {
            if(room.room_name == name)
            {
                Debug.Log("Room " + name + " has been found!");
                int floor = room.room_name[0] - '0';
                //Cam_To_TargetPos(id); //Puts cam to target room's text. 12.1. Not every room has this so?
                if(GetComponent<FloorController>().Get_FloorCount() != floor) { Canvas.GetComponent<Canvas_Controller>().PlayFade(4 + floor); } // Mode 4 means floor 0, usage example: 4 (Which means 0) + floor (in this example is 2) equals next floor to be => 2.
                Debug.Log("This room is located on floor " + floor + ".");
                roomIsFound = true;
                break;
            }
            id++;
        }

        if (!roomIsFound) { Debug.Log("Room was not found"); }
    }
    [SerializeField] public Material Huone_Mat;
    [SerializeField] public Material HuoneFound_Mat;

    public void Cam_To_TargetPos(int id)
    {
        int floor = rooms[id].room_name[0] - '0';
        if(GetComponent<FloorController>().Get_FloorCount() == floor)
        {
            Vector3 target = rooms[id].room_obj.transform.GetChild(0).position; //Means the target is set to text object
            target.y = 0;
            Cam.transform.position = target;
        }
    }

    public void ProcessToRoom()
    {
        string room = Canvas.GetComponent<Search_Function>().GetTarget();

        bool success = false;
        foreach (RoomData roomData in rooms)
        {
            if (roomData.room_name == room) { success = true; break; }
        }

        if (success) //Put all actions here!
        {
            Debug.Log("Room " + room + " exist.");
            //Cam_to_TargetRoom(room); //This should target room location including it's floor.
        }
        else
        {
            Debug.Log("ERROR: This room " + room + " does not exist!");
        }
    }
}
