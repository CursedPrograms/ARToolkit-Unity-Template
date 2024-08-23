using System;
using UnityEngine;

[RequireComponent(typeof(Transform))]               
[ExecuteInEditMode]                                 
public class ARTrackedCamera : ARCamera
{
	private const string LogTag = "ARTrackedCamera: ";

	public float secondsToRemainVisible = 0.0f;		            

	[NonSerialized]
	protected int cullingMask = -1;					           

	private bool lastArVisible = false;
	
	[SerializeField]
	private string _markerTag = "";					         
	
	public string MarkerTag
	{
		get
		{
			return _markerTag;
		}
		
		set
		{
			_markerTag = value;
			_marker = null;
		}
	}
	
	public override ARMarker GetMarker()
	{
		if (_marker == null) {
			ARMarker[] ms = FindObjectsOfType<ARMarker>();
			foreach (ARMarker m in ms) {
				if (m.Tag == _markerTag) {
					_marker = m;
					break;
				}
			}
		}
		return _marker;
	}

	public virtual void Start()
	{
		if (cullingMask == -1) {
			cullingMask = this.gameObject.GetComponent<Camera>().cullingMask;
		}
	}

	protected override void ApplyTracking()
	{
		if (arVisible || (timeLastUpdate - timeTrackingLost < secondsToRemainVisible)) {
			if (arVisible != lastArVisible) {
				this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
				if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerFound", GetMarker(), SendMessageOptions.DontRequireReceiver);
			}
			transform.localPosition = arPosition;          
			transform.localRotation = arRotation;
			if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerTracked", GetMarker(), SendMessageOptions.DontRequireReceiver);
		} else {
			if (arVisible != lastArVisible) {
				this.gameObject.GetComponent<Camera>().cullingMask = 0;
				if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerLost", GetMarker(), SendMessageOptions.DontRequireReceiver);
			}
		}
		lastArVisible = arVisible;
	}

}
