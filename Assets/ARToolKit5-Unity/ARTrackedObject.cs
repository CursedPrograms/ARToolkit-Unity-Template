using UnityEngine;

[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class ARTrackedObject : MonoBehaviour
{
	private const string LogTag = "ARTrackedObject: ";

	private AROrigin _origin = null;
	private ARMarker _marker = null;

	private bool visible = false;					    
	private float timeTrackingLost = 0;				      
	public float secondsToRemainVisible = 0.0f;		            
	private bool visibleOrRemain = false;			         

	public GameObject eventReceiver;

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

	public virtual ARMarker GetMarker()
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

	public virtual AROrigin GetOrigin()
	{
		if (_origin == null) {
			_origin = this.gameObject.GetComponentInParent<AROrigin>();     
		}
		return _origin;
	}

	void Start()
	{
		if (Application.isPlaying) {
			for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(false);
		} else {
			for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(true);
		}
	}

	void LateUpdate()
	{
		transform.localScale = Vector3.one;
		
		if (Application.isPlaying) {

			AROrigin origin = GetOrigin();
			if (origin == null) {
			} else {

				ARMarker marker = GetMarker();
				if (marker == null) {
				} else {

					float timeNow = Time.realtimeSinceStartup;
					
					ARMarker baseMarker = origin.GetBaseMarker();
					if (baseMarker != null && marker.Visible) {

						if (!visible) {
							visible = visibleOrRemain = true;
							if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerFound", marker, SendMessageOptions.DontRequireReceiver);

							for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(true);
						}

                        Matrix4x4 pose;
                        if (marker == baseMarker) {
                            pose = origin.transform.localToWorldMatrix;
                        } else {
						    pose = (origin.transform.localToWorldMatrix * baseMarker.TransformationMatrix.inverse * marker.TransformationMatrix);
						}
						transform.position = ARUtilityFunctions.PositionFromMatrix(pose);
						transform.rotation = ARUtilityFunctions.QuaternionFromMatrix(pose);

						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerTracked", marker, SendMessageOptions.DontRequireReceiver);

					} else {

						if (visible) {
							visible = false;
							timeTrackingLost = timeNow;
						}

						if (visibleOrRemain && (timeNow - timeTrackingLost >= secondsToRemainVisible)) {
							visibleOrRemain = false;
							if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerLost", marker, SendMessageOptions.DontRequireReceiver);
							for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(false);
						}
					}
				}  

			}  
		}  

	}

}

