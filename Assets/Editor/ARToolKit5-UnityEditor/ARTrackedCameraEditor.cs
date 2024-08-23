using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARTrackedCamera))] 
public class ARTrackedCameraEditor : ARCameraEditor 
{
    public override void OnInspectorGUI()
    {
		ARTrackedCamera artc = (ARTrackedCamera)target;
		if (artc == null) return;
		artc.MarkerTag = EditorGUILayout.TextField("Marker tag", artc.MarkerTag);

		ARMarker marker = artc.GetMarker();
		EditorGUILayout.LabelField("Got marker", marker == null ? "no" : "yes");
		if (marker != null) {
			string type = ARMarker.MarkerTypeNames[marker.MarkerType];
			EditorGUILayout.LabelField("Marker UID", (marker.UID != ARMarker.NO_ID ? marker.UID.ToString() : "Not loaded") + " (" + type + ")");	
		}
		
		EditorGUILayout.Separator();
		artc.eventReceiver = (GameObject)EditorGUILayout.ObjectField("Event Receiver:", artc.eventReceiver, typeof(GameObject), true);
		
		EditorGUILayout.Separator();
		artc.secondsToRemainVisible = EditorGUILayout.FloatField("Stay visible", artc.secondsToRemainVisible);
		
		base.OnInspectorGUI();
	}
}
