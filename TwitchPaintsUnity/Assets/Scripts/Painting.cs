using UnityEngine;
using System.Collections;

public class Painting : MonoBehaviour {

	private Texture2D texture;

	private int width, height;

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

	public void Paint(int x, int y, int brushSize, Color c, float alpha) {

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
				Debug.Log("painting " + c + " at " + x + ", " + y + ", size: " + brushSize + ", alpha: " + alpha + ", currentColor: " + currentColor);
			}
		}
		this.texture.Apply();

	}

	public void SaveCurrent() {
		System.IO.File.WriteAllBytes(Application.dataPath + "/painting.png", this.texture.EncodeToPNG());
	}
}
