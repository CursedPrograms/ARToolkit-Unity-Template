using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ContentMode
{
	Stretch,
    Fit,
	Fill,
	OneToOne
}

public enum ContentAlign
{
	TopLeft,
	Top,
	TopRight,
	Left,
	Center,
	Right,
	BottomLeft,
	Bottom,
	BottomRight
}

[ExecuteInEditMode]
public class ARController : MonoBehaviour
{
    public static Action<String> logCallback { get; set; }
    private static List<String> logMessages = new List<String>();
    private const int MaximumLogMessages = 1000;
    private const string LogTag = "ARController: ";

	public bool UseNativeGLTexturingIfAvailable = true;
	public bool AllowNonRGBVideo = false;
	public bool QuitOnEscOrBack = true;
	public bool AutoStartAR = true;

	private string _version = "";
	private bool _running = false;
	private bool _runOnUnpause = false;
	private bool _sceneConfiguredForVideo = false;
	private bool _sceneConfiguredForVideoWaitingMessageLogged = false;
	private bool _useNativeGLTexturing = false;
	private bool _useColor32 = true;

	public string videoCParamName0 = "camera_para";
	public string videoConfigurationWindows0 = "-showDialog -flipV";
	public string videoConfigurationMacOSX0 = "-width=640 -height=480";
	public string videoConfigurationiOS0 = "";
	public string videoConfigurationAndroid0 = "";
	public int BackgroundLayer0 = 8;

	private int _videoWidth0 = 0;
	private int _videoHeight0 = 0;
	private int _videoPixelSize0 = 0;
	private string _videoPixelFormatString0 = "";
	private Matrix4x4 _videoProjectionMatrix0;

	private GameObject _videoBackgroundMeshGO0 = null;                       
	private Color[] _videoColorArray0 = null;                  
	private Color32[] _videoColor32Array0 = null;                  
	private Texture2D _videoTexture0 = null;        
	private Material _videoMaterial0 = null;             

	public bool VideoIsStereo = false;
	public string transL2RName = "transL2R";

	public string videoCParamName1 = "camera_paraR";
	public string videoConfigurationWindows1 = "-devNum=2 -showDialog -flipV";
	public string videoConfigurationMacOSX1 = "-source=1 -width=640 -height=480";
	public string videoConfigurationiOS1 = "";
	public string videoConfigurationAndroid1 = "";
	public int BackgroundLayer1 = 9;

	private int _videoWidth1 = 0;
	private int _videoHeight1 = 0;
	private int _videoPixelSize1 = 0;
	private string _videoPixelFormatString1 = "";
	private Matrix4x4 _videoProjectionMatrix1;

	private GameObject _videoBackgroundMeshGO1 = null;                       
	private Color[] _videoColorArray1 = null;                  
	private Color32[] _videoColor32Array1 = null;                  
	private Texture2D _videoTexture1 = null;        
	private Material _videoMaterial1 = null;             

	private Camera clearCamera = null;
	private GameObject _videoBackgroundCameraGO0 = null;                
	private Camera _videoBackgroundCamera0 = null;                 
	private GameObject _videoBackgroundCameraGO1 = null;              
	private Camera _videoBackgroundCamera1 = null;                 

    public float NearPlane = 0.01f;
    public float FarPlane = 5.0f;

	public bool ContentRotate90 = false;    
	public bool ContentFlipH = false;
	public bool ContentFlipV = false;
	public ContentAlign ContentAlign = ContentAlign.Center;

    public readonly static Dictionary<ContentMode, string> ContentModeNames = new Dictionary<ContentMode, string>
    {
		{ContentMode.Stretch, "Stretch"},
		{ContentMode.Fit, "Fit"},
		{ContentMode.Fill, "Fill"},
		{ContentMode.OneToOne, "1:1"},
	};

    private int frameCounter = 0;
    private float timeCounter = 0.0f;
    private float lastFramerate = 0.0f;
    private float refreshTime = 0.5f;


    public enum ARToolKitThresholdMode
    {
        Manual = 0,
        Median = 1,
        Otsu = 2,
        Adaptive = 3
    }

    public enum ARToolKitLabelingMode
    {
        WhiteRegion = 0,
        BlackRegion = 1,
    }

    public readonly static Dictionary<ARToolKitThresholdMode, string> ThresholdModeDescriptions = new Dictionary<ARToolKitThresholdMode, string>
    {
        {ARToolKitThresholdMode.Manual, "Uses a fixed threshold value"},
        {ARToolKitThresholdMode.Median, "Automatically adjusts threshold to whole-image median"},
        {ARToolKitThresholdMode.Otsu, "Automatically adjusts threshold using Otsu's method for foreground/background determination"},
        {ARToolKitThresholdMode.Adaptive, "Uses adaptive dynamic thresholding (warning: computationally expensive)"}
    };
	
	public enum ARToolKitPatternDetectionMode {
		AR_TEMPLATE_MATCHING_COLOR = 0,
		AR_TEMPLATE_MATCHING_MONO = 1,
		AR_MATRIX_CODE_DETECTION = 2,
		AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX = 3,
		AR_TEMPLATE_MATCHING_MONO_AND_MATRIX = 4
	};

	public enum ARToolKitMatrixCodeType {
	    AR_MATRIX_CODE_3x3 = 3,
    	AR_MATRIX_CODE_3x3_PARITY65 = 257,
    	AR_MATRIX_CODE_3x3_HAMMING63 = 515,
    	AR_MATRIX_CODE_4x4 = 4,
    	AR_MATRIX_CODE_4x4_BCH_13_9_3 = 772,
    	AR_MATRIX_CODE_4x4_BCH_13_5_5 = 1028
	};
	
	public enum ARToolKitImageProcMode {
		AR_IMAGE_PROC_FRAME_IMAGE = 0,
		AR_IMAGE_PROC_FIELD_IMAGE = 1
	};

	public enum ARW_UNITY_RENDER_EVENTID {
        NOP = 0,     
        UPDATE_TEXTURE_GL = 1,
		UPDATE_TEXTURE_GL_STEREO = 2,
	};

	public enum ARW_ERROR {
		ARW_ERROR_NONE                  =    0,
		ARW_ERROR_GENERIC               =   -1,
		ARW_ERROR_OUT_OF_MEMORY         =   -2,
		ARW_ERROR_OVERFLOW              =   -3,
		ARW_ERROR_NODATA				=   -4,
		ARW_ERROR_IOERROR               =   -5,
		ARW_ERROR_EOF                   =	-6,
		ARW_ERROR_TIMEOUT               =   -7,
		ARW_ERROR_INVALID_COMMAND       =   -8,
		ARW_ERROR_INVALID_ENUM          =   -9,
		ARW_ERROR_THREADS               =   -10,
		ARW_ERROR_FILE_NOT_FOUND		=   -11,
		ARW_ERROR_LENGTH_UNAVAILABLE	=	-12,
		ARW_ERROR_DEVICE_UNAVAILABLE    =   -13
	};

