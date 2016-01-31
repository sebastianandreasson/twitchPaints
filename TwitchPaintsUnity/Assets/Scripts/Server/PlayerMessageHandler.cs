using UnityEngine;
using System.Collections.Generic;

public class PlayerMessageHandler : MonoBehaviour, MessageReceiver {

	public GameObject messagePrefab;
	public GameObject emoticonPrefab;

	public void Receive (SimpleJSON.JSONNode node) {



		SimpleJSON.JSONNode message = node["args"][0];


		//Expecting an array with one member

		string playerMessage = message["message"];
		string levelObjectId = message["id"];
		SimpleJSON.JSONArray emoticons = message["emotes"].AsArray;

		//Find player object in world:
		GameObject go = GameObject.Find(levelObjectId);
		if (go != null) {

			//Instantiate a message pop up:
			GameObject messageObject = Instantiate(messagePrefab) as GameObject;

			//Transform playerObjectMessageTarget = GameObject.Find("player_" + playerName).transform.FindChild("MessageGoesHere");
			Transform playerObjectMessageTarget = go.transform.FindChild("MessageGoesHere");

			//Debug.Log("object " + ("player_" + playerName) + ": " + playerObject);
			
			messageObject.transform.parent = playerObjectMessageTarget; //ToDo: Some offset here!
			messageObject.transform.localPosition = Vector3.zero;
			
			//Finally, set the message text:
			//messageObject.GetComponent<GuiElement_PlayerMessage> ().Set (playerMessage, 1f);


			//Emojis:
			float delay = 0f;
			foreach (SimpleJSON.JSONNode e in emoticons) {
				
				int id = e["id"].AsInt;
				string url = e["imageURL"];
				GameObject emojiObject = Instantiate(emoticonPrefab) as GameObject;
				//emojiObject.GetComponent<Emoji> ().Set(url, delay, playerObjectMessageTarget);
				delay += .2f;
				emojiObject.transform.position = playerObjectMessageTarget.position;
			}
		}




		
	}

}
