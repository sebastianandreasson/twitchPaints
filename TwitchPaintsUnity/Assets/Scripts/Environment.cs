using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {

	public GameObject paintModeLight;
	public GameObject displayModeLight;

	public Transform cameraPositionPaintMode;
	public Transform cameraPositionStudioMode;

	void Start() {
		SetToDisplayMode();
	}

	public void SetToPaintMode() {
		paintModeLight.SetActive(true);
		displayModeLight.SetActive(false);

		//iTween.MoveTo(Camera.main.gameObject, cameraPositionPaintMode, 1f);
	}

	public void SetToDisplayMode() {
		paintModeLight.SetActive(false);
		displayModeLight.SetActive(true);

		//iTween.MoveTo(Camera.main.gameObject, cameraPositionStudioMode, 1f);
	}
}
