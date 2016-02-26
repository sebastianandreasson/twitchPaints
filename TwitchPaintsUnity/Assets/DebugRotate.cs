using UnityEngine;
using System.Collections;

public class DebugRotate : MonoBehaviour {


	// Update is called once per frame
	void Update () {
		float d = 50f * Time.deltaTime;
		transform.Rotate(d, -2f * d, 1.5f * d);
	}
}
