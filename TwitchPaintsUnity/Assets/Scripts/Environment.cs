using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {

	public Light paintModeLight;
	public Light displayModeLight;

	public Transform cameraPositionPaintMode;
	public Transform cameraPositionDisplayMode;

	private float paintLightIntensity;
	private float displayModeIntensity;

	public bool demoMode;

	void Start() {

		//Store values:
		this.paintLightIntensity = paintModeLight.intensity;
		this.displayModeIntensity = displayModeLight.intensity;

		SetToDisplayMode();

		if (demoMode) {
			StartCoroutine(DemoRoutine());
		}
	}

	public void SetToPaintMode() {

		Hashtable htA = iTween.Hash("from",displayModeIntensity,"to",0f,"time",3f,"onupdate","SetDisplayLightIntensity");
		iTween.ValueTo(gameObject, htA);
		Hashtable htB = iTween.Hash("from",0f,"to",paintLightIntensity,"time",3f,"onupdate","SetPaintLightIntensity");
		iTween.ValueTo(gameObject, htB);

		iTween.MoveTo(Camera.main.gameObject, cameraPositionPaintMode.position, 3f);
	}

	public void SetToDisplayMode() {
		Hashtable htA = iTween.Hash("from",0f,"to",displayModeIntensity,"time",3f,"onupdate","SetDisplayLightIntensity");
		iTween.ValueTo(gameObject, htA);
		Hashtable htB = iTween.Hash("from",paintLightIntensity,"to",0f,"time",3f,"onupdate","SetPaintLightIntensity");
		iTween.ValueTo(gameObject, htB);

		iTween.MoveTo(Camera.main.gameObject, cameraPositionDisplayMode.position, 3f);
	}

	#region iTween callbacks
	private void SetDisplayLightIntensity(float value) {
		this.displayModeLight.intensity = value;
	}

	private void SetPaintLightIntensity(float value) {
		this.paintModeLight.intensity = value;
	}
	#endregion

	private bool tmpBool = false;
	IEnumerator DemoRoutine() {
		while (demoMode) {
			if (tmpBool) {
				SetToDisplayMode();
			} else {
				SetToPaintMode();
			}
			tmpBool = !tmpBool;
			yield return new WaitForSeconds(6f);
		}
	}
}
