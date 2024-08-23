using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum MarkerType
{
    Square,      		      
    SquareBarcode,            
    Multimarker,               
	NFT
}

public enum ARWMarkerOption : int {
        ARW_MARKER_OPTION_FILTERED = 1,
        ARW_MARKER_OPTION_FILTER_SAMPLE_RATE = 2,
        ARW_MARKER_OPTION_FILTER_CUTOFF_FREQ = 3,
        ARW_MARKER_OPTION_SQUARE_USE_CONT_POSE_ESTIMATION = 4,
		ARW_MARKER_OPTION_SQUARE_CONFIDENCE = 5,
		ARW_MARKER_OPTION_SQUARE_CONFIDENCE_CUTOFF = 6,
		ARW_MARKER_OPTION_NFT_SCALE = 7
}

[ExecuteInEditMode]
public class ARMarker : MonoBehaviour
{

    public readonly static Dictionary<MarkerType, string> MarkerTypeNames = new Dictionary<MarkerType, string>
    {
		{MarkerType.Square, "Single AR pattern"},
		{MarkerType.SquareBarcode, "Single AR barcode"},
    	{MarkerType.Multimarker, "Multimarker AR configuration"},
		{MarkerType.NFT, "NFT dataset"}
    };

    private const string LogTag = "ARMarker: ";
    
    public const int NO_ID = -1;

    [NonSerialized]                     
    public int UID = NO_ID;              

    public MarkerType MarkerType = MarkerType.Square;
    public string Tag = "";

	public int PatternFilenameIndex = 0;
    public string PatternFilename = "";
	public string PatternContents = "";     
    public float PatternWidth = 0.08f;
	
	public int BarcodeID = 0;
	
    public string MultiConfigFile = "";
	
	public string NFTDataName = "";
	#if !UNITY_METRO
	private readonly string[] NFTDataExts = {"iset", "fset", "fset2"};
	#endif
	[NonSerialized]
	public float NFTWidth;               
	[NonSerialized]
	public float NFTHeight;               

	private ARPattern[] patterns;

	[SerializeField]
	private bool currentUseContPoseEstimation = false;						          
	[SerializeField]
	private bool currentFiltered = false;
	[SerializeField]
	private float currentFilterSampleRate = 30.0f;
	[SerializeField]
	private float currentFilterCutoffFreq = 15.0f;
	[SerializeField]
	private float currentNFTScale = 1.0f;									         

    private bool visible = false;                                                
	private Matrix4x4 transformationMatrix;                                        

    void Awake()
    {
        UID = NO_ID;
    }
	
	public void OnEnable()
	{
		Load();
	}
	
	public void OnDisable()
	{
		Unload();
	}

	#if !UNITY_METRO
	private bool unpackStreamingAssetToCacheDir(string basename)
	{
		if (!File.Exists(System.IO.Path.Combine(Application.temporaryCachePath, basename))) {
			string file = System.IO.Path.Combine(Application.streamingAssetsPath, basename);         
			WWW unpackerWWW = new WWW(file);
			while (!unpackerWWW.isDone) { }           
			if (!string.IsNullOrEmpty(unpackerWWW.error)) {
				ARController.Log(LogTag + "Error unpacking '" + file + "'");
				return (false);
			}
			File.WriteAllBytes(System.IO.Path.Combine(Application.temporaryCachePath, basename), unpackerWWW.bytes);     
		}
		return (true);
	}
	#endif

    public void Load() 
    {
        if (UID != NO_ID) {
			return;
		}

		if (!PluginFunctions.inited) {
			return;
		}

        string dir = Application.streamingAssetsPath;
        string cfg = "";
		
        switch (MarkerType) {

			case MarkerType.Square:
				cfg = "single_buffer;" + PatternWidth*1000.0f + ";buffer=" + PatternContents;
				break;
			
			case MarkerType.SquareBarcode:
				cfg = "single_barcode;" + BarcodeID + ";" + PatternWidth*1000.0f;
				break;
			
            case MarkerType.Multimarker:
				#if !UNITY_METRO
				if (dir.Contains("://")) {
					dir = Application.temporaryCachePath;
					if (!unpackStreamingAssetToCacheDir(MultiConfigFile)) {
						dir = "";
					} else {
					
				    }
				}
				#endif
				
				if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(MultiConfigFile)) {
					cfg = "multi;" + System.IO.Path.Combine(dir, MultiConfigFile);
				}
                break;

			
			case MarkerType.NFT:
				#if !UNITY_METRO
				if (dir.Contains("://")) {
					dir = Application.temporaryCachePath;
					foreach (string ext in NFTDataExts) {
						string basename = NFTDataName + "." + ext;
						if (!unpackStreamingAssetToCacheDir(basename)) {
							dir = "";
							break;
						}
					}
				}
				#endif
			
				if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(NFTDataName)) {
					cfg = "nft;" + System.IO.Path.Combine(dir, NFTDataName);
				}
				break;

