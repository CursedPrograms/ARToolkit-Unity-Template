using System;
using UnityEngine;

public class ARPattern
{
    public Texture2D texture = null;
    public Matrix4x4 matrix;
    public float width;
	public float height;
	public int imageSizeX;
	public int imageSizeY;

    public ARPattern(int markerID, int patternID)
    {
		float[] matrixRawArray = new float[16];
		float widthRaw = 0.0f;
		float heightRaw = 0.0f;

		if (!PluginFunctions.arwGetMarkerPatternConfig(markerID, patternID, matrixRawArray, out widthRaw, out heightRaw, out imageSizeX, out imageSizeY))
		{
			throw new ArgumentException("Invalid argument", "markerID,patternID");
		}
		width = widthRaw*0.001f;
		height = heightRaw*0.001f;

		matrixRawArray[12] *= 0.001f;            
		matrixRawArray[13] *= 0.001f;
		matrixRawArray[14] *= 0.001f;

		Matrix4x4 matrixRaw = ARUtilityFunctions.MatrixFromFloatArray(matrixRawArray);
		matrix = ARUtilityFunctions.LHMatrixFromRHMatrix(matrixRaw);

		if (imageSizeX > 0 && imageSizeY > 0) {
			texture = new Texture2D(imageSizeX, imageSizeY, TextureFormat.RGBA32, false);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.anisoLevel = 0;
			
			Color[] colors = new Color[imageSizeX * imageSizeY];
			if (PluginFunctions.arwGetMarkerPatternImage(markerID, patternID, colors)) {
				texture.SetPixels(colors);
				texture.Apply();
			}
		}

    }
}
