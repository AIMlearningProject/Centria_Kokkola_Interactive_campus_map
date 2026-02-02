using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System;

[System.Serializable]
public class RoomData
{
    public string room_name;
    public GameObject room_obj;
    public int status;

    public List<Reservation> reservations = new();

    public RoomData(string roomName, GameObject roomObj, int status)
    {
        this.room_name = roomName;
        this.room_obj = roomObj;
        this.status = status;
    }
    public static List<Reservation> GetReservationsForRoom(List<RoomData> rooms, string roomName)
    {
        foreach (var room in rooms)
        {
            if (room.room_name == roomName)
                return room.reservations;
        }

        return null; // room not found
    }
    public bool TryAddReservation(Reservation newRes)
    {
        foreach (var res in reservations)
        {
            if (newRes.startTime < res.endTime && newRes.endTime > res.startTime)
            {
                return false;
            }
        }

        reservations.Add(newRes);
        return true;
    }

    public string GetReservationName(int orderNum) //Use this as reverse to get reservation information. Simply remove the label and replace that with some other variable like start or end time.
    {
        if (orderNum < 0 || orderNum >= reservations.Count) { return null; }

        return reservations[orderNum].label;
    }

    public bool UpdateStatusNow() //Simply returns if room is being used.
    {
        status = IsOccupiedNow() ? 1 : 0;
        return status == 1;
    }
    private bool IsOccupiedNow() //Used at above.
    {
        int now = TimeUtils.GetFinnishMinutesSinceMidnight();

        foreach (var res in reservations)
        {
            if (now >= res.startTime && now < res.endTime)
                return true;
        }

        return false;
    }

    public static void LoadMats() //This is loading the mats for rooms to use. Loading is called from Room_Controller since this class is not meant to use mono behavior.
    {
        Huone_Mat = Resources.Load<Material>("Materials/HUONE"); Debug.Log("Loaded mat: " + Huone_Mat.name + ".");
        HuoneFound_Mat = Resources.Load<Material>("Materials/HUONE_RESERVED"); Debug.Log("Loaded mat: " + HuoneFound_Mat.name + ".");
        Huone_1_Mat = Resources.Load<Material>("Materials/HUONE 1"); Debug.Log("Loaded mat: " + Huone_1_Mat.name + ".");
    }

    private static Material Huone_Mat;
    private static Material HuoneFound_Mat;
    private static Material Huone_1_Mat;

    public void SetReserved()
    {
        status = 1;
        Renderer rend = room_obj.GetComponent<Renderer>();
        Material[] mats = rend.materials;

        if (mats[0].name.Contains("HUONE 1"))
        {
            mats[0] = HuoneFound_Mat;
            mats[1] = Huone_Mat;
        }
        else
        {
            mats[1] = HuoneFound_Mat;
            mats[0] = Huone_Mat;
        }

        rend.materials = mats;

        //Debug.Log("Found room " + room_name + ". Turning status into 1.");
    }

    public void SetFree()
    {
        status = 0;
        Renderer rend = room_obj.GetComponent<Renderer>();
        Material[] mats = rend.materials;

        if (mats[0].name.Contains("HUONE 1"))
        {
            mats[0] = Huone_1_Mat;
            mats[1] = Huone_Mat;
        }
        else
        {
            mats[0] = Huone_Mat;
            mats[1] = Huone_1_Mat;
        }

        rend.materials = mats;
    }

    public static void TestReservationsForAllRooms(List<RoomData> rooms) //Simply test for applying reservation mat to all rooms.
    {
        foreach (var room in rooms)
        {
            //if (room.IsOccupiedNow()) { if (room.status != 1) { room.SetReserved(); } }
            ///else { if (room.status != 0) { room.SetFree(); } }

            room.SetReserved();
        }
    }
}

[System.Serializable]
public class Reservation
{
    public string label;
    public float startTime;
    public float endTime;

    public Reservation(string label, float startTime, float endTime)
    {
        this.label = label;
        this.startTime = startTime;
        this.endTime = endTime;
    }
    public bool IsNow()
    {
        DateTime now = TimeUtils.GetFinnishTime();
        int nowMinutes = now.Hour * 60 + now.Minute;

        if (endTime <= startTime)
            return false;

        return nowMinutes >= startTime && nowMinutes < endTime;
    }
    public bool IsPassed() //Instead of deleting, it checks if reservation is already passed.
    {
        if (endTime <= startTime)
            return true;

        DateTime now = TimeUtils.GetFinnishTime();
        int nowMinutes = now.Hour * 60 + now.Minute;

        return endTime <= nowMinutes;
    }
}
public class Room_Controller : MonoBehaviour
{
    public bool AddReservationToRoom(string roomName, string label, float startTime, float endTime)
    {
        if (!roomLookupByName.TryGetValue(roomName, out RoomData room)) { Debug.LogWarning($"Room not found: {roomName}"); return false; }

        Reservation res = new Reservation(label, startTime, endTime);
        return room.TryAddReservation(res);
    }

