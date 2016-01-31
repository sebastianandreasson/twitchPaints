using UnityEngine;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Messages;
using SocketIOClient.Eventing;
using System;
using SimpleJSON;
using System.Text;
using System.Collections;

public class ServerConnection : MonoBehaviour {
	

	public FakeServer fakeServer;
	public bool useFakeServer;
	
	[Header("Message receivers")]
	public GameObject paintSessionHandler; 


	public static string MSGTYPE_NEWPLAYERS = "newPlayers";
	public static string MSGTYPE_NEWPAINTING = "newPainting";
	public static string MSGTYPE_PAINT = "paint";
	public static string MSGTYPE_SAVEPAINTING = "savePainting";


	private Dictionary<string, MessageReceiver> messageReceivers = new Dictionary<string, MessageReceiver> ();

	private Client client;

	private ConcurrentQueue<JSONNode> messageQ = new ConcurrentQueue<JSONNode> ();

	// Use this for initialization
	void Start () {
		Invoke("Init", 2f);
		//StartCoroutine(TestUpload());
	}


	private IEnumerator TestUpload() {
		//TEST:
		WWWForm form = new WWWForm();
		//form.AddField("name", "zip");
		form.AddBinaryData("file", System.IO.File.ReadAllBytes("frame00000.png"), "frame00000.png", "image/png");
		WWW www = new WWW("http://192.168.0.5:3000/upload", form);
		yield return www;
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.LogError(www.error);
		}
		else {
			Debug.Log("Finished Uploading Screenshot");
		}
	}

	void Init() {

		//this.messageReceivers.Add (MSGTYPE_PLAYERMESSAGE, playerMessageHandler.GetComponent<PlayerMessageHandler> ());
		//this.messageReceivers.Add (MSGTYPE_NEWPLAYERS, playerSpawner.GetComponent<PlayerSpawner> ());
		this.messageReceivers.Add (MSGTYPE_NEWPAINTING, paintSessionHandler.GetComponent<PaintSessionHandler> ());
		this.messageReceivers.Add (MSGTYPE_SAVEPAINTING, paintSessionHandler.GetComponent<PaintSessionHandler> ());
		this.messageReceivers.Add (MSGTYPE_PAINT, paintSessionHandler.GetComponent<PaintSessionHandler> ());




		if (useFakeServer) {
			fakeServer.Opened += SocketOpened;
			fakeServer.Message += SocketMessage;
			fakeServer.SocketConnectionClosed += SocketConnectionClosed;
			fakeServer.Error += SocketError;
			
		} else {



			Debug.Log("connect!");
			string url = "http://192.168.0.5:1338/";
			//string url = "http://46.101.225.117:1338"; //Sebastians Droplet server
			
			Debug.Log(url);
			
			client = new Client(url);
			
			
			client.Opened += SocketOpened;
			client.Message += SocketMessage;
			client.SocketConnectionClosed += SocketConnectionClosed;
			client.Error +=SocketError;
			
			client.Connect ();
			
			
			Invoke ("TestPlayers", 3f);

			
			//InvokeRepeating ("TestConnection", 2f, 2f);
			//InvokeRepeating ("SendPlayerStats", 5f, 2f);

			Invoke ("SendLevelObjectsToServer", 3f);




		}
	}

	#region Messages to server

	#endregion
	
	void OnDisable() {
		if (!useFakeServer) {
			client.Opened -= SocketOpened;
			client.Message -= SocketMessage;
			client.SocketConnectionClosed -= SocketConnectionClosed;
			client.Error -= SocketError;
			client.Close ();
		}
	}
	
	void TestConnection() {
		Debug.Log("Connected? " + client.IsConnected);
//		client.Emit("abc", null);
//		client.Connect();
	}
	
	
	#region Server listeners
	private void SocketOpened(object sender, EventArgs e) {
		//invoke when socket opened
		Debug.Log("Opened");
	}
	
	private void SocketMessage (object sender, EventArgs e) {
		if (e!= null) {
			MessageEventArgs mea = (MessageEventArgs) e;
			//Debug.Log(mea);
			string msg = mea.Message.MessageText;
			
			Debug.Log("Message from server: " + msg);
//			Debug.Log(mea);
			//JSON parse:
			//var N = JSON.Parse(msg);
			
			if (msg != null && msg != "") {

				//FOr fake:
				if (useFakeServer) {
					msg = msg.Substring(1, msg.Length - 2);
					msg = System.Text.RegularExpressions.Regex.Unescape(msg);
					Debug.Log("msg after unescape: " + msg);
				}

				try {
					var N = JSON.Parse(msg);


					
					if (N == null) {
						Debug.Log("Node is null");
					}
					//Debug.Log("Node: " + N);
					
					//2015-09-20 This comes in through a background thread, so need to enqueue and then pick up in the main thread:
					messageQ.Enqueue(N);
				} catch(Exception ex) {
					Debug.LogError(ex);
				}


			} else {
				Debug.Log("Not parsing JSON...");
			}
			
			
			
			//process(msg);
		}
	}

	private void ConsumeMessage(JSONNode N) {
		string msgType = N["name"];

		//Debug.Log("Message type: " + msgType + " has receiver? " + messageReceivers.ContainsKey(msgType));
		
		//The "content"-node:
		//var Nsub = N["args"][0];
		
		//Send message through to proper receiver:
		//messageReceivers[msgType].Receive(Nsub);
		//Always send entire message!!

		//FOR DEV: (be Sebastian skicka close ups ibland
//		if (msgType == MSGTYPE_CAMERAVIEW && UnityEngine.Random.Range(0f, 1f) < .5f) {
//			msgType = MSGTYPE_CLOSEUPCAMERAVIEW;
//		}

		messageReceivers[msgType].Receive(N);

	}
	
	private void SocketConnectionClosed(object sender, EventArgs e) {
		Debug.Log("closed");
		if ( e!= null) {
			MessageEventArgs mea = (MessageEventArgs) e;
			string msg = mea.Message.MessageText;
			Debug.Log("Closed, message from server: " + msg);
			//process(msg);
		}
	}
	
	private void SocketError(object sender, EventArgs e) {
		Debug.Log("Socket error");
		if ( e!= null) {
			MessageEventArgs mea = (MessageEventArgs) e;
			string msg = mea.Message.MessageText;
			Debug.Log("Error, message from server: " + msg);
			//process(msg);
		}
	}
	#endregion

	void Update () {
		if (messageQ.Count > 0) {
			SimpleJSON.JSONNode node = messageQ.Dequeue();
			Debug.LogWarning("Consuming server message: " + node["name"]);
			ConsumeMessage(node);
		}
	}
	
}
