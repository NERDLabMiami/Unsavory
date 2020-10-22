using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Search : MonoBehaviour
{
    private float timeToStop;
    public float searchTime;
    public GameObject searchResults;
    public GameObject OKButton;

    private Text searching;
    // Start is called before the first frame update

    void Start()
    {
        searching = GetComponent<Text>();
        timeToStop = Time.time + searchTime;
        InvokeRepeating("Look", 0, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time >= timeToStop)
        {
            searchResults.SetActive(true);
            this.gameObject.SetActive(false);
            OKButton.SetActive(true);

        }
    }

    void Look() {
        searching.text = searching.text + ".";
        if (searching.text.Length > 5)
        {
            searching.text = ".";
        }
    }
}
