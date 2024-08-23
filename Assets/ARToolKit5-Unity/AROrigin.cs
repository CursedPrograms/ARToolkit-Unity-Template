using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class AROrigin : MonoBehaviour
{
	private const string LogTag = "AROrigin: ";

	public enum FindMode {
		AutoAll,
		AutoByTags,
		Manual
	}
	public List<String> findMarkerTags = new List<string>();

	private ARMarker baseMarker = null;
	private List<ARMarker> markersEligibleForBaseMarker = new List<ARMarker>();

	[SerializeField]
	private FindMode _findMarkerMode = FindMode.AutoAll;

	public FindMode findMarkerMode
	{
		get
		{
			return _findMarkerMode;
		}
		
		set
		{
			if (_findMarkerMode != value) {
				_findMarkerMode = value;
				FindMarkers();
			}
		}
	}

	public void AddMarker(ARMarker marker, bool atHeadOfList = false)
	{
		if (!atHeadOfList) {
			markersEligibleForBaseMarker.Add(marker);
		} else {
			markersEligibleForBaseMarker.Insert(0, marker);
		}
	}

	public bool RemoveMarker(ARMarker marker)
	{
		if (baseMarker == marker) baseMarker = null;
		return markersEligibleForBaseMarker.Remove(marker);
	}
	
	public void RemoveAllMarkers()
	{
		baseMarker = null;
		markersEligibleForBaseMarker.Clear();
	}

	public void FindMarkers()
	{
		RemoveAllMarkers();
		if (findMarkerMode != FindMode.Manual) {
			ARMarker[] ms = FindObjectsOfType<ARMarker>();      
			foreach (ARMarker m in ms) {
				if (findMarkerMode == FindMode.AutoAll || (findMarkerMode == FindMode.AutoByTags && findMarkerTags.Contains(m.Tag))) {
					markersEligibleForBaseMarker.Add(m);
				}
			}
			ARController.Log(LogTag + "Found " + markersEligibleForBaseMarker.Count + " markers eligible to become base marker.");
		}
	}

	void Start()
	{
		FindMarkers();
	}

	public ARMarker GetBaseMarker()
	{
		if (baseMarker != null) {
			if (baseMarker.Visible) return baseMarker;
			else baseMarker = null;
		}
		
		foreach (ARMarker m in markersEligibleForBaseMarker) {
			if (m.Visible) {
				baseMarker = m;
				break;
			}
		}
		
		return baseMarker;
	}
	
	void OnApplicationQuit()
	{
		RemoveAllMarkers();
	}
}

