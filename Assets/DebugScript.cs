using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugScript : MonoBehaviour {
	public Text healthText;
	public Slider healthSlider;
	public Text cashText;
	public Slider cashSlider;
	public Text pillText;
	public Slider pillSlider;
	public Text wageText;
	public Slider wageSlider;
	public Text firedText;
	public Slider firedSlider;

	// Use this for initialization
	void Start () {
		healthSlider.value = PlayerPrefs.GetFloat("health");
		healthText.text = healthSlider.value.ToString();
		cashSlider.value = PlayerPrefs.GetFloat("money");
		cashText.text = cashSlider.value.ToString();	
		pillSlider.value = PlayerPrefs.GetInt("pills");
		pillText.text = pillSlider.value.ToString();
		wageSlider.value = PlayerPrefs.GetFloat ("hourly wage");
		wageText.text = wageSlider.value.ToString();
		firedSlider.value = PlayerPrefs.GetInt("max warnings");
		firedText.text = firedSlider.value.ToString();

	}

	
	// Update is called once per frame
	void Update () {
	
	}

	public void setHealth(float h) {
		healthText.text = h.ToString();
		PlayerPrefs.SetFloat("health", h);
	}

	public void setCash(float c) {
		cashText.text = "$" + c.ToString();
		PlayerPrefs.SetFloat("money", c);
	}

	public void setPills(float p) {
		pillText.text = p.ToString();
		PlayerPrefs.SetInt("pills", (int)p);
	}

	public void setWages(float w) {
		wageText.text = w.ToString();
	}

	public void setFiredAttempts(float f) {
		firedText.text = f.ToString();
		PlayerPrefs.SetInt("max warnings", (int) f);
	}
}
