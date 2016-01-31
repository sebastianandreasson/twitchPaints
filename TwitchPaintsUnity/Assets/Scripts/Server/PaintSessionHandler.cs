using UnityEngine;
using System.Collections;

public class PaintSessionHandler : MonoBehaviour, MessageReceiver {

	public GameObject paintingPrefab;

	private Painting currentPainting;

	public void Receive (SimpleJSON.JSONNode message) {
				
		string msgType = message["name"]; 
		if (msgType == ServerConnection.MSGTYPE_NEWPAINTING) {
			HandleNewPaintingMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_PAINT) {
			HandlePaintMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_SAVEPAINTING) {
			HandleSavePaintingMessage();
		}
		
	}

	public void HandleSavePaintingMessage() {
		this.currentPainting.SaveCurrent();
		StartCoroutine(UploadFile());
	}

	public void HandlePaintMessage(SimpleJSON.JSONNode node) {

		Debug.Log("Received paint message: " + node);
		
		SimpleJSON.JSONNode message = node["args"][0];

		int x = message["x"].AsInt;
		int y = this.currentPainting.Height - message["y"].AsInt - 1;
		int r = message["rgb"]["r"].AsInt;
		int g = message["rgb"]["g"].AsInt;
		int b = message["rgb"]["b"].AsInt;
		float a = message["rgb"]["a"].AsFloat;
		int size = message["size"].AsInt;

		float rf = (float) r / 255f;
		float gf = (float) g / 255f;
		float bf = (float) b / 255f;
		
		this.currentPainting.Paint(x, y, size, new Color(rf, gf, bf, 1f), a);
	}


	public void HandleNewPaintingMessage (SimpleJSON.JSONNode node) {
		Debug.Log("Received paint session message: " + node);

		SimpleJSON.JSONNode message = node["args"][0];

		GameObject go = GameObject.Instantiate(paintingPrefab);

		int width = message["width"].AsInt;
		int height = message["height"].AsInt;

		this.currentPainting = go.GetComponent<Painting> ();
		this.currentPainting.Init(width + 1, height + 1);
	}

	#region Internal
	private IEnumerator UploadFile() {
		//TEST:
		WWWForm form = new WWWForm();
		//form.AddField("name", "zip");
		form.AddBinaryData("file", System.IO.File.ReadAllBytes(Application.dataPath + "/painting.png"), "painting.png", "image/png");
		WWW www = new WWW("http://192.168.0.5:3000/upload", form);
		yield return www;
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.LogError(www.error);
		}
		else {
			Debug.Log("Finished Uploading Screenshot");
		}
	}
	#endregion

}