	[SerializeField]
	private ContentMode currentContentMode = ContentMode.Fit;
	[SerializeField]
    private ARToolKitThresholdMode currentThresholdMode = ARToolKitThresholdMode.Manual;
	[SerializeField]
    private int currentThreshold = 100;
	[SerializeField]
    private ARToolKitLabelingMode currentLabelingMode = ARToolKitLabelingMode.BlackRegion;
	[SerializeField]
	private int currentTemplateSize = 16;
	[SerializeField]
	private int currentTemplateCountMax = 25;
	[SerializeField]
	private float currentBorderSize = 0.25f;
	[SerializeField]
	private ARToolKitPatternDetectionMode currentPatternDetectionMode = ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR;
	[SerializeField]
	private ARToolKitMatrixCodeType currentMatrixCodeType = ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3;
	[SerializeField]
	private ARToolKitImageProcMode currentImageProcMode = ARToolKitImageProcMode.AR_IMAGE_PROC_FRAME_IMAGE;
	[SerializeField]
	private bool currentUseVideoBackground = true;
	[SerializeField]
	private bool currentNFTMultiMode = false;


    void Awake()
    {
        if (PluginFunctions.arwInitialiseAR(TemplateSize, TemplateCountMax)) {
			_version = PluginFunctions.arwGetARToolKitVersion();
			Log(LogTag + "ARToolKit version " + _version + " initialised.");
		} else {
            Log(LogTag + "Error initialising ARToolKit");
        }
	}

	void OnEnable()
	{
		switch (Application.platform) {
            case RuntimePlatform.OSXEditor:						     
			case RuntimePlatform.OSXPlayer:						     
				goto case RuntimePlatform.WindowsPlayer;
			case RuntimePlatform.WindowsEditor:					    
			case RuntimePlatform.WindowsPlayer:					    
                PluginFunctions.arwRegisterLogCallback(Log);
                break;
			case RuntimePlatform.Android:						    
				break;
			case RuntimePlatform.IPhonePlayer:					    
				break;
            default:
                break;
        }
	}
	
	void Start()
	{
		ARMarker[] markers = FindObjectsOfType(typeof(ARMarker)) as ARMarker[];
		foreach (ARMarker m in markers) {
			m.Load();
		}
		
		if (Application.isPlaying) {
			
			if (AutoStartAR) {
				if (!StartAR()) Application.Quit();
			}
			
		} else {
		
        }
	}
	
	void OnApplicationPause(bool paused)
	{
		if (paused) {
			if (_running) {
				StopAR();
				_runOnUnpause = true;
			}
		} else {
			if (_runOnUnpause) {
				StartAR();
				_runOnUnpause = false;
			}
		}
	}
	
	void Update()

    {
		if (Application.isPlaying) {

            if (Input.GetKeyDown(KeyCode.Menu) || Input.GetKeyDown(KeyCode.Return)) showGUIDebug = !showGUIDebug;
			if (QuitOnEscOrBack && Input.GetKeyDown(KeyCode.Escape)) Application.Quit();       
	
	        CalculateFPS();
	        
	        UpdateAR();
	
		} else {
		
        }
    }

    void OnApplicationQuit()
    {
        StopAR();
    }

	void OnDisable()
	{
		switch (Application.platform) {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
				goto case RuntimePlatform.WindowsPlayer;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                PluginFunctions.arwRegisterLogCallback(null);
                break;
            case RuntimePlatform.Android:
				break;
            case RuntimePlatform.IPhonePlayer:
				break;
			default:
                break;
        }

    }
	
    void OnDestroy()
	{
		Log(LogTag + "Shutting down ARToolKit");
		if (!PluginFunctions.arwShutdownAR ()) {
			Log(LogTag + "Error shutting down ARToolKit.");
		}

	}
	
	public bool StartAR()
	{
        if (_running) {
            Log(LogTag + "WARNING: StartAR() called while already running. Ignoring.\n");
            return false;
        }
        
        Log(LogTag + "Starting AR.");

		_sceneConfiguredForVideo = _sceneConfiguredForVideoWaitingMessageLogged = false;
        
        string renderDevice = SystemInfo.graphicsDeviceVersion;
        _useNativeGLTexturing = !renderDevice.StartsWith("Direct") && UseNativeGLTexturingIfAvailable;
        if (_useNativeGLTexturing) {
            Log(LogTag + "Render device: " + renderDevice + ", using native GL texturing.");
        } else {
            Log(LogTag + "Render device: " + renderDevice + ", using Unity texturing.");
        }

        CreateClearCamera();
        
        string videoConfiguration0;
		string videoConfiguration1;
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
				videoConfiguration0 = videoConfigurationMacOSX0;
				videoConfiguration1 = videoConfigurationMacOSX1;
				if (_useNativeGLTexturing || !AllowNonRGBVideo) {
					if (videoConfiguration0.IndexOf("-device=QuickTime7") != -1 || videoConfiguration0.IndexOf("-device=QUICKTIME") != -1) videoConfiguration0 += " -pixelformat=BGRA";
					if (videoConfiguration1.IndexOf("-device=QuickTime7") != -1 || videoConfiguration1.IndexOf("-device=QUICKTIME") != -1) videoConfiguration1 += " -pixelformat=BGRA";
				}
				break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                videoConfiguration0 = videoConfigurationWindows0;
				videoConfiguration1 = videoConfigurationWindows1;
				if (_useNativeGLTexturing || !AllowNonRGBVideo) {
					if (videoConfiguration0.IndexOf("-device=WinMF") != -1) videoConfiguration0 += " -format=BGRA";
					if (videoConfiguration1.IndexOf("-device=WinMF") != -1) videoConfiguration1 += " -format=BGRA";
				}
				break;
            case RuntimePlatform.Android:
				videoConfiguration0 = videoConfigurationAndroid0 + " -cachedir=\"" + Application.temporaryCachePath + "\""  + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=RGBA" : "");
				videoConfiguration1 = videoConfigurationAndroid1 + " -cachedir=\"" + Application.temporaryCachePath + "\""  + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=RGBA" : "");
				break;
            case RuntimePlatform.IPhonePlayer:
				videoConfiguration0 = videoConfigurationiOS0 + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=BGRA" : "");
				videoConfiguration1 = videoConfigurationiOS1 + (_useNativeGLTexturing || !AllowNonRGBVideo ? " -format=BGRA" : "");
				break;
			default:
                videoConfiguration0 = "";			
				videoConfiguration1 = "";			
				break;
        }	

