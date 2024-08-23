using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARCamera))] 
public class ARCameraEditor : Editor 
{
	private static TextAsset[] OpticalParamsAssets;
	private static int OpticalParamsAssetCount;
	private static string[] OpticalParamsFilenames;

	public static void RefreshOpticalParamsFilenames() 
	{
		OpticalParamsAssets = Resources.LoadAll("ardata/optical", typeof(TextAsset)).Cast<TextAsset>().ToArray();
		OpticalParamsAssetCount = OpticalParamsAssets.Length;
		OpticalParamsFilenames = new string[OpticalParamsAssetCount];
		for (int i = 0; i < OpticalParamsAssetCount; i++) {					
			OpticalParamsFilenames[i] = OpticalParamsAssets[i].name;				
		}
	}

    public override void OnInspectorGUI()
    {
		ARCamera arc = (ARCamera)target;
		if (arc == null) return;
		
		EditorGUILayout.Separator();
		arc.Stereo = EditorGUILayout.Toggle("Part of a stereo pair", arc.Stereo);
		if (arc.Stereo) {
			arc.StereoEye = (ARCamera.ViewEye)EditorGUILayout.EnumPopup("Stereo eye:", arc.StereoEye);
		}

		EditorGUILayout.Separator();

		arc.Optical = EditorGUILayout.Toggle("Optical see-through mode.", arc.Optical);

		if (arc.Optical) {
			RefreshOpticalParamsFilenames();            
			if (OpticalParamsFilenames.Length > 0) {
				int opticalParamsFilenameIndex = EditorGUILayout.Popup("Optical parameters file", arc.OpticalParamsFilenameIndex, OpticalParamsFilenames);
				string opticalParamsFilename = OpticalParamsAssets[opticalParamsFilenameIndex].name;
				if (opticalParamsFilename != arc.OpticalParamsFilename) {
					arc.OpticalParamsFilenameIndex = opticalParamsFilenameIndex;
					arc.OpticalParamsFilename = opticalParamsFilename;
					arc.OpticalParamsFileContents = OpticalParamsAssets[arc.OpticalParamsFilenameIndex].bytes;
				}
				arc.OpticalEyeLateralOffsetRight = EditorGUILayout.FloatField("Lateral offset right:", arc.OpticalEyeLateralOffsetRight);
				EditorGUILayout.HelpBox("Enter an amount by which this eye should be moved to the right, relative to the video camera lens. E.g. if this is the right eye, but you're using calibrated optical paramters for the left eye, enter 0.065 (65mm).", MessageType.Info);
			} else {
				arc.OpticalParamsFilenameIndex = 0;
				EditorGUILayout.LabelField("Optical parameters file", "No parameters files available");
				arc.OpticalParamsFilename = "";
				arc.OpticalParamsFileContents = new byte[0];
			}
		}
    }
}
