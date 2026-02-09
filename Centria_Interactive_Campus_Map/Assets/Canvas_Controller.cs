using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class Canvas_Controller : MonoBehaviour
{
    [SerializeField] public GameObject Maps;

    [SerializeField] public Image fadeImage;
    [SerializeField] public float fadeDuration; //Set duration for fade here. //By default it is set to 1.

    private int Fade_mode; //This settings chooses what happens during fade like: Floor change.

    [SerializeField] public GameObject MapNav_Interface;

    [SerializeField] public GameObject MapPath_Interface;

    [SerializeField] public GameObject Room_Information;

    [SerializeField] public GameObject Room_Error_Screen; //Error screen is inside Search screen as child.
    private TextMeshProUGUI Error_MSG;

    public bool OnMap()
    {
        bool success = false;
        //success = MapNav_Interface.activeSelf; if(success) { return success; }
        success = MapPath_Interface.activeSelf; if (success) { return success; }
        success = Room_Information.activeSelf; if (success) { return success; }
        success = GetComponent<Search_Function>().GetSearchActive(); if (success) { return success; }
        return success;
    }

    private void Start()
    {
        MapNav_Interface.SetActive(true);
        MapPath_Interface.SetActive(false);
        Room_Information.SetActive(false);

        Error_MSG = Room_Error_Screen.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void StartSearching()
    {
        GetComponent<Search_Function>().StartAndCloseSearch();
        MapNav_Interface.SetActive(!MapNav_Interface.activeSelf);
    }

    private List<String> searchErrors = new List<string>() //This list contains error messages that are applied to text inside error screen.
    { 
        "0",                                        //Should not be get. [0] only used while exiting error screen.                             
        "Error: Searched room does not exist.",
        "Error: Search field was empty." 
    };

    public void ActivateErrorScreen(int error) //Opens/closes error screen.
    {
        Error_MSG.text = searchErrors[error];
        Room_Error_Screen.SetActive(!Room_Error_Screen.activeSelf);
    }

    private string room_name; //Because of fade function was not intended to get anything values than mode itself, Room should be store as variable inside canvas_controller so it could forwarded.
    public void SetRoomNameAtCanvas(string roomName)
    {
        room_name = roomName; Debug.Log("New room is set as room_name: " + room_name);
    }

    public void Show_RoomInformation() //Open the reservation info screen for specific room. Uses room_name to get right room.
    {
        bool Go = !Room_Information.activeSelf;

        Room_Information.SetActive(Go);
        if(Go)
        {
            DeactivateAllReservationSlots();
            Debug.Log("Loading room information for: " + room_name);

            Room_Information.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Room " + room_name;

            List<Reservation> list = Maps.GetComponent<Room_Controller>().GetReservationsForRoomByName(room_name);

            if (list != null)
            {
                Debug.Log("Currently this room has: " + Maps.GetComponent<Room_Controller>().GetReservationsForRoomByName(room_name).Count + " reservations.");

                Transform reservationSlots = Room_Information.transform.GetChild(2); int i = 0; int MaxSlots = reservationSlots.childCount; Debug.Log("Max slots for reservations is " + MaxSlots);

                foreach (Reservation reservation in list)
                {
                    if(!reservation.IsPassed())
                    {
                        reservationSlots.GetChild(i).gameObject.SetActive(true);
                        reservationSlots.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = reservation.label;
                        reservationSlots.GetChild(i).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = ProcessThisTime(reservation.startTime / 60) + " - " + ProcessThisTime(reservation.endTime / 60);

                        if (reservation.IsNow()) { reservationSlots.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.blue; }
                        else { reservationSlots.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.red; }

                        i++;
                        if(i > MaxSlots) { break; }
                    }
                }

            }
            else { Debug.Log("This room does not have reservations."); }
        }
        else { Debug.Log("Exiting room information panel."); }
    }

    string ProcessThisTime(float time)
    {
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.RoundToInt((time - hours) * 60f);

        if (minutes == 60)
        {
            minutes = 0;
            hours++;
        }

        hours = (hours + 24) % 24;

        return $"{hours:D2}:{minutes:D2}";
    }

    void DeactivateAllReservationSlots()
    {
        Transform slots = Room_Information.transform.GetChild(2);
        Debug.Log("Deactivating all reservation slots. " + slots.name);
        for (int i = 0; i < slots.childCount; i++) 
        { 
            slots.GetChild(i).gameObject.SetActive(false);
            slots.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.red;
        }
    }

    public void ShowPath()
    {
        GetComponent<Search_Function>().CloseSearch();
        MapPath_Interface.SetActive(true); //Open two buttons that are used to procee or going back from pathing section.
        string room = GetComponent<Search_Function>().GetTarget();
        Debug.Log("Proceeding to pathing using room " + room + "."); //There was weird error so it stops here if any happens in the future.
        Maps.GetComponent<FloorController>().SetFloorToBe(1); //Always starts from floor 1. Dont comment out or there will be errors in steps.
        Maps.GetComponent<Path_Controller>().Operation(room);
    }

    public void DisableShowPathEverything()
    {
        MapNav_Interface.SetActive(true);
        GetComponent<Search_Function>().CloseSearch();
        MapPath_Interface.SetActive(false);
        Maps.GetComponent<FloorController>().SetFloorToBe(1);
        Maps.GetComponent<Path_Controller>().ResetEverything();
    }
    public enum Fade_Modes
    {
        None = 0,
        FloorUp = 1,
        FloorDown = 2,
        SearchAct = 3,
        Floor0 = 4,
        Floor1 = 5,
        Floor2 = 6,
        SearchActBack = 7,
        ShowPathNow = 8,
        RoomShow = 9,
        SearchBack = 10
    }

    public void PlayFade(int mode) //This is attached to buttons that starts fade sequence. This is optional since fade sequence can be skipped by using funcs from => Fade_mode_Funcs().
    {
        if(mode == (int)Fade_Modes.FloorUp && Maps.GetComponent<FloorController>().Get_FloorCount() >= 2) { Debug.Log("Cannot go up anymore. Max is 2. Current floor is " + Maps.GetComponent<FloorController>().Get_FloorCount()); }
        else if (mode == (int)Fade_Modes.FloorDown && Maps.GetComponent<FloorController>().Get_FloorCount() <= 0) { Debug.Log("Cannot go lower anymore. Min is 0. Current floor is " + Maps.GetComponent<FloorController>().Get_FloorCount()); }
        else if (mode == (int)Fade_Modes.SearchBack && !GetComponent<Search_Function>().IsSearchFieldEmpty()) 
        { 
            Debug.Log("Deleting one letter."); GetComponent<Search_Function>().PadButton("*"); 
        }
        //else if (mode >= (int)Fade_Modes.Floor0 && mode <= (int)Fade_Modes.Floor2) { } //This should always work.
        else
        {
            Fade_mode = mode;
            StartCoroutine(FadeInOut());
        }
    }

    void Fade_mode_Funcs()
    {
        if (Fade_mode == (int)Fade_Modes.FloorUp) { Maps.GetComponent<FloorController>().FloorUp(); }
        else if (Fade_mode == (int)Fade_Modes.FloorDown) { Maps.GetComponent<FloorController>().FloorDown(); }
        else if (Fade_mode == (int)Fade_Modes.SearchAct) { StartSearching(); }
        else if (Fade_mode >= (int)Fade_Modes.Floor0 && Fade_mode <= (int)Fade_Modes.Floor2) 
        { 
            Maps.GetComponent<FloorController>().SetFloorToBe(Fade_mode - 4);
            //StartSearching(); //Disables Search window. This was meant for old method. Currently not in use.
        }
        else if(Fade_mode == (int)Fade_Modes.SearchActBack)
        {
            //string pathToTake = GetComponent<Search_Function>().GetTarget();
            GetComponent<Search_Function>().StartWithoutReset();
            MapNav_Interface.SetActive(false);
            Maps.GetComponent<Path_Controller>().DeactivateAll();
            Maps.GetComponent<Path_Controller>().ResetNavigationVariables();
        }
        else if (Fade_mode == (int)Fade_Modes.ShowPathNow) { ShowPath(); }
        else if (Fade_mode == (int)Fade_Modes.RoomShow) { Show_RoomInformation(); }
        else if (Fade_mode == (int)Fade_Modes.SearchBack) { DisableShowPathEverything(); }
            Fade_mode = 0; //This value should always reset.
    }

    private IEnumerator FadeInOut() //Simply a fade effect is done here. You can switch color from the image component itself. This only applies for alpha channel.
    {
        Color color = fadeImage.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        Fade_mode_Funcs();

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }
}
