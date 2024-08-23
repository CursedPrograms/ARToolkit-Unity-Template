using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]               
[ExecuteInEditMode]                                 
public class ARTransitionalCamera : ARTrackedCamera
{

    public Vector3 vrTargetPosition;        
	public Quaternion vrTargetRotation;     

    public GameObject targetObject;
    public float transitionAmount = 0.0f;
	public float movementRate = 1.389f;            

    private float vrObserverAzimuth = 0.0f;
    private float vrObserverElevation = 0.0f;
	private Vector3 vrObserverOffset = Vector3.zero;          

    public bool automaticTransition = false;
    public float automaticTransitionDistance = 0.3f;

    IEnumerator DoTransition(bool flyIn)
    {
		ARController artoolkit = Component.FindObjectOfType(typeof(ARController)) as ARController;

        float transitionSpeed = flyIn ? 1.0f : -1.0f;
        bool transitioning = true;

        while (transitioning) {
            transitionAmount += transitionSpeed * Time.deltaTime;

            if (transitionAmount > 1.0f) {
                transitionAmount = 1.0f;
                transitioning = false;
            }

            if (transitionAmount < 0.0f) {
                transitionAmount = 0.0f;
                transitioning = false;
            }
       
            if (artoolkit != null) artoolkit.SetVideoAlpha(1.0f - transitionAmount);

            yield return null;
        }

        print("Transition complete");
    }

    public void transitionIn()
    {
        StopCoroutine("DoTransition");
        StartCoroutine("DoTransition", true);

        this.GetComponent<AudioSource>().Play();
    }

    public void transitionOut()
    {
        StopCoroutine("DoTransition");
        StartCoroutine("DoTransition", false);

        this.GetComponent<AudioSource>().Play();
    }

    public override void Start()
    {
		base.Start();

		Matrix4x4 targetInWorldFrame = targetObject.transform.localToWorldMatrix;
		Matrix4x4 targetInCameraFrame = this.gameObject.GetComponent<Camera>().transform.parent.worldToLocalMatrix * targetInWorldFrame;
		vrTargetPosition = ARUtilityFunctions.PositionFromMatrix(targetInCameraFrame);
		vrTargetRotation = ARUtilityFunctions.QuaternionFromMatrix(targetInCameraFrame);

		vrObserverAzimuth = vrObserverElevation = 0.0f;              
		vrObserverOffset = Vector3.zero;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) {
            transitionIn();
        }

        if (Input.GetKeyDown(KeyCode.O)) {
            transitionOut();
        }

        if (automaticTransition) {
            if (arVisible) {

                if (arPosition.magnitude < automaticTransitionDistance) {

                    if (transitionAmount != 1) transitionIn();

                }
            }
        }

    }

    public void OnPreRender()
    {
        GL.ClearWithSkybox(false, this.gameObject.GetComponent<Camera>());

        this.GetComponent<Skybox>().material.SetColor("_Tint", new Color(1, 1, 1, transitionAmount));
    }

    protected override void ApplyTracking()
    {
        Vector2 look = Vector2.zero;
		Vector3 move = Vector3.zero;

		if (SystemInfo.deviceType == DeviceType.Handheld) {
			if (Input.touchCount == 1) {
	            Touch touch = Input.GetTouch(0);
				look = touch.deltaPosition;           
	        } else if (Input.touchCount == 2) {
	            if (transitionAmount <= 0) transitionIn();
	            if (transitionAmount >= 1) transitionOut();
	        }
		} else {
			look.x = -Input.GetAxis("Mouse X");    
			look.y = -Input.GetAxis("Mouse Y");    
			move.x = movementRate * Time.deltaTime * Input.GetAxis("Horizontal");
			move.y = 0.0f;
			move.z = movementRate * Time.deltaTime * Input.GetAxis("Vertical");

            if (Input.GetMouseButton(0)) transitionIn();
            if (Input.GetMouseButton(1)) transitionOut();
        }

		vrObserverAzimuth -= look.x * 0.5f;
		if (vrObserverAzimuth >= 360.0f) vrObserverAzimuth -= 360.0f;
		else if (vrObserverAzimuth < 0.0f) vrObserverAzimuth += 360.0f;
		vrObserverElevation += look.y * 0.5f;
		if (vrObserverElevation > 90.0f) vrObserverElevation = 90.0f;
		else if (vrObserverElevation < -90.0f) vrObserverElevation = -90.0f;
		Quaternion vrObserverDirection = Quaternion.Euler(vrObserverElevation, vrObserverAzimuth, 0.0f);

		vrObserverOffset += vrObserverDirection * move;

		Vector3 vrPosition = vrTargetPosition + vrTargetRotation * vrObserverOffset;
		Quaternion vrRotation = vrTargetRotation * vrObserverDirection; 
 
        if (transitionAmount < 1) {
            if (arVisible) {
				transform.localPosition = Vector3.Lerp(arPosition, vrPosition, transitionAmount);
				transform.localRotation = Quaternion.Slerp(arRotation, vrRotation, transitionAmount);
                this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
            } else {
                this.gameObject.GetComponent<Camera>().cullingMask = 0;
            }
        } else {
            this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
			transform.localPosition = vrPosition;
			transform.localRotation = vrRotation;
		}
    }

    void OnGUI()
    {
		if (SystemInfo.deviceType == DeviceType.Handheld) {

        } else {

        }
    }
       

}

