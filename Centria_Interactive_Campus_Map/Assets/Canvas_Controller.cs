using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Canvas_Controller : MonoBehaviour
{
    [SerializeField] public GameObject Maps;

    [SerializeField] public Image fadeImage;
    [SerializeField] public float fadeDuration; //Set duration for fade here.

    private int Fade_mode; //This settings chooses what happens during fade like: Floor change.

    [SerializeField] public GameObject MapNav_Interface;

    public void StartSearching()
    {
        GetComponent<Search_Function>().StartAndCloseSearch();
        MapNav_Interface.SetActive(!MapNav_Interface.activeSelf);
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
    }

    public void PlayFade(int mode) //This is attached to buttons that starts fade sequence. This is optional since fade sequence can be skipped by using funcs from => Fade_mode_Funcs().
    {
        if(mode == (int)Fade_Modes.FloorUp && Maps.GetComponent<FloorController>().Get_FloorCount() >= 2) { Debug.Log("Cannot go up anymore. Max is 2. Current floor is " + Maps.GetComponent<FloorController>().Get_FloorCount()); }
        else if (mode == (int)Fade_Modes.FloorDown && Maps.GetComponent<FloorController>().Get_FloorCount() <= 0) { Debug.Log("Cannot go lower anymore. Min is 0. Current floor is" + Maps.GetComponent<FloorController>().Get_FloorCount()); }
        else if (mode == (int)Fade_Modes.SearchAct && !GetComponent<Search_Function>().IsSearchFieldEmpty()) { Debug.Log("Deleting one letter."); GetComponent<Search_Function>().PadButton("*"); }
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
            Maps.GetComponent<FloorController>().SetFloorToBe(Fade_mode);
            StartSearching(); //Disables Search window.
        }
        Fade_mode = 0; //This value should always reset.
    }

    private IEnumerator FadeInOut()
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
