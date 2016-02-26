using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour, MessageReceiver  {

	public RectTransform listParent;
	public UnityEngine.UI.Text headerText;
	public UnityEngine.UI.Text subText;
	public GameObject listItemPrefab;

	private List<GameObject> currentItems = new List<GameObject> ();

	private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public void Receive (SimpleJSON.JSONNode message) {
		string msgType = message["name"]; 

		if (msgType == ServerConnection.MSGTYPE_HUDLIST) {
			HandleHudListMessage(message);
		}
	}

	private void HandleHudListMessage(SimpleJSON.JSONNode node) {

		//Clear list:
		Clear(false);
		
		SimpleJSON.JSONNode message = node["args"][0];

		SimpleJSON.JSONArray list = message.AsArray;

		int i = 1;
		foreach(SimpleJSON.JSONNode s in list) {

			GameObject go = Instantiate(listItemPrefab);
			this.currentItems.Add (go);
			//char letter = alphabet.Substring(i);

			go.GetComponent<UnityEngine.UI.Text> ().text = i + ". " + s["name"] + ", " + s["votes"];
			//go.transform.parent = listParent;
			go.transform.SetParent(listParent, true);

			i ++;
		}
	}

	public void SetHeader(string headerText) {
		this.headerText.text = headerText;
	}

	public void SetSubtitle(string subText) {
		this.subText.text = subText;
	}

	public void Clear(bool alsoClearHeader) {
		Debug.Log("Clearing UI list");
		if (alsoClearHeader) {
			SetHeader("");
			SetSubtitle("");
		}
		foreach (GameObject item in this.currentItems) {
			Destroy(item);
		}
		this.currentItems.Clear();
	}
}
