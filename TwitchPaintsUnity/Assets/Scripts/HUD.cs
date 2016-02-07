using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour, MessageReceiver  {

	public RectTransform listParent;
	public GameObject listItemPrefab;

	public void Receive (SimpleJSON.JSONNode message) {
		string msgType = message["name"]; 

		if (msgType == ServerConnection.MSGTYPE_HUDLIST) {
			HandleHudListMessage(message);
		}
	}

	private void HandleHudListMessage(SimpleJSON.JSONNode node) {
		Debug.Log("Received paint message: " + node);

		//Clear list:
		var children = new List<GameObject>();
		foreach (Transform child in listParent) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));
		//end cl

		
		SimpleJSON.JSONNode message = node["args"][0];

		SimpleJSON.JSONArray list = message.AsArray;

		int i = 1;
		foreach(SimpleJSON.JSONNode s in list) {
			Debug.Log("List element: " + s);

			GameObject go = Instantiate(listItemPrefab);
			go.GetComponent<UnityEngine.UI.Text> ().text = i + ". " + s["name"] + ", " + s["votes"];
			go.transform.parent = listParent;

			i ++;
		}
	}
}
