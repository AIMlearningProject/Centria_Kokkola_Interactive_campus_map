using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Search_Function : MonoBehaviour
{
    private string SearchTarget; //To store text, that user puts using buttons.

    [SerializeField] public TextMeshProUGUI next_prev_button_text;

    public string GetTarget() 
    {
        Debug.Log("Sending target... (" + SearchTarget + ")");
        next_prev_button_text.text = "Next step";
        return SearchTarget;
    }
    [SerializeField] public TextMeshProUGUI Search_Text; //Display for variable above this.

    [SerializeField] public GameObject Search_Interface;

    public bool GetSearchActive() { return Search_Interface.activeSelf; }

    private void Start()
    {
        SearchTarget = "";
        Search_Text.text = "";
    }
    public void StartAndCloseSearch()
    {
        Search_Interface.SetActive(!Search_Interface.activeSelf);
        SearchTarget = "";
        Search_Text.text = "";
    }

    public void CloseSearch()
    {
        Search_Interface.SetActive(false);
    }

    public void StartWithoutReset()
    {
        Search_Interface.SetActive(true);
    }

    public bool IsSearchFieldEmpty() 
    {
        if(!Search_Interface.activeSelf) { return true; } //Means that it is going to open the search window if not active.
        return SearchTarget.Length == 0;
    }



    //In use at buttons that user inputs character/numbers to search for specific room.
    public void PadButton(string sign) //WARNING: Dont replace string=>char!. First it was char, BUT buttons cannot use it as function, if so. Buttons can use this func only using string!
    {
        if(sign == "*") //Using this sign, deletes last character from the search string. Always use this char, that is not possible to use by user. 
        {
            SearchTarget = SearchTarget.Remove(SearchTarget.Length - 1, 1);
            Debug.Log("Deleted last character from target string");
        }
        else if(SearchTarget.Length < 4) //else then add that char to Search target only if lenght is less that 4 since that is the Max length of rooms example: 033F. 
        {
            SearchTarget += sign;
            Debug.Log("Added character to target: " + sign);
        }

        Search_Text.text = SearchTarget;
    }
}
