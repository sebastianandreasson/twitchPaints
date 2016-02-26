using UnityEngine;
using System.Collections;

public class Painting : MonoBehaviour {

	public bool demoMode;

	private Texture2D texture;
	private int width, height;
	private bool allowPainting = true;


	public int Width {
		get {
			return this.width;
		}
	}
	
	public int Height {
		get {
			return this.height;
		}
	}


	void Start() {
		if (demoMode) {
			StartCoroutine(DemoRoutine());
		}
	}

	private IEnumerator DemoRoutine() {

		Init(100, 100);

		while (demoMode) {

			int brushSize = Random.Range(1, 7);
			float alpha = Random.Range(0f, 1f);
			float r = Random.Range(0f, 1f);
			float g = Random.Range(0f, 1f);
			float b = Random.Range(0f, 1f);
			int x = Random.Range(0, width);
			int y = Random.Range(0, height);

			Paint(x, y, brushSize, new Color(r, g, b), alpha);
			yield return new WaitForSeconds(.1f);
		}
	}


	
	public void Init(int width, int height) {

		this.width = width;
		this.height = height;


		this.texture = new Texture2D(width, height);
		this.texture.filterMode = FilterMode.Point;
		this.texture.wrapMode = TextureWrapMode.Clamp;
		GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = this.texture;

		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				this.texture.SetPixel(x, y, Color.white);
			}
		}
		this.texture.Apply();

	}

	public void FreezePainting() {
		allowPainting = false;
	}

	#region Painting
	public void Paint(int x, int y, int brushSize, Color c, float alpha) {

		if (!allowPainting) {
			Debug.Log("Painting stopped, not allowing paint command");
			return;
		}

		//Assume top - down perspective:
		y = height - y - 1;

		for (int xx = x - brushSize / 2; xx <= x + brushSize / 2; xx ++) {
			for (int yy = y - brushSize / 2; yy <= y + brushSize / 2; yy ++) {

				if (xx < 0 || xx >= width || yy < 0 || yy >= height) {
					continue;
				}

				//Read existing value:
				Color currentColor = this.texture.GetPixel(xx, yy);

				//Mix:
				Color newColor = Color.Lerp(currentColor, c, alpha);

				this.texture.SetPixel(xx, yy, newColor);
				//Debug.Log("painting " + c + " at " + x + ", " + y + ", size: " + brushSize + ", alpha: " + alpha + ", currentColor: " + currentColor);
			}
		}
		this.texture.Apply();

	}
	#endregion

	public void SaveCurrent() {
		//System.IO.File.WriteAllBytes(Application.dataPath + "/painting.png", this.texture.EncodeToPNG());
	}

	#region Transformations
	//Paint space has 0,0 in top left, and width,height in bottom right
	public Vector3 PaintingToWorldSpace(int x, int y) {
		float xw = (width / 2 - x) * .01f;
		float yw = (y - height / 2) * .01f;
		return new Vector3(xw, yw, 0f);
	}
	#endregion
}
