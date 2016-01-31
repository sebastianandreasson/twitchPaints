using UnityEngine;
using System.Collections;

public class PaintMessageHandler : MessageReceiver {

	public void Receive (SimpleJSON.JSONNode message) {
		Debug.Log ("Received paint message: " + message);
	}
}
