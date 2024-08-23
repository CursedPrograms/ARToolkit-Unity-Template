using System;
using UnityEngine;

[RequireComponent(typeof(Transform))]               
[ExecuteInEditMode]                                 
public class ARCamera : MonoBehaviour
{
	private const string LogTag = "ARCamera: ";
	
	public enum ViewEye
	{
		Left = 1,
		Right = 2,
	}
	
	private AROrigin _origin = null;
	protected ARMarker _marker = null;				              
	
	[NonSerialized]
	protected Vector3 arPosition = Vector3.zero;	     
	[NonSerialized]
	protected Quaternion arRotation = Quaternion.identity;      
	[NonSerialized]
	protected bool arVisible = false;				    
	[NonSerialized]
	protected float timeLastUpdate = 0;				      
	[NonSerialized]
	protected float timeTrackingLost = 0;			      
	
	public GameObject eventReceiver;
	
	public bool Stereo = false;
	public ViewEye StereoEye = ViewEye.Left;
	
	public bool Optical = false;
	private bool opticalSetupOK = false;
	public int OpticalParamsFilenameIndex = 0;
	public string OpticalParamsFilename = "";
	public byte[] OpticalParamsFileContents = new byte[0];     
	public float OpticalEyeLateralOffsetRight = 0.0f;
	private Matrix4x4 opticalViewMatrix;           
	

	public bool SetupCamera(float nearClipPlane, float farClipPlane, Matrix4x4 projectionMatrix, ref bool opticalOut)
	{
		Camera c = this.gameObject.GetComponent<Camera>();
		
		c.orthographic = false;
		
		c.nearClipPlane = nearClipPlane;
		c.farClipPlane = farClipPlane;
		
		if (Optical) {
			float fovy ;
			float aspect;
			float[] m = new float[16];
			float[] p = new float[16];
			opticalSetupOK = PluginFunctions.arwLoadOpticalParams(null, OpticalParamsFileContents, OpticalParamsFileContents.Length, out fovy, out aspect, m, p);
			if (!opticalSetupOK) {
				ARController.Log(LogTag + "Error loading optical parameters.");
				return false;
			}
			m[12] *= 0.001f;
			m[13] *= 0.001f;
			m[14] *= 0.001f;
			ARController.Log(LogTag + "Optical parameters: fovy=" + fovy  + ", aspect=" + aspect + ", camera position (m)={" + m[12].ToString("F3") + ", " + m[13].ToString("F3") + ", " + m[14].ToString("F3") + "}");
			
			c.projectionMatrix = ARUtilityFunctions.MatrixFromFloatArray(p);
			
			opticalViewMatrix = ARUtilityFunctions.MatrixFromFloatArray(m);
			if (OpticalEyeLateralOffsetRight != 0.0f) opticalViewMatrix = Matrix4x4.TRS(new Vector3(-OpticalEyeLateralOffsetRight, 0.0f, 0.0f), Quaternion.identity, Vector3.one) * opticalViewMatrix; 
			opticalViewMatrix = ARUtilityFunctions.LHMatrixFromRHMatrix(opticalViewMatrix);
			
			opticalOut = true;
		} else {
			c.projectionMatrix = projectionMatrix;
		}
		
		c.clearFlags = CameraClearFlags.Nothing;
		
		c.depth = 2;
		
		c.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		c.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
		c.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		
		return true;
	}
	
	public virtual AROrigin GetOrigin()
	{
		if (_origin == null) {
			_origin = this.gameObject.GetComponentInParent<AROrigin>();
		}
		return _origin;
	}
	
	public virtual ARMarker GetMarker()
	{
		AROrigin origin = GetOrigin();
		if (origin == null) return null;
		return (origin.GetBaseMarker());
	}
	
	private void UpdateTracking()
	{
		timeLastUpdate = Time.realtimeSinceStartup;
			
		ARMarker marker = GetMarker();
		if (marker == null) {
			if (arVisible) {
				timeTrackingLost = timeLastUpdate;
				arVisible = false;
			}
		} else {
			
			if (marker.Visible) {
				
				Matrix4x4 pose;
				if (Optical && opticalSetupOK) {
					pose = (opticalViewMatrix * marker.TransformationMatrix).inverse;
				} else {
					pose = marker.TransformationMatrix.inverse;
				}
				
				arPosition = ARUtilityFunctions.PositionFromMatrix(pose);
				arRotation = ARUtilityFunctions.QuaternionFromMatrix(pose);
				
				if (!arVisible) {
					arVisible = true;
				}
			} else {
				if (arVisible) {
					timeTrackingLost = timeLastUpdate;
					arVisible = false;
				}
			}
		}
	}
	
	protected virtual void ApplyTracking()
	{
		if (arVisible) {
			transform.localPosition = arPosition;          
			transform.localRotation = arRotation;
		}
	}
	
	public void LateUpdate()
	{
		transform.localScale = Vector3.one;
		
		if (Application.isPlaying) {
			UpdateTracking();
			ApplyTracking();
		}
	}
	
}