    public enum FinnishTimeSlots // These are times that are used to create start/end times for reservations. These are only hours as minutes like 6 hours is 360 minutes.
    {
        H06_00 = 360,  // 6*60
        H07_00 = 420,  // 7*60
        H08_00 = 480,  // 8*60
        H09_00 = 540,  // 9*60
        H10_00 = 600,  // 10*60
        H11_00 = 660,  // 11*60
        H12_00 = 720,  // 12*60
        H13_00 = 780,  // 13*60
        H14_00 = 840,  // 14*60
        H15_00 = 900,  // 15*60
        H16_00 = 960,  // 16*60
        H17_00 = 1020, // 17*60
        H18_00 = 1080, // 18*60
        H19_00 = 1140, // 19*60
        H20_00 = 1200, // 20*60
        H21_00 = 1260  // 21*60
    }


    [SerializeField] public GameObject Cam; // Not the main camera, but it's parent object attach here.

    [SerializeField] public GameObject Canvas; // Needs connection to canvas components

    private List<RoomData> rooms = new List<RoomData>(); //Stores rooms from floors. Loads all from awake. 
    public List<RoomData> GetRooms() { return rooms; }

    public List<Reservation> GetReservationsForRoomByName(string _room)
    {
        List<Reservation> reservations =  RoomData.GetReservationsForRoom(rooms, _room);

        if (reservations != null) { return reservations; }

        return null;
    }

    private Dictionary<string, RoomData> roomLookupByName = new Dictionary<string, RoomData>();

    public void RoomsUpdate()
    {
        foreach (var room in rooms)
        {
            bool occupied = room.UpdateStatusNow(); // updates status and returns true if occupied

            if (occupied)
            {
                room.SetReserved();
                //Debug.Log($"Room '{room.room_name}' is currently occupied."); //Use to test, if room is not getting right materials applied or is not getting reserved status;
            }
            else
            {
                room.SetFree();
                //Debug.Log($"Room '{room.room_name}' is free."); //Use to test, if room is not getting right materials applied or is not getting reserved status;
            }

            foreach (var res in room.reservations)
            {
                Debug.Log($"Reservation: {res.label}, {res.startTime/60}–{res.endTime/60}");
            }
        }
    }

    public TextMeshProUGUI ShowTime;
    private DateTime currentTime;

    void Update()
    {
        currentTime = TimeUtils.GetFinnishTime();
        ShowTime.text = $"{currentTime:HH:mm}";
        if (Input.GetKeyDown(KeyCode.Tab)) { RoomsUpdate(); }
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
        RoomData.LoadMats();


        for (int i = 0; i < transform.childCount; i++)
        {
            for(int j = 0; j < transform.GetChild(i).transform.childCount; j++)
            {
                if(!transform.GetChild(i).GetChild(j).gameObject.name.Contains("*")) 
                {
                    GameObject room = transform.GetChild(i).GetChild(j).gameObject;
                    RoomData roomData = new RoomData(room.name, room, 0);
                    rooms.Add(roomData);
                    roomLookupByName.Add(room.name, roomData);
                    if (room.transform.childCount > 0)
                    {
                        transform.GetChild(i).GetChild(j).GetChild(0).GetComponent<TextMeshPro>().text = room.name; //Rooms have text on top of them. If text missing or it is wrong, this fixes it.
                        transform.GetChild(i).GetChild(j).GetChild(0).gameObject.SetActive(false); //There is a option to turn these on/off
                    }
                }
            }
        }

        //These are test reservations. Should be deleted, if using proper ones. Only use as reference to create adding function;
        AddReservationToRoom("131", "Varaustesti", (float)FinnishTimeSlots.H11_00 + 47, (float)FinnishTimeSlots.H13_00+30);
        AddReservationToRoom("131", "Varaustesti1", (float)FinnishTimeSlots.H13_00 + 51f, (float)FinnishTimeSlots.H15_00 + 30f);
        AddReservationToRoom("131", "Varaustesti2", (float)FinnishTimeSlots.H15_00 + 45f, (float)FinnishTimeSlots.H17_00 + 15f);

        Debug.Log(((int)FinnishTimeSlots.H14_00 + 30f) / 60);

        //RoomData.TestReservationsForAllRooms(rooms);
        //Debug.Log("Room count is " + rooms.Count);
    }

