using UnityEngine;
using System.Collections;

public class Tweet {

	private string twitterHandle;
	private string tweetText;
	private string twitterUserImageUrl;

	public Tweet (string twitterHandle, string tweetText, string twitterUserImageUrl) {
		this.twitterHandle = twitterHandle;
		this.tweetText = tweetText;
		this.twitterUserImageUrl = twitterUserImageUrl;
	}

	public string TwitterHandle {
		get {
			return this.twitterHandle;
		}
	}

	public string TweetText {
		get {
			return this.tweetText;
		}
	}

	public string TwitterUserImageUrl {
		get {
			return this.twitterUserImageUrl;
		}
	}
	
}
