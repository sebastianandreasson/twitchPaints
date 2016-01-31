using UnityEngine;
using System.Collections;

public class FakeServer : MonoBehaviour {

	string TEST_msg_playerPool = "{\"name\":\"playerVotingPool\",\"args\":[{\"playerVotingPool\":[{\"username\":\"username0\",\"votes\":200},{\"username\":\"username1\",\"votes\":500},{\"username\":\"username2\",\"votes\":500},{\"username\":\"username3\",\"votes\":500},{\"username\":\"username4\",\"votes\":500},{\"username\":\"username5\",\"votes\":500}]}]}";
	string TEST_msg_playerMessage = "{\"name\":\"playerMessage\",\"args\":[{\"username\":\"username0\",\"message\":\"Hi there!\"}]";

	public event System.EventHandler Opened;
	public event System.EventHandler Message;
	public event System.EventHandler SocketConnectionClosed;
	public event System.EventHandler Error;

	void Start() {

		Opened (this, null);

		InvokeRepeating ("SendPlayerPoolMessage", 1f, 1f);

		InvokeRepeating ("SendPlayerMessage", 3f, 2.7f);

	}

	void OnDisable() {
		SocketConnectionClosed (this, null);
	}

	#region Fake Messages
	private void SendPlayerPoolMessage() {

		SocketIOClient.Messages.JSONMessage m = new SocketIOClient.Messages.JSONMessage (TEST_msg_playerPool);
		
		SocketIOClient.MessageEventArgs e = new SocketIOClient.MessageEventArgs (m);

		Message (this, e);

	}

	private void SendPlayerMessage() {
		
		SocketIOClient.Messages.JSONMessage m = new SocketIOClient.Messages.JSONMessage (TEST_msg_playerMessage);
		
		SocketIOClient.MessageEventArgs e = new SocketIOClient.MessageEventArgs (m);
		
		Message (this, e);
		
	}

	#endregion
}