    [SerializeField] public List<string[]> Debug_reservations = new List<string[]>();

    IEnumerator APISlowMethod() //Old func. Used and tested, but really bad.
    {
        int id_start = 100000; int id_end = 200000;
        for (int i = id_start; i < id_end; i++) 
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
                string vs = req.downloadHandler.text; Debug.Log("Answer (" + i + "): " + vs);
                if (vs.Length > 0) { Debug.Log("ID " + i + " | Status: " + req.responseCode + " | Length: " + vs.Length); }
            }
        }
    }

    IEnumerator API() //Little improved, but still bad and slow to use in real time.
    {
        int id_start = 119694;
        int id_end = 200000;
        int batchSize = 10; //Select here how many request is processed at once. 10 is good, 20 is a max, beyond is no go.

        for (int batch = id_start; batch < id_end; batch += batchSize)
        {
            List<UnityWebRequestAsyncOperation> ops = new List<UnityWebRequestAsyncOperation>();

            for (int i = batch; i < batch + batchSize && i < id_end; i++)
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
                Debug.Log("This room is located on floor " + floor + ".");
                //Cam_To_TargetPos(id); //Puts cam to target room's text. 12.1. Not every room has this so?
                if (GetComponent<FloorController>().Get_FloorCount() != floor) { Canvas.GetComponent<Canvas_Controller>().PlayFade(4 + floor); } // Mode 4 means floor 0, usage example: 4 (Which means 0) + floor (in this example is 2) equals next floor to be => 2.
                roomIsFound = true;
                break;
            }
            id++;
        }

        if (!roomIsFound) { Debug.Log("Room was not found"); }
    }
    [SerializeField] public Material Huone_Mat; public Material Get_Huone_Mat() { return Huone_Mat; }
    [SerializeField] public Material HuoneFound_Mat; public Material Get_HuoneFound_Mat() { return HuoneFound_Mat; }
    [SerializeField] public Material Huone_1_Mat; public Material Get_Huone_1_Mat() { return Huone_1_Mat; }

    public void Cam_To_TargetPos(int id) //Unused since new feature was added. Only used as reference to create better functions.
    {
        int floor = rooms[id].room_name[0] - '0';
        if(GetComponent<FloorController>().Get_FloorCount() == floor)
        {
            Vector3 target = rooms[id].room_obj.transform.GetChild(0).position; //Means the target is set to text object
            target.y = 0;
            Cam.transform.position = target;
        }
    }

    public void ProcessToRoom() //Unused since new feature was added. Only used as reference to create better functions.
    {
        string room = Canvas.GetComponent<Search_Function>().GetTarget();

        if(room != "")
        {

            bool success = false;
            foreach (RoomData roomData in rooms)
            {
                if (roomData.room_name == room) { success = true; break; }
            }

            if (success) //Put all actions here!
            {
                Debug.Log("Room " + room + " exist.");
                Canvas.GetComponent<Canvas_Controller>().PlayFade(8);
                //Cam_to_TargetRoom(room); //OLD METHOD: This targets room location including it's floor. 
            }
            else
            {
                Canvas.GetComponent<Canvas_Controller>().ActivateErrorScreen(1);
                Debug.Log("ERROR: This room " + room + " does not exist!");
            }
        }
        else
        {
            Canvas.GetComponent<Canvas_Controller>().ActivateErrorScreen(2);
        }
    }
}
public static class TimeUtils //Helper
{
    public static DateTime GetFinnishTime()
    {
        try
        {
            // Windows
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "FLE Standard Time"
            );
        }
        catch
        {
            // Linux
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "Europe/Helsinki"
            );
        }
    }

    public static int GetFinnishMinutesSinceMidnight()
    {
        DateTime finTime = GetFinnishTime();
        return finTime.Hour * 60 + finTime.Minute;
    }

    public static bool IsReservationNow(Reservation res) //Not used. Only reference.
    {
        DateTime now = GetFinnishTime();

        int nowMinutes = now.Hour * 60 + now.Minute;

        if (res.startTime <= res.endTime)
        {
            return nowMinutes >= res.startTime && nowMinutes < res.endTime;
        }

        return nowMinutes >= res.startTime || nowMinutes < res.endTime;
    }
}
