using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoToWork : MonoBehaviour
{
    public GameObject GoToWorkButton;
    public GameObject LookForWorkButton;

    public Text Headline;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("fired", 0) ==1)
        {
            GoToWorkButton.SetActive(false);
            LookForWorkButton.SetActive(true);
            //can't work
            Headline.text = "Look for a new job?";
        }
        else
        {
            GoToWorkButton.SetActive(true);
            LookForWorkButton.SetActive(false);
            Headline.text = "Go to work?";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
