using UnityEngine;
using System.Collections;

public class PaintSessionHandler : MonoBehaviour, MessageReceiver {

	public GameObject paintingPrefab;
	public Environment environment;
	public HUD hud;

	private Painting currentPainting;

	#region Interface
	public void Receive (SimpleJSON.JSONNode message) {
				
		string msgType = message["name"]; 
		if (msgType == ServerConnection.MSGTYPE_STARTPAINTING) {
			HandleStartPaintingSessionMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_ENDPAINTING) {
			HandleEndPaintingSessionMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_STARTTHEME) {
			HandleStartThemeSessionMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_ENDTHEME) {
			HandleEndThemeSessionMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_STARTNAMING) {
			HandleStartNamingSessionMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_ENDNAMING) {
			HandleEndNamingSessionMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_PAINT) {
			HandlePaintMessage(message);
		} else if (msgType == ServerConnection.MSGTYPE_SAVEPAINTING) {
			HandleSavePaintingMessage();
		}
		
	}
	#endregion

	public void HandleSavePaintingMessage() {
		this.currentPainting.SaveCurrent();
		StartCoroutine(UploadFile());
	}

	#region Paint session
	public void HandlePaintMessage(SimpleJSON.JSONNode node) {

		Debug.Log("Received paint message: " + node);
		
		SimpleJSON.JSONNode message = node["args"][0];

		int x = message["x"].AsInt;
		//int y = this.currentPainting.Height - message["y"].AsInt - 1;
		int y = message["y"].AsInt;
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


	public void HandleStartPaintingSessionMessage (SimpleJSON.JSONNode node) {
		Debug.Log("Received paint session message: " + node);

		SimpleJSON.JSONNode message = node["args"][0];

		GameObject go = GameObject.Instantiate(paintingPrefab);

		int width = message["width"].AsInt;
		int height = message["height"].AsInt;

		this.currentPainting = go.GetComponent<Painting> ();
		this.currentPainting.Init(width + 1, height + 1);

		environment.SetToPaintMode();

		//ToDo: Set subtitle
	}

	public void HandleEndPaintingSessionMessage(SimpleJSON.JSONNode node) {
		Debug.Log("Received paint session message: " + node);
		
		SimpleJSON.JSONNode message = node["args"][0];
		
		//1: Freeze painting:
		if (this.currentPainting != null) {
			this.currentPainting.FreezePainting();
		}

		//2: Send picture:
		if (this.currentPainting != null) {
			this.currentPainting.SaveCurrent();
			StartCoroutine(UploadFile());
		}

		//3. Change environment
		this.environment.SetToDisplayMode();

		//Stop showing subtitle:
		this.hud.Clear(true);

	}
	#endregion

	#region Naming session
	public void HandleStartNamingSessionMessage(SimpleJSON.JSONNode node) {
		Debug.Log("Start naming session. TODO");

		this.hud.SetHeader("What should we call this Masterpiece?");
	}

	public void HandleEndNamingSessionMessage(SimpleJSON.JSONNode node) {
		Debug.Log("End naming session. TODO");

		//Destroy existing painting:
		if (this.currentPainting != null) {
			Destroy(this.currentPainting.gameObject);
		}

		//Clear HUD:
		this.hud.Clear (true);


	}
	#endregion

	#region Theme session
	public void HandleStartThemeSessionMessage(SimpleJSON.JSONNode node) {
		Debug.Log("Start theme session. TODO");
		this.hud.SetHeader("What is the theme for the next painting?");
	}
	
	public void HandleEndThemeSessionMessage(SimpleJSON.JSONNode node) {
		Debug.Log("End theme session. TODO");

		//Clear HUD:
		this.hud.Clear (true);

		string chosenTheme = node["args"][0]["themeName"];

		this.hud.SetSubtitle("Theme: " + chosenTheme);
	}
	#endregion

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
