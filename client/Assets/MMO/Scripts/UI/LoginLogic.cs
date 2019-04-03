using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ghbc;

public class LoginLogic : MonoBehaviour
{
	[SerializeField]
	private GameObject addressObj;
	[SerializeField]
	private GameObject connectBtn;

	[SerializeField]
	private GameObject ingameObj;
	// Use this for initialization
	void Start ()
	{
		InputField inputfield = addressObj.GetComponent<InputField> ();
//		Text inputAdress = addressObj.transform.Find ("Text").GetComponent<Text> ();
		var record = PlayerPrefs.GetString ("address");
		if (!string.IsNullOrEmpty (record)) {
			print ("address record: " + record);
			inputfield.text = record;
		}
		var btn = connectBtn.GetComponent<Button> ();
		btn.onClick.AddListener (() => {
			if (!string.IsNullOrEmpty (inputfield.text)) {
				PlayerPrefs.SetString ("address", inputfield.text);
				this.gameObject.SetActive (false);
				Netmanager.conf = NetworkConfig.GetInstance ();
				Netmanager.conf.ip = inputfield.text;
				var tt = this.transform.parent.Find ("addresshold").GetComponent<Text> ();
				tt.text = Netmanager.conf.ip;
				Netmanager.ToConnect ();
				//==
				ingameObj.SetActive (true);
			}
		});
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
