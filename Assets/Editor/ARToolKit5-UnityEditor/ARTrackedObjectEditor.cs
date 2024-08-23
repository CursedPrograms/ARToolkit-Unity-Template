using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARTrackedObject))] 
public class ARTrackedObjectEditor : Editor 
{
    public override void OnInspectorGUI()
    {
		ARTrackedObject arto = (ARTrackedObject)target;
		if (arto == null) return;

		arto.MarkerTag = EditorGUILayout.TextField("Marker tag", arto.MarkerTag);

		ARMarker marker = arto.GetMarker();
		EditorGUILayout.LabelField("Got marker", marker == null ? "no" : "yes");
		if (marker != null) {
			string type = ARMarker.MarkerTypeNames[marker.MarkerType];
			EditorGUILayout.LabelField("Marker UID", (marker.UID != ARMarker.NO_ID ? marker.UID.ToString() : "Not loaded") + " (" + type + ")");	
		}
		
		EditorGUILayout.Separator();
		
		arto.secondsToRemainVisible = EditorGUILayout.FloatField("Stay visible", arto.secondsToRemainVisible);
		
		EditorGUILayout.Separator();
		
		arto.eventReceiver = (GameObject)EditorGUILayout.ObjectField("Event Receiver:", arto.eventReceiver, typeof(GameObject), true);
	}
}
