using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Reference atlas sprite:
//http://forum.unity3d.com/threads/mini-tutorial-on-changing-sprite-on-runtime.212619/
public class PlayerSpawner : MonoBehaviour, MessageReceiver {

	public GameObject playerPrefab;
	public Transform playerSpawnPoint;
	public Transform playerParent;
	public Texture2D spriteAtlasTexture;
	public Texture2D spriteFrameTexture;

	private Sprite[] spriteAtlas;
	private int thumbnailSize;
	private int playerCount = 0;
	private int atlasSize;
	private Color[] framePixels;

	void Start() {
		//Load sprite array (doesn't seem to be a way to slice sprites at runtime):
		//this.spriteAtlas = Resources.LoadAll<Sprite> ("PlayerAtlas4096");

		this.spriteAtlas = Resources.LoadAll<Sprite> (spriteAtlasTexture.name);
		this.atlasSize = spriteAtlasTexture.width;
		this.framePixels = spriteFrameTexture.GetPixels();
		this.thumbnailSize = spriteFrameTexture.width;
		Debug.Log("FIrst pixel alpha: " + framePixels[0].a);
		ClearTexture();
		InvokeRepeating("FlushTexture", 5f, 1f);
	}

	private void HandleNewPlayersMessage(SimpleJSON.JSONNode node) {

		if (playerCount >= 500) {
			return;
		}

		SimpleJSON.JSONNode message = node["args"][0];
		
//		List<string> playerNames = new List<string> ();
//		List<string> playerImageURLs = new List<string> ();

		foreach (SimpleJSON.JSONNode sub in message["players"].AsArray) {
//			playerNames.Add (sub["username"]);
//			playerImageURLs.Add (sub["imageURL"]);

			string username = sub["username"];
			string imageURL = sub["imageURL"];
			string id = sub["id"];
			float speed = sub["speed"].AsFloat;
			speed = Random.Range(1.8f, 2.2f); //Until S has implemented the JSON property
			SpawnPlayer(id, username, speed, imageURL);
		}
	}
	
	public void Receive (SimpleJSON.JSONNode message) {
		//Iterate through members and send sub-message
		//Need script for that?
		//Förslag på princip: JSON noder ska bara skickas en nod djupt, dvs. får ej skickas vidare från denna klass. Tolka istället här och skicka specifik instruktion.


		string msgType = message["name"]; 
		if (msgType == ServerConnection.MSGTYPE_NEWPLAYERS) {
			HandleNewPlayersMessage(message);
		}

	}

	private void SpawnPlayer(string id, string username, float speed, string imageURL) {
		StartCoroutine(SpawnPlayerRoutine(id, username, speed, imageURL));
	}

	private IEnumerator SpawnPlayerRoutine(string id, string username, float speed, string imageURL) {

		WWW www = new WWW(imageURL);
	
		yield return www;

		GameObject playerObject = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity) as GameObject;
		playerObject.name = id;
		playerObject.transform.parent = playerParent;
		Player player = playerObject.GetComponent<Player> ();
		player.Init(id, username);

		//Report to coordinator:
		//LevelObjectCoordinator.ReportNewObject(player, id);

		Texture2D tex = www.texture;
		Color[] scaledPixels = ResizePixelsPointMethod(tex.GetPixels(), tex.width, tex.height, thumbnailSize, thumbnailSize);

		//Apply frame:
		scaledPixels = GetFramedPixels(scaledPixels);


//		tex.Resize(thumbnailSize, thumbnailSize); //Only resizes the container... need to do scaling myself
//		
//		//http://wiki.unity3d.com/index.php/TextureScale
//
//		tex.SetPixels(scaledPixels);
//		tex.Apply();
//		Rect r = new Rect(Vector2.zero, new Vector2(tex.width, tex.height));
//		Sprite sprite = Sprite.Create(tex, r, new Vector2(.5f, 0f), thumbnailSize); 
//		playerObject.GetComponentsInChildren<SpriteRenderer> ()[0].sprite = sprite;

		//Set pixels in atlas:

		int n = atlasSize / thumbnailSize;
		int texX = (this.playerCount % n) * thumbnailSize;
		int texY = atlasSize - (this.playerCount / n) * thumbnailSize - thumbnailSize;
		//Debug.Log("tex coords: " + texX + ", " + texY);
		spriteAtlasTexture.SetPixels(texX, texY, thumbnailSize, thumbnailSize, scaledPixels);
		//spriteAtlasTexture.Apply(false);
		//Debug.Log("Applied, player count: " + this.playerCount);
		//spriteAtlasTexture.SetPixels( scaledPixels

		//Assign sprite to this object:
		playerObject.GetComponentsInChildren<SpriteRenderer> ()[0].sprite = spriteAtlas[this.playerCount];
		playerObject.GetComponentsInChildren<SpriteRenderer> ()[0].sortingOrder = playerCount;

		//ToDo: handle swapping player slots...
		this.playerCount ++;
	}

	private void FlushTexture() {
		spriteAtlasTexture.Apply(false);
	}

	private void ClearTexture() {
		Color32[] allWhite = new Color32[atlasSize * atlasSize];
		for (int i = 0; i < atlasSize * atlasSize; i ++) {
			allWhite[i] = Color.white;
		}
		spriteAtlasTexture.SetPixels32(allWhite);
		spriteAtlasTexture.Apply();
	}

	private Color[] GetFramedPixels(Color[] basePixels) {
		if (basePixels.Length != this.framePixels.Length) {
			Debug.LogWarning("Frame and picture size incompatible");
			return null;
		} else {
			Color[] endPixels = new Color[this.framePixels.Length];
			for (int i = 0; i < endPixels.Length; i ++) {
				endPixels[i] = basePixels[i] * this.framePixels[i];
				//endPixels[i] = BlackWhite(endPixels[i]);
				endPixels[i].a = this.framePixels[i].a;
			}
			return endPixels;
		}

	}

	private Color BlackWhite(Color c) {
		float gray = .33333f * (c.r + c.g + c.b);
		return new Color(gray, gray, gray, c.a);
	}

	private Color[] ResizePixelsPointMethod(/*int[] pixels*/Color[] pixels,int w1,int h1,int w2,int h2) {
		//int[] temp = new int[w2*h2] ;
		Color[] temp = new Color[w2*h2] ;
		// EDIT: added +1 to account for an early rounding problem
		int x_ratio = (int)((w1<<16)/w2) +1;
		int y_ratio = (int)((h1<<16)/h2) +1;
		//int x_ratio = (int)((w1<<16)/w2) ;
		//int y_ratio = (int)((h1<<16)/h2) ;
		int x2, y2 ;
		for (int i=0;i<h2;i++) {
			for (int j=0;j<w2;j++) {
				x2 = ((j*x_ratio)>>16) ;
				y2 = ((i*y_ratio)>>16) ;
				temp[(i*w2)+j] = pixels[(y2*w1)+x2] ;
			}                
		}                
		return temp ;
	}
}