		TextAsset ta;
		byte[] cparam0 = null;
		byte[] cparam1 = null;
		byte[] transL2R = null;
        ta = Resources.Load("ardata/" + videoCParamName0, typeof(TextAsset)) as TextAsset;
        if (ta == null) {		
			Log(LogTag + "StartAR(): Error: Camera parameters file not found at Resources/ardata/" + videoCParamName0 + ".bytes");
            return (false);
        }
        cparam0 = ta.bytes;
		if (VideoIsStereo) {
			ta = Resources.Load("ardata/" + videoCParamName1, typeof(TextAsset)) as TextAsset;
			if (ta == null) {		
				Log(LogTag + "StartAR(): Error: Camera parameters file not found at Resources/ardata/" + videoCParamName1 + ".bytes");
				return (false);
			}
			cparam1 = ta.bytes;
			ta = Resources.Load("ardata/" + transL2RName, typeof(TextAsset)) as TextAsset;
			if (ta == null) {		
				Log(LogTag + "StartAR(): Error: The stereo calibration file not found at Resources/ardata/" + transL2RName + ".bytes");
				return (false);
			}
			transL2R = ta.bytes;
		}
        
		if (!VideoIsStereo) {
			Log(LogTag + "Starting ARToolKit video with vconf '" + videoConfiguration0 + "'.");
			_running = PluginFunctions.arwStartRunningB(videoConfiguration0, cparam0, cparam0.Length, NearPlane, FarPlane);
		} else {
			Log(LogTag + "Starting ARToolKit video with vconfL '" + videoConfiguration0 + "', vconfR '" + videoConfiguration1 + "'.");
			_running = PluginFunctions.arwStartRunningStereoB(videoConfiguration0, cparam0, cparam0.Length, videoConfiguration1, cparam1, cparam1.Length, transL2R, transL2R.Length, NearPlane, FarPlane);

		}
        
        if (!_running) {
            Log(LogTag + "Error starting running");
			ARW_ERROR error = (ARW_ERROR)PluginFunctions.arwGetError();
			if (error == ARW_ERROR.ARW_ERROR_DEVICE_UNAVAILABLE) {
				showGUIErrorDialogContent = "Unable to start AR tracking. The camera may be in use by another application.";
			} else {
				showGUIErrorDialogContent = "Unable to start AR tracking. Please check that you have a camera connected.";
			}
			showGUIErrorDialog = true;
            return false;
        }
        
        Log(LogTag + "Setting ARToolKit tracking settings.");
        VideoThreshold = currentThreshold;
        VideoThresholdMode = currentThresholdMode;
        LabelingMode = currentLabelingMode;
        BorderSize = currentBorderSize;
        PatternDetectionMode = currentPatternDetectionMode;
        MatrixCodeType = currentMatrixCodeType;
        ImageProcMode = currentImageProcMode;
		NFTMultiMode = currentNFTMultiMode;
        
