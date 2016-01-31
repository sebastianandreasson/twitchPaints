using UnityEngine;
using System.Collections.Generic;


public class TwitterHandler : MonoBehaviour, MessageReceiver {


	public GameObject tweetPrefab;
	public Transform listitemParent;
	public int simultaneousTweets;

	public float itemHeight;

	private Queue<Tweet> pendingTweets = new Queue<Tweet> ();
	private LinkedList<GameObject> displayedTweets = new LinkedList<GameObject> ();

	void Start() {
		InvokeRepeating("UpdateList", 8f, 1f);
	}

	public void Receive (SimpleJSON.JSONNode node) {

		//Debug.Log("Twitter handler received: " + node["args"][0]["message"]);
//		Debug.Log("List item parent:"  + listitemParent);
		//ClearBoard();
		
		SimpleJSON.JSONNode message = node["args"][0];
		string twitterHandle = message["username"];
		string tweet = message["message"];
		pendingTweets.Enqueue(new Tweet(twitterHandle, tweet, ""));



	}

	private void UpdateList() {

		//Remove top and add bottom, then move everything upwards
		if (pendingTweets.Count > 0) {

			//Move all up:
			Vector3 move = new Vector3(0f, itemHeight, 0f);
			foreach (GameObject currentTweet in displayedTweets) {
				//iTween.MoveBy(currentTweet, move, .5f);
			}

			//Remove the oldest:
			if (displayedTweets.Count > simultaneousTweets) {
				GameObject oldest = displayedTweets.First.Value;
				Destroy(oldest, .6f);
				displayedTweets.RemoveFirst();
			}

			//Add new:
			GameObject go = Instantiate(tweetPrefab) as GameObject;
			go.transform.parent = listitemParent;
			go.transform.localPosition = Vector3.zero;
			displayedTweets.AddLast(go);

			Tweet t = pendingTweets.Dequeue();
			//go.GetComponent<GuiElement_Tweet> ().Set(t.TwitterHandle, t.TweetText);

		}
	}
	

}

