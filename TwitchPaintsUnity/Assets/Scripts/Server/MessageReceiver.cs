using UnityEngine;
using System.Collections;

public interface MessageReceiver {
	
	void Receive (SimpleJSON.JSONNode message);
	
}