		return true;
	}
	
	bool UpdateAR()
	{
        if (!_running) {
            return false;
        }
        
        if (!_sceneConfiguredForVideo) {
            
            if (!PluginFunctions.arwIsRunning()) {
				if (!_sceneConfiguredForVideoWaitingMessageLogged) {
					Log(LogTag + "UpdateAR: Waiting for ARToolKit video.");
					_sceneConfiguredForVideoWaitingMessageLogged = true;
				}
            } else {
				Log(LogTag + "UpdateAR: ARToolKit video is running. Configuring Unity scene for video.");
		
				if (!VideoIsStereo) {

					bool ok1 = PluginFunctions.arwGetVideoParams(out _videoWidth0, out _videoHeight0, out _videoPixelSize0, out _videoPixelFormatString0);
					if (!ok1) return false;
					Log(LogTag + "Video " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + ")");
					
					float[] projRaw = new float[16];
					PluginFunctions.arwGetProjectionMatrix(projRaw);
					_videoProjectionMatrix0 = ARUtilityFunctions.MatrixFromFloatArray(projRaw);
					Log(LogTag + "Projection matrix: [" + Environment.NewLine + _videoProjectionMatrix0.ToString().Trim() + "]");
					if (ContentRotate90) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * _videoProjectionMatrix0;
					if (ContentFlipV) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1.0f, -1.0f, 1.0f)) * _videoProjectionMatrix0;
					if (ContentFlipH) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1.0f, 1.0f, 1.0f)) * _videoProjectionMatrix0;

					_videoBackgroundMeshGO0 = CreateVideoBackgroundMesh(0, _videoWidth0, _videoHeight0, BackgroundLayer0, out _videoColorArray0, out _videoColor32Array0, out _videoTexture0, out _videoMaterial0);
					if (_videoBackgroundMeshGO0 == null || _videoTexture0 == null || _videoMaterial0 == null) {
						Log (LogTag + "Error: unable to create video mesh.");
					}

				} else {

					bool ok1 = PluginFunctions.arwGetVideoParamsStereo(out _videoWidth0, out _videoHeight0, out _videoPixelSize0, out _videoPixelFormatString0, out _videoWidth1, out _videoHeight1, out _videoPixelSize1, out _videoPixelFormatString1);
					if (!ok1) return false;
					Log(LogTag + "Video left " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + "), right " + _videoWidth1 + "x" + _videoHeight1 + "@" + _videoPixelSize1 + "Bpp (" + _videoPixelFormatString1 + ")");
					
					float[] projRaw0 = new float[16];
					float[] projRaw1 = new float[16];
					PluginFunctions.arwGetProjectionMatrixStereo(projRaw0, projRaw1);
					_videoProjectionMatrix0 = ARUtilityFunctions.MatrixFromFloatArray(projRaw0);
					_videoProjectionMatrix1 = ARUtilityFunctions.MatrixFromFloatArray(projRaw1);
					Log(LogTag + "Projection matrix left: [" + Environment.NewLine + _videoProjectionMatrix0.ToString().Trim() + "], right: [" + Environment.NewLine + _videoProjectionMatrix1.ToString().Trim() + "]");
					if (ContentRotate90) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * _videoProjectionMatrix0;
					if (ContentRotate90) _videoProjectionMatrix1 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * _videoProjectionMatrix1;
					if (ContentFlipV) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1.0f, -1.0f, 1.0f)) * _videoProjectionMatrix0;
					if (ContentFlipV) _videoProjectionMatrix1 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1.0f, -1.0f, 1.0f)) * _videoProjectionMatrix1;
					if (ContentFlipH) _videoProjectionMatrix0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1.0f, 1.0f, 1.0f)) * _videoProjectionMatrix0;
					if (ContentFlipH) _videoProjectionMatrix1 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1.0f, 1.0f, 1.0f)) * _videoProjectionMatrix1;

					_videoBackgroundMeshGO0 = CreateVideoBackgroundMesh(0, _videoWidth0, _videoHeight0, BackgroundLayer0, out _videoColorArray0, out _videoColor32Array0, out _videoTexture0, out _videoMaterial0);
					_videoBackgroundMeshGO1 = CreateVideoBackgroundMesh(1, _videoWidth1, _videoHeight1, BackgroundLayer1, out _videoColorArray1, out _videoColor32Array1, out _videoTexture1, out _videoMaterial1);
					if (_videoBackgroundMeshGO0 == null || _videoTexture0 == null || _videoMaterial0 == null || _videoBackgroundMeshGO1 == null || _videoTexture1 == null || _videoMaterial1 == null) {
						Log (LogTag + "Error: unable to create video background mesh.");
					}
				}
	            
				bool haveStereoARCameras = false;
				ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
				foreach (ARCamera arc in arCameras) {
					if (arc.Stereo) haveStereoARCameras = true;
				}
				if (!haveStereoARCameras) {
					_videoBackgroundCameraGO0 = CreateVideoBackgroundCamera("Video background", BackgroundLayer0, out _videoBackgroundCamera0);
					if (_videoBackgroundCameraGO0 == null || _videoBackgroundCamera0 == null) {
						Log (LogTag + "Error: unable to create video background camera.");
					}
				} else {
					_videoBackgroundCameraGO0 = CreateVideoBackgroundCamera("Video background (L)", BackgroundLayer0, out _videoBackgroundCamera0);
					_videoBackgroundCameraGO1 = CreateVideoBackgroundCamera("Video background (R)", (VideoIsStereo ? BackgroundLayer1 : BackgroundLayer0), out _videoBackgroundCamera1);
					if (_videoBackgroundCameraGO0 == null || _videoBackgroundCamera0 == null || _videoBackgroundCameraGO1 == null || _videoBackgroundCamera1 == null) {
						Log (LogTag + "Error: unable to create video background camera.");
					}
				}

				ConfigureForegroundCameras();

				ConfigureViewports();

				if (_useNativeGLTexturing) {
					if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android) {
						if (!VideoIsStereo) PluginFunctions.arwSetUnityRenderEventUpdateTextureGLTextureID(_videoTexture0.GetNativeTextureID());
						else PluginFunctions.arwSetUnityRenderEventUpdateTextureGLStereoTextureIDs(_videoTexture0.GetNativeTextureID(), _videoTexture1.GetNativeTextureID());
					}
				}

				Log (LogTag + "Scene configured for video.");
	            _sceneConfiguredForVideo = true;     
	        }  
		}  
        
		bool gotFrame = PluginFunctions.arwCapture();
		bool ok = PluginFunctions.arwUpdateAR();
		if (!ok) return false;
		if (gotFrame) {
		    if (_sceneConfiguredForVideo && UseVideoBackground) {
	        	UpdateTexture();
        	}
		}

		return true;
	}
	
	public bool StopAR()
	{
        if (!_running) {
            return false;
        }
        
		Log(LogTag + "Stopping AR.");

    	if (!PluginFunctions.arwStopRunning()) {
            Log(LogTag + "Error stopping AR.");
        }
		
		DestroyVideoBackground();
		DestroyClearCamera();

		_running = false;
		return true;
	}

	public void SetContentForScreenOrientation(bool cameraIsFrontFacing)
	{
		ScreenOrientation orientation = Screen.orientation;
		if (orientation == ScreenOrientation.Portrait) {  
			ContentRotate90 = true;
			ContentFlipV = false;
			ContentFlipH = cameraIsFrontFacing;
		} else if (orientation == ScreenOrientation.LandscapeLeft) {        
			ContentRotate90 = false;
			ContentFlipV = false;
			ContentFlipH = cameraIsFrontFacing;
		} else if (orientation == ScreenOrientation.PortraitUpsideDown) {   
			ContentRotate90 = true;
			ContentFlipV = true;
			ContentFlipH = (!cameraIsFrontFacing);
		} else if (orientation == ScreenOrientation.LandscapeRight) {        
			ContentRotate90 = false;
			ContentFlipV = true;
			ContentFlipH = (!cameraIsFrontFacing);
		}
	}

    public void SetVideoAlpha(float a)
    {
        if (_videoMaterial0 != null) {
            _videoMaterial0.color = new Color(1.0f, 1.0f, 1.0f, a);
        }
		if (_videoMaterial1 != null) {
			_videoMaterial1.color = new Color(1.0f, 1.0f, 1.0f, a);
		}
	}


    public bool DebugVideo
    {
        get
        {
			return (PluginFunctions.arwGetVideoDebugMode());
        }

        set
        {
            PluginFunctions.arwSetVideoDebugMode(value);
        }
    }

	public string Version
	{
		get
		{
			return _version;
		}
	}

    public ARController.ARToolKitThresholdMode VideoThresholdMode
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetVideoThresholdMode();
                if (ret >= 0) currentThresholdMode = (ARController.ARToolKitThresholdMode)ret;
				else currentThresholdMode = ARController.ARToolKitThresholdMode.Manual;
            }
            return currentThresholdMode;
        }

        set
        {
            currentThresholdMode = value;
            if (_running) {
				PluginFunctions.arwSetVideoThresholdMode((int)currentThresholdMode);
            }
        }
    }

    public int VideoThreshold
    {
        get
        {
            if (_running) {
				currentThreshold = PluginFunctions.arwGetVideoThreshold();
            	if (currentThreshold < 0 || currentThreshold > 255) currentThreshold = 100;
			}
            return currentThreshold;
        }

        set
        {
            currentThreshold = value;
            if (_running) {
                PluginFunctions.arwSetVideoThreshold(value);
            }
        }
    }

    public ARController.ARToolKitLabelingMode LabelingMode
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetLabelingMode();
                if (ret >= 0) currentLabelingMode = (ARController.ARToolKitLabelingMode)ret;
				else currentLabelingMode = ARController.ARToolKitLabelingMode.BlackRegion;
            }
            return currentLabelingMode;
        }

        set
        {
            currentLabelingMode = value;
            if (_running) {
				PluginFunctions.arwSetLabelingMode((int)currentLabelingMode);
            }
        }
    }

    public float BorderSize
    {
        get
        {
			float ret;
            if (_running) {
				ret = PluginFunctions.arwGetBorderSize();
                if (ret > 0.0f && ret < 0.5f) currentBorderSize = ret;
				else currentBorderSize = 0.25f;
            }
            return currentBorderSize;
        }

        set
        {
            currentBorderSize = value;
            if (_running) {
				PluginFunctions.arwSetBorderSize(currentBorderSize);
            }
        }
    }

	public int TemplateSize
	{
		get
		{
			return currentTemplateSize;
		}
		
		set
		{
			currentTemplateSize = value;
			Log (LogTag + "Warning: template size changed. Please reload scene.");
		}
	}
	
	public int TemplateCountMax
	{
		get
		{
			return currentTemplateCountMax;
		}
		
		set
		{
			currentTemplateCountMax = value;
			Log (LogTag + "Warning: template maximum count changed. Please reload scene.");
		}
	}
	
	public ARController.ARToolKitPatternDetectionMode PatternDetectionMode
	{
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetPatternDetectionMode();
                if (ret >= 0) currentPatternDetectionMode = (ARController.ARToolKitPatternDetectionMode)ret;
				else currentPatternDetectionMode = ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR;
            }
            return currentPatternDetectionMode;
        }

        set
        {
            currentPatternDetectionMode = value;
            if (_running) {
				PluginFunctions.arwSetPatternDetectionMode((int)currentPatternDetectionMode);
            }
        }
    }

    public ARController.ARToolKitMatrixCodeType MatrixCodeType
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetMatrixCodeType();
                if (ret >= 0) currentMatrixCodeType = (ARController.ARToolKitMatrixCodeType)ret;
				else currentMatrixCodeType = ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3;
            }
            return currentMatrixCodeType;
        }

        set
        {
            currentMatrixCodeType = value;
            if (_running) {
				PluginFunctions.arwSetMatrixCodeType((int)currentMatrixCodeType);
            }
        }
    }

    public ARController.ARToolKitImageProcMode ImageProcMode
    {
        get
        {
			int ret;
            if (_running) {
				ret = PluginFunctions.arwGetImageProcMode();
                if (ret >= 0) currentImageProcMode = (ARController.ARToolKitImageProcMode)ret;
				else currentImageProcMode = ARController.ARToolKitImageProcMode.AR_IMAGE_PROC_FRAME_IMAGE;
            }
            return currentImageProcMode;
        }

        set
        {
            currentImageProcMode = value;
            if (_running) {
				PluginFunctions.arwSetImageProcMode((int)currentImageProcMode);
            }
        }
    }

	public bool NFTMultiMode
	{
		get
		{
			if (_running) {
				currentNFTMultiMode = PluginFunctions.arwGetNFTMultiMode();
			}
			return currentNFTMultiMode;
		}
		
		set
		{
			currentNFTMultiMode = value;
			if (_running) {
				PluginFunctions.arwSetNFTMultiMode(currentNFTMultiMode);
			}
		}
	}

	public ContentMode ContentMode
	{
		get
		{
			return currentContentMode;
		}
		
		set
		{
			if (currentContentMode != value) {
				currentContentMode = value;
				if (_running) {
					ConfigureViewports();
				}
			}
		}
	}
	
	public bool UseVideoBackground
	{
		get
		{
			return currentUseVideoBackground;
		}
		
		set
		{
			currentUseVideoBackground = value;
			if (clearCamera != null) clearCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, (currentUseVideoBackground ? 1.0f : 0.0f));
			if (_videoBackgroundCamera0 != null) _videoBackgroundCamera0.enabled = currentUseVideoBackground;
			if (_videoBackgroundCamera1 != null) _videoBackgroundCamera1.enabled = currentUseVideoBackground;
		}
	}
	
	private void UpdateTexture()
    {
        if (!_running) return;


		if (!VideoIsStereo) {

			if (_videoTexture0 == null) {
				Log(LogTag + "Error: No video texture to update.");
			} else {

				if (_useNativeGLTexturing) {
					
					if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) {
						PluginFunctions.arwUpdateTextureGL(_videoTexture0.GetNativeTextureID());
					} else {
						GL.IssuePluginEvent((int)ARW_UNITY_RENDER_EVENTID.UPDATE_TEXTURE_GL);
					}
					
				} else {
					
					if (_videoColor32Array0 != null) {

						bool updatedTexture = PluginFunctions.arwUpdateTexture32(_videoColor32Array0);
						if (updatedTexture) {
							_videoTexture0.SetPixels32(_videoColor32Array0);
							_videoTexture0.Apply(false);
						}
					} else if (_videoColorArray0 != null) {

						bool updatedTexture = PluginFunctions.arwUpdateTexture(_videoColorArray0);
						if (updatedTexture) {
							_videoTexture0.SetPixels(0, 0, _videoWidth0, _videoHeight0, _videoColorArray0);
							_videoTexture0.Apply(false);
						}
					} else {
						Log(LogTag + "Error: No video color array to update.");
					}

				}
			}

		} else {

			if (_videoTexture0 == null || _videoTexture1 == null) {
				Log(LogTag + "Error: No video textures to update.");
			} else {
				
				if (_useNativeGLTexturing) {
					
					if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) {
						PluginFunctions.arwUpdateTextureGLStereo(_videoTexture0.GetNativeTextureID(), _videoTexture1.GetNativeTextureID());
					} else {
						GL.IssuePluginEvent((int)ARW_UNITY_RENDER_EVENTID.UPDATE_TEXTURE_GL_STEREO);
					}

				} else {
					
					if (_videoColor32Array0 != null && _videoColor32Array1 != null) {
					
						bool updatedTexture = PluginFunctions.arwUpdateTexture32Stereo(_videoColor32Array0, _videoColor32Array1);
						if (updatedTexture) {
						
							_videoTexture0.SetPixels32(_videoColor32Array0);
							_videoTexture1.SetPixels32(_videoColor32Array1);

							_videoTexture0.Apply(false);
							_videoTexture1.Apply(false);
						}
					} else if (_videoColorArray0 != null && _videoColorArray1 != null) {
					
						bool updatedTexture = PluginFunctions.arwUpdateTextureStereo(_videoColorArray0, _videoColorArray1);
						if (updatedTexture) {
						
							_videoTexture0.SetPixels(0, 0, _videoWidth0, _videoHeight0, _videoColorArray0);
							_videoTexture1.SetPixels(0, 0, _videoWidth1, _videoHeight1, _videoColorArray1);

							_videoTexture0.Apply(false);
							_videoTexture1.Apply(false);
						}
					} else {
						Log(LogTag + "Error: No video color array to update.");
					}
				}
			}


		}


    }

	private bool CreateClearCamera()
    {
		clearCamera = this.gameObject.GetComponent<Camera>();
		if (clearCamera == null) {
			clearCamera = this.gameObject.AddComponent<Camera>();    
		}

        clearCamera.depth = 0;
        clearCamera.cullingMask = 0;
		
        clearCamera.clearFlags = CameraClearFlags.SolidColor;
        if (UseVideoBackground) clearCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else clearCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        return true;
    }
	
	private GameObject CreateVideoBackgroundMesh(int index, int w, int h, int layer, out Color[] vbca, out Color32[] vbc32a, out Texture2D vbt, out Material vbm)
	{
		if (w <= 0 || h <= 0) {
			Log(LogTag + "Error: Cannot configure video texture with invalid video size: " + w + "x" + h);
			vbca = null; vbc32a = null; vbt = null; vbm = null;
			return null;
		}
		
		GameObject vbmgo = new GameObject("Video source " + index);
		if (vbmgo == null) {
			Log(LogTag + "Error: CreateVideoBackgroundCamera cannot create GameObject.");
			vbca = null; vbc32a = null; vbt = null; vbm = null;
			return null;
		}
		vbmgo.layer = layer;      

		int textureWidth;
		int textureHeight;
			textureWidth = w;
			textureHeight = h;
		Log(LogTag + "Video size " + w + "x" + h + " will use texture size " + textureWidth + "x" + textureHeight + ".");
		
		float textureScaleU = (float)w / (float)textureWidth;
		float textureScaleV = (float)h / (float)textureHeight;
		if (!_useNativeGLTexturing) {
			if (_useColor32) {
				vbca = null;
				vbc32a = new Color32[w * h];
			} else {
				vbca = new Color[w * h];
				vbc32a = null;
			}
		} else {
			vbca = null;
			vbc32a = null;
		}
		if (!_useNativeGLTexturing && _useColor32) vbt = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
		else vbt = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
		vbt.hideFlags = HideFlags.HideAndDontSave;
		vbt.filterMode = FilterMode.Bilinear;
		vbt.wrapMode = TextureWrapMode.Clamp;
		vbt.anisoLevel = 0;
		
		Color32[] arr = new Color32[textureWidth * textureHeight];
		Color32 blackOpaque = new Color32(0, 0, 0, 255);
		for (int i = 0; i < arr.Length; i++) arr[i] = blackOpaque;
		vbt.SetPixels32(arr);
		vbt.Apply();       
		arr = null;

		string shaderSource = 
			    "Shader \"VideoPlaneNoLight\" {"+
				"  Properties {"+
				"    _Color (\"Main Color\", Color) = (1,1,1,1)"+
				"    _MainTex (\"Base (RGB)\", 2D) = \"white\" { }"+
				"  }"+
				"  SubShader {"+
				"    Pass {"+
				"      Material {"+
				"        Diffuse [_Color]"+
				"      }"+
				"      Lighting Off"+
				"      ZWrite Off"+
				"      Blend SrcAlpha OneMinusSrcAlpha"+
				"      SeparateSpecular Off"+
				"      SetTexture [_MainTex] {"+
				"        constantColor [_Color]"+
				"        Combine texture * constant, texture * constant"+
				"      }"+
				"    }"+
				"  }"+
				"} ";
		vbm = new Material(shaderSource); 
		vbm.shader.hideFlags = HideFlags.HideAndDontSave;
		vbm.hideFlags = HideFlags.HideAndDontSave;
		vbm.mainTexture = vbt;
		MeshFilter filter = vbmgo.AddComponent<MeshFilter>();
		filter.mesh = newVideoMesh(ContentFlipH, !ContentFlipV, textureScaleU, textureScaleV);            
		MeshRenderer meshRenderer = vbmgo.AddComponent<MeshRenderer>();
		meshRenderer.castShadows = false;
		meshRenderer.receiveShadows = false;
		vbmgo.GetComponent<Renderer>().material = vbm;
		
		return vbmgo;
	}

	private GameObject CreateVideoBackgroundCamera(String name, int layer, out Camera vbc)
	{
		GameObject vbcgo = new GameObject(name);
		if (vbcgo == null) {
			Log(LogTag + "Error: CreateVideoBackgroundCamera cannot create GameObject.");
			vbc = null;
			return null;
		}
		vbc = vbcgo.AddComponent<Camera>();
		if (vbc == null) {
			Log(LogTag + "Error: CreateVideoBackgroundCamera cannot add Camera to GameObject.");
			return null;
		}

		vbc.orthographic = true;
		vbc.projectionMatrix = Matrix4x4.identity;
		if (ContentRotate90) vbc.projectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * vbc.projectionMatrix;
		vbc.projectionMatrix = Matrix4x4.Ortho(-1.0f, 1.0f, -1.0f, 1.0f, 0.0f, 1.0f) * vbc.projectionMatrix;
		vbc.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		vbc.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
		vbc.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		
		vbc.clearFlags = CameraClearFlags.Nothing;
		
		vbc.cullingMask = 1 << layer;
		
		vbc.depth = 1;

		vbc.enabled = UseVideoBackground;

		return vbcgo;
	}
	
	private void DestroyVideoBackground()
	{
		bool ed = Application.isEditor;

		_videoBackgroundCamera0 = null;
		_videoBackgroundCamera1 = null;
		if (_videoBackgroundCameraGO0 != null) {
			if (ed) DestroyImmediate(_videoBackgroundCameraGO0);
			else Destroy(_videoBackgroundCameraGO0);
			_videoBackgroundCameraGO0 = null;
		}
		if (_videoBackgroundCameraGO1 != null) {
			if (ed) DestroyImmediate(_videoBackgroundCameraGO1);
			else Destroy(_videoBackgroundCameraGO1);
			_videoBackgroundCameraGO1 = null;
		}

		if (_videoMaterial0 != null) {
			if (ed) DestroyImmediate(_videoMaterial0);
			else Destroy(_videoMaterial0);
			_videoMaterial0 = null;
		}
		if (_videoMaterial1 != null) {
			if (ed) DestroyImmediate(_videoMaterial1);
			else Destroy(_videoMaterial1);
			_videoMaterial1 = null;
		}
		if (_videoTexture0 != null) {
			if (ed) DestroyImmediate(_videoTexture0);
			else Destroy(_videoTexture0);
			_videoTexture0 = null;
		}
		if (_videoTexture1 != null) {
			if (ed) DestroyImmediate(_videoTexture1);
			else Destroy(_videoTexture1);
			_videoTexture1 = null;
		}
		if (_videoColorArray0 != null) _videoColorArray0 = null;
		if (_videoColorArray1 != null) _videoColorArray1 = null;
		if (_videoColor32Array0 != null) _videoColor32Array0 = null;
		if (_videoColor32Array1 != null) _videoColor32Array1 = null;
		if (_videoBackgroundMeshGO0 != null) {
			if (ed) DestroyImmediate(_videoBackgroundMeshGO0);
			else Destroy(_videoBackgroundMeshGO0);
			_videoBackgroundMeshGO0 = null;
		}
		if (_videoBackgroundMeshGO1 != null) {
			if (ed) DestroyImmediate(_videoBackgroundMeshGO1);
			else Destroy(_videoBackgroundMeshGO1);
			_videoBackgroundMeshGO1 = null;
		}
		Resources.UnloadUnusedAssets();
	}

	private bool DestroyClearCamera()
	{
		if (clearCamera != null) {
			clearCamera = null;
		}

		return true;
	}

	private Rect getViewport(int contentWidth, int contentHeight, bool stereo, ARCamera.ViewEye viewEye)
	{
		int backingWidth = Screen.width;
		int backingHeight = Screen.height;
		int left, bottom, w, h;

		if (stereo) {
			w = backingWidth / 2;
			h = backingHeight;
			if (viewEye == ARCamera.ViewEye.Left) left = 0;
			else left = backingWidth / 2;
			bottom = 0;
		} else {
			if (ContentMode == ContentMode.Stretch) {
				w = backingWidth;
				h = backingHeight;
			} else {
				int contentWidthFinalOrientation = (ContentRotate90 ? contentHeight : contentWidth);
				int contentHeightFinalOrientation = (ContentRotate90 ? contentWidth : contentHeight);
				if (ContentMode == ContentMode.Fit || ContentMode == ContentMode.Fill) {
					float scaleRatioWidth, scaleRatioHeight, scaleRatio;
					scaleRatioWidth = (float)backingWidth / (float)contentWidthFinalOrientation;
					scaleRatioHeight = (float)backingHeight / (float)contentHeightFinalOrientation;
					if (ContentMode == ContentMode.Fill) scaleRatio = Math.Max(scaleRatioHeight, scaleRatioWidth);
					else scaleRatio = Math.Min(scaleRatioHeight, scaleRatioWidth);
					w = (int)((float)contentWidthFinalOrientation * scaleRatio);
					h = (int)((float)contentHeightFinalOrientation * scaleRatio);
				} else {  
					w = contentWidthFinalOrientation;
					h = contentHeightFinalOrientation;
				}
			}
			
			if (ContentAlign == ContentAlign.TopLeft
			    || ContentAlign == ContentAlign.Left
			    || ContentAlign == ContentAlign.BottomLeft) left = 0;
			else if (ContentAlign == ContentAlign.TopRight
			         || ContentAlign == ContentAlign.Right
			         || ContentAlign == ContentAlign.BottomRight) left = backingWidth - w;
			else left = (backingWidth - w) / 2;
			
			if (ContentAlign == ContentAlign.BottomLeft
			    || ContentAlign == ContentAlign.Bottom
			    || ContentAlign == ContentAlign.BottomRight) bottom = 0;
			else if (ContentAlign == ContentAlign.TopLeft
			         || ContentAlign == ContentAlign.Top
			         || ContentAlign == ContentAlign.TopRight) bottom = backingHeight - h;
			else bottom = (backingHeight - h) / 2;
		}

		return new Rect(left, bottom, w, h);
	}

	private void CycleContentMode()
	{
		switch (ContentMode) {
		case ContentMode.Fit:
			ContentMode = ContentMode.Stretch;
			break;
		default:
			ContentMode = ContentMode.Fit;
			break;
		}
	}

	private bool ConfigureForegroundCameras()
	{
		bool optical = false;
		
		ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
		foreach (ARCamera arc in arCameras) {
			
			bool ok;
			if (!arc.Stereo) {
				ok = arc.SetupCamera(NearPlane, FarPlane, _videoProjectionMatrix0, ref optical);
			} else {
				if (arc.StereoEye == ARCamera.ViewEye.Left) {
					ok = arc.SetupCamera(NearPlane, FarPlane, _videoProjectionMatrix0, ref optical);
				} else {
					ok = arc.SetupCamera(NearPlane, FarPlane, (VideoIsStereo ? _videoProjectionMatrix1 : _videoProjectionMatrix0), ref optical);
				}
			}
			if (!ok) {
				Log(LogTag + "Error setting up ARCamera.");
			}
		}

		UseVideoBackground = !optical;
		
		return true;
	}
	
	private bool ConfigureViewports()
	{
		bool haveStereoARCamera = false;

		ARCamera[] arCameras = FindObjectsOfType(typeof(ARCamera)) as ARCamera[];
		foreach (ARCamera arc in arCameras) {
			if (!arc.Stereo) {
				arc.gameObject.GetComponent<Camera>().pixelRect = getViewport(_videoWidth0, _videoHeight0, false, ARCamera.ViewEye.Left);
			} else {
				haveStereoARCamera = true;
				if (arc.StereoEye == ARCamera.ViewEye.Left) {
					arc.gameObject.GetComponent<Camera>().pixelRect = getViewport(_videoWidth0, _videoHeight0, true, ARCamera.ViewEye.Left);
				} else {
					arc.gameObject.GetComponent<Camera>().pixelRect = getViewport((VideoIsStereo ? _videoWidth1 : _videoWidth0), (VideoIsStereo ? _videoHeight1 : _videoHeight0), true, ARCamera.ViewEye.Right);
				}
			}
		}

		if (!haveStereoARCamera) {
			_videoBackgroundCamera0.pixelRect = getViewport(_videoWidth0, _videoHeight0, false, ARCamera.ViewEye.Left);
		} else {
			_videoBackgroundCamera0.pixelRect = getViewport(_videoWidth0, _videoHeight0, true, ARCamera.ViewEye.Left);
			_videoBackgroundCamera1.pixelRect = getViewport((VideoIsStereo ? _videoWidth1 : _videoWidth0), (VideoIsStereo ? _videoHeight1 : _videoHeight0), true, ARCamera.ViewEye.Right);
		}

#if UNITY_ANDROID
		if (Application.platform == RuntimePlatform.Android) {
			using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
				jo.Call("setStereo", new object[] {haveStereoARCamera});
			}
		}		
