using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class ScrollableList : MonoBehaviour
{
	public GameObject itemPrefab;
	public int itemCount = 0, columnCount = 1;

	public void createScrollableBillList() {
		RectTransform rowRectTransform = itemPrefab.GetComponent<RectTransform>();
		RectTransform containerRectTransform = gameObject.GetComponent<RectTransform>();
		int day = PlayerPrefs.GetInt("current level");
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		int[] dueDates = PlayerPrefsX.GetIntArray("due");
		string[] effectsArray = PlayerPrefsX.GetStringArray("effects");

		//calculate the width and height of each child item.
		float width = containerRectTransform.rect.width / columnCount;
		float ratio = width / rowRectTransform.rect.width;
		float height = rowRectTransform.rect.height * ratio;
		List<string> effects = new List<string>();

		//adjust the height of the container so that it will just barely fit all its children
		itemCount = 0;
		for (int i = 0; i < bills["bills"].Count -1; i++) {
			if (dueDates[i] <= day) {
				itemCount++;
			}
		}

		float scrollHeight = height * itemCount;
		Debug.Log("Scroll Height is " + scrollHeight);
		containerRectTransform.offsetMin = new Vector2(containerRectTransform.offsetMin.x, -scrollHeight / 2);
		containerRectTransform.offsetMax = new Vector2(containerRectTransform.offsetMax.x, scrollHeight / 2);

		for (int i = 0; i < bills["bills"].Count - 1; i++) {
			bool instantiateBill = false;
			bool overdue = false;
			if (dueDates[i] == day) {
				instantiateBill = true;
				Debug.Log(bills["bills"][i]["title"] + " is due, cost is " + bills["bills"][i]["amount"]);
			}
			else if (dueDates[i] <= day) {
				instantiateBill = true;
				overdue = true;
//				Debug.Log(bills["bills"][i]["title"] + " for " + bills["bills"][i]["amount"] + " is overdue by " + (day - dueDates[i]) + " days");
				//bill is over due
				effects.Add(bills["bills"][i]["effect"]);
			}

			if (instantiateBill) {
				GameObject newItem = Instantiate(itemPrefab) as GameObject;
				
				newItem.GetComponentInChildren<BillScript>().amount = bills["bills"][i]["amount"].AsInt;
				newItem.GetComponentInChildren<BillScript>().id = i;
				newItem.GetComponentInChildren<BillScript>().title = bills["bills"][i]["title"];
				newItem.GetComponentInChildren<BillScript>().effect = bills["bills"][i]["effect"];
				newItem.GetComponentInChildren<BillScript>().overdue = overdue;
				newItem.name = gameObject.name + " item at (" + i;
				newItem.transform.parent = gameObject.transform;
				
				//move and size the new item
				RectTransform rectTransform = newItem.GetComponent<RectTransform>();				
				float x = -containerRectTransform.rect.width / 2 + width * (i % columnCount);
				float y = containerRectTransform.rect.height / 2 - height * i;
				rectTransform.offsetMin = new Vector2(x, y);				
				x = rectTransform.offsetMin.x + width;
				y = rectTransform.offsetMin.y + height;
				rectTransform.offsetMax = new Vector2(x, y);

			}
		}

	}



	void Start() {

    }

}