            default:
                break;

        }
		
		if (!string.IsNullOrEmpty(cfg)) {
        	UID = PluginFunctions.arwAddMarker(cfg);
			if (UID == NO_ID) {
				ARController.Log(LogTag + "Error loading marker.");
			} else {

				if (MarkerType == MarkerType.Square || MarkerType == MarkerType.SquareBarcode) UseContPoseEstimation = currentUseContPoseEstimation;
				Filtered = currentFiltered;
				FilterSampleRate = currentFilterSampleRate;
				FilterCutoffFreq = currentFilterCutoffFreq;
				if (MarkerType == MarkerType.NFT) NFTScale = currentNFTScale;

				if (MarkerType == MarkerType.NFT) {

					int imageSizeX, imageSizeY;
					PluginFunctions.arwGetMarkerPatternConfig(UID, 0, null, out NFTWidth, out NFTHeight, out imageSizeX, out imageSizeY);
					NFTWidth *= 0.001f;
					NFTHeight *= 0.001f;
				} else {

					int numPatterns = PluginFunctions.arwGetMarkerPatternCount(UID);
        			if (numPatterns > 0) {
						patterns = new ARPattern[numPatterns];
			        	for (int i = 0; i < numPatterns; i++) {
            				patterns[i] = new ARPattern(UID, i);
        				}
					}

				}
			}
		}
    }

    void Update()
    {
		float[] matrixRawArray = new float[16];

		if (UID == NO_ID || !PluginFunctions.inited) {
            visible = false;
            return;
        }

        if (Application.isPlaying) {

			visible = PluginFunctions.arwQueryMarkerTransformation(UID, matrixRawArray);
            if (visible) {
				matrixRawArray[12] *= 0.001f;            
				matrixRawArray[13] *= 0.001f;
				matrixRawArray[14] *= 0.001f;

				Matrix4x4 matrixRaw = ARUtilityFunctions.MatrixFromFloatArray(matrixRawArray);
				transformationMatrix = ARUtilityFunctions.LHMatrixFromRHMatrix(matrixRaw);
			}
		}
    }
	
	public void Unload()
	{
		if (UID == NO_ID) {
			return;
		}
		
		if (PluginFunctions.inited) {
        	PluginFunctions.arwRemoveMarker(UID);
		}

		UID = NO_ID;

		patterns = null;     
	}
	
    public Matrix4x4 TransformationMatrix
    {
        get
        {                
            return transformationMatrix;
        }
    }

    public bool Visible
    {
        get
        {
            return visible;
        }
    }


    public ARPattern[] Patterns
    {
        get
        {
            return patterns;
        }
    }

    public bool Filtered
    {
        get
        {
			return currentFiltered;  
        }

        set
        {
			currentFiltered = value;
			if (UID != NO_ID) {
				PluginFunctions.arwSetMarkerOptionBool(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_FILTERED, value);
			}
        }
    }

    public float FilterSampleRate
    {
        get
        {
			return currentFilterSampleRate;  
        }

        set
        {
			currentFilterSampleRate = value;
			if (UID != NO_ID) {
				PluginFunctions.arwSetMarkerOptionFloat(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_FILTER_SAMPLE_RATE, value);
			}
        }
    }

	public float FilterCutoffFreq
    {
        get
        {
			return currentFilterCutoffFreq;  
        }

        set
        {
			currentFilterCutoffFreq = value;
			if (UID != NO_ID) {
				PluginFunctions.arwSetMarkerOptionFloat(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_FILTER_CUTOFF_FREQ, value);
			}
        }
    }

	public bool UseContPoseEstimation
    {
        get
        {
			return currentUseContPoseEstimation;  
        }

        set
        {
			currentUseContPoseEstimation = value;
			if (UID != NO_ID && (MarkerType == MarkerType.Square || MarkerType == MarkerType.SquareBarcode)) {
				PluginFunctions.arwSetMarkerOptionBool(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_SQUARE_USE_CONT_POSE_ESTIMATION, value);
			}
        }
    }

	public float NFTScale
	{
		get
		{
			return currentNFTScale;  
		}
		
		set
		{
			currentNFTScale = value;
			if (UID != NO_ID && (MarkerType == MarkerType.NFT)) {
				PluginFunctions.arwSetMarkerOptionFloat(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_NFT_SCALE, value);
			}
		}
	}
	

}
