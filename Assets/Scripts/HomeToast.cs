using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomeToast : MonoBehaviour
{
    public GameObject root;
    public Text messageText;
    public float seconds = 2.5f;

    Coroutine _routine;

    public void Show(string msg)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(msg));
    }

    IEnumerator ShowRoutine(string msg)
    {
        if (messageText) messageText.text = msg;
        if (root) root.SetActive(true);
        yield return new WaitForSeconds(seconds);
        if (root) root.SetActive(false);
        _routine = null;
    }
}