#endif
		return true;
	}

    private Mesh newVideoMesh(bool flipX, bool flipY, float textureScaleU, float textureScaleV)
    {
        Mesh m = new Mesh();
        m.Clear();

        float r = 1.0f;

        m.vertices = new Vector3[] { 
                new Vector3(-r, -r, 0.5f), 
                new Vector3( r, -r, 0.5f), 
                new Vector3( r,  r, 0.5f),
                new Vector3(-r,  r, 0.5f),
            };

        m.normals = new Vector3[] { 
                new Vector3(0.0f, 0.0f, 1.0f), 
                new Vector3(0.0f, 0.0f, 1.0f), 
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
            };

        float u1 = flipX ? textureScaleU : 0.0f;
        float u2 = flipX ? 0.0f : textureScaleU;

        float v1 = flipY ? textureScaleV : 0.0f;
        float v2 = flipY ? 0.0f : textureScaleV;

        m.uv = new Vector2[] { 
                new Vector2(u1, v1), 
                new Vector2(u2, v1), 
                new Vector2(u2, v2),
                new Vector2(u1, v2)
            };

        m.triangles = new int[] { 
                2, 1, 0,
                3, 2, 0
            };

        ;
		return m;
    }

    public static void Log(String msg)
    {
        logMessages.Add(msg);
        while (logMessages.Count > MaximumLogMessages) logMessages.RemoveAt(0);

        if (logCallback != null) logCallback(msg);
        else Debug.Log(msg);
    }

    private void CalculateFPS()
    {
        if (timeCounter < refreshTime) {
            timeCounter += Time.deltaTime;
            frameCounter++;
        } else {
            lastFramerate = (float)frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
        }
    }
    

    private GUIStyle[] style = new GUIStyle[3];
    private bool guiSetup = false;

    private void SetupGUI()
    {

        style[0] = new GUIStyle(GUI.skin.label);
        style[0].normal.textColor = new Color(0, 0, 0, 1);

        style[1] = new GUIStyle(GUI.skin.label);
        style[1].normal.textColor = new Color(0.0f, 0.5f, 0.0f, 1);

        style[2] = new GUIStyle(GUI.skin.label);
        style[2].normal.textColor = new Color(0.5f, 0.0f, 0.0f, 1);

        guiSetup = true;
    }
	
	private bool showGUIErrorDialog = false;
	private string showGUIErrorDialogContent = "";
	private Rect showGUIErrorDialogWinRect = new Rect(0.0f, 0.0f, 320.0f, 240.0f);

	private bool showGUIDebug = false;
    private bool showGUIDebugInfo = true;
    private bool showGUIDebugLogConsole = false;
	
    void OnGUI()
    {
        if (!guiSetup) SetupGUI();

		if (showGUIErrorDialog) {
			showGUIErrorDialogWinRect = GUILayout.Window(0, showGUIErrorDialogWinRect, DrawErrorDialog, "Error");
			showGUIErrorDialogWinRect.x = ((float)Screen.width - showGUIErrorDialogWinRect.width) * 0.5f;
			showGUIErrorDialogWinRect.y = ((float)Screen.height - showGUIErrorDialogWinRect.height) * 0.5f;
			GUILayout.Window(0, showGUIErrorDialogWinRect, DrawErrorDialog, "Error");	
		}
		
        if (showGUIDebug) {
            if (GUI.Button(new Rect(550, 250, 150, 50), "Info")) showGUIDebugInfo = !showGUIDebugInfo;
            if (showGUIDebugInfo) DrawInfoGUI();

            if (GUI.Button(new Rect(550, 320, 150, 50), "Log")) showGUIDebugLogConsole = !showGUIDebugLogConsole;
            if (showGUIDebugLogConsole) DrawLogConsole();

			if (GUI.Button(new Rect(550, 390, 150, 50), "Content mode: " + ContentModeNames[ContentMode])) CycleContentMode();
#if UNITY_ANDROID
			if (Application.platform == RuntimePlatform.Android) {
				if (GUI.Button(new Rect(400, 250, 150, 50), "Camera preferences")) {
					using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
						AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
						jo.Call("launchPreferencesActivity");
					}
				}
			}
#endif
			if (GUI.Button(new Rect(400, 390, 150, 50), "Video background: " + UseVideoBackground)) UseVideoBackground = !UseVideoBackground;

            GUI.Label(new Rect(700, 20, 100, 25), "FPS: " + lastFramerate);
        }
    }
	
	
	private void DrawErrorDialog(int winID)
	{
		GUILayout.BeginVertical();
		GUILayout.Label(showGUIErrorDialogContent);
	   	GUILayout.FlexibleSpace();
		if (GUILayout.Button("OK")) showGUIErrorDialog = false;
		GUILayout.EndVertical();
	}
	
    private void DrawInfoGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 25), "ARToolKit " + Version);
        GUI.Label(new Rect(10, 30, 500, 25), "Video " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + ")");

        GUI.Label(new Rect(10, 90, 500, 25), "Graphics device: " + SystemInfo.graphicsDeviceName);
        GUI.Label(new Rect(10, 110, 500, 25), "Operating system: " + SystemInfo.operatingSystem);
        GUI.Label(new Rect(10, 130, 500, 25), "Processors: (" + SystemInfo.processorCount + "x) " + SystemInfo.processorType);
        GUI.Label(new Rect(10, 150, 500, 25), "Memory: " + SystemInfo.systemMemorySize + "MB");

        GUI.Label(new Rect(10, 170, 500, 25), "Resolution : " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" + Screen.currentResolution.refreshRate + "Hz");
        GUI.Label(new Rect(10, 190, 500, 25), "Screen : " + Screen.width + "x" + Screen.height);
        GUI.Label(new Rect(10, 210, 500, 25), "Viewport : " + _videoBackgroundCamera0.pixelRect.xMin + "," + _videoBackgroundCamera0.pixelRect.yMin + ", " + _videoBackgroundCamera0.pixelRect.xMax + ", " + _videoBackgroundCamera0.pixelRect.yMax);
        int y = 350;

		ARMarker[] markers = Component.FindObjectsOfType(typeof(ARMarker)) as ARMarker[];
		foreach (ARMarker m in markers) {
            GUI.Label(new Rect(10, y, 500, 25), "Marker: " + m.UID + ", " + m.Visible);
            y += 25;
        }
    }


    public Vector2 scrollPosition = Vector2.zero;

    private void DrawLogConsole()
    {
        Rect consoleRect = new Rect(0, 0, Screen.width, 200);

        GUIStyle s = new GUIStyle(GUI.skin.box);
        s.normal.background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        s.normal.background.SetPixel(0, 0, new Color(1, 1, 1, 1));
        s.normal.background.Apply();

        GUI.Box(consoleRect, "", s);

        DrawLogEntries(consoleRect, false);
    }


    private void DrawLogEntries(Rect container, bool reverse)
    {
        Rect scrollViewRect = new Rect(5, 5, container.width - 10, container.height - 10);

        float height = 0;
        float width = scrollViewRect.width - 30;

        foreach (String s in logMessages) {
            float h = GUI.skin.label.CalcHeight(new GUIContent(s), width);
            height += h;
        }

        Rect contentRect = new Rect(0, 0, width, height);

        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);

        float y = 0;

        IEnumerable<string> lm = logMessages;
        if (reverse) lm = lm.Reverse<string>();

        int i = 0;

        foreach (String s in lm) {
            i = 0;
            if (s.StartsWith(LogTag)) i = 1;
            else if (s.StartsWith("ARController C++:")) i = 2;

            float h = GUI.skin.label.CalcHeight(new GUIContent(s), width);
            GUI.Label(new Rect(0, y, width, h), s, style[i]);

            y += h;
        }

        GUI.EndScrollView();
    }

}

