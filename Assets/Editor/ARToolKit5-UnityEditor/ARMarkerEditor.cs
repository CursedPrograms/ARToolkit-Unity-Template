using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARMarker))]
public class ARMarkerEditor : Editor
{
    public bool showFilterOptions = false;

	private static TextAsset[] PatternAssets;
	private static int PatternAssetCount;
	private static string[] PatternFilenames;
	
	void OnDestroy()
	{
		PatternAssets = null;
		PatternAssetCount = 0;
		PatternFilenames = null;
	}
	
	private static void RefreshPatternFilenames() 
	{
		PatternAssets = Resources.LoadAll("ardata/markers", typeof(TextAsset)).Cast<TextAsset>().ToArray();
		PatternAssetCount = PatternAssets.Length;
		
		PatternFilenames = new string[PatternAssetCount];
		for (int i = 0; i < PatternAssetCount; i++) {					
			PatternFilenames[i] = PatternAssets[i].name;				
		}
	}
	
    public override void OnInspectorGUI()
    {
   
        EditorGUILayout.BeginVertical();
		
        ARMarker m = (ARMarker)target;
        if (m == null) return;
		
		if (m.UID == ARMarker.NO_ID) m.Load(); 
		
        m.Tag = EditorGUILayout.TextField("Tag", m.Tag);
        EditorGUILayout.LabelField("UID", (m.UID == ARMarker.NO_ID ? "Not loaded": m.UID.ToString()));
		
        EditorGUILayout.Separator();
		
        MarkerType t = (MarkerType)EditorGUILayout.EnumPopup("Type", m.MarkerType);
        if (m.MarkerType != t) {    
			m.Unload();
			m.MarkerType = t;
			m.Load();
		}
		
        EditorGUILayout.LabelField("Description", ARMarker.MarkerTypeNames[m.MarkerType]);
		
        switch (m.MarkerType) {
			
			case MarkerType.Square:	
        	case MarkerType.SquareBarcode:
			
				if (m.MarkerType == MarkerType.Square) {
				
					RefreshPatternFilenames();           
					if (PatternFilenames.Length > 0) {
						int patternFilenameIndex = EditorGUILayout.Popup("Pattern file", m.PatternFilenameIndex, PatternFilenames);
						string patternFilename = PatternAssets[patternFilenameIndex].name;
						if (patternFilename != m.PatternFilename) {
							m.Unload();
							m.PatternFilenameIndex = patternFilenameIndex;
							m.PatternFilename = patternFilename;
							m.PatternContents = PatternAssets[m.PatternFilenameIndex].text;
							m.Load();
						}
					} else {
						m.PatternFilenameIndex = 0;
						EditorGUILayout.LabelField("Pattern file", "No patterns available");
						m.PatternFilename = "";
						m.PatternContents = "";
					}
				
				} else {
				
					int BarcodeID = EditorGUILayout.IntField("Barcode ID", m.BarcodeID);
	 				if (BarcodeID != m.BarcodeID) {
						m.Unload();
						m.BarcodeID = BarcodeID;
						m.Load();
					}
				
				}
			
				float patternWidthPrev = m.PatternWidth;
				m.PatternWidth = EditorGUILayout.FloatField("Width", m.PatternWidth);
				if (patternWidthPrev != m.PatternWidth) {
					m.Unload();
					m.Load();
				}
				m.UseContPoseEstimation = EditorGUILayout.Toggle("Cont. pose estimation", m.UseContPoseEstimation);
			
				break;
			
        	case MarkerType.Multimarker:
				string MultiConfigFile = EditorGUILayout.TextField("Multimarker config.", m.MultiConfigFile);
        	    if (MultiConfigFile != m.MultiConfigFile) {
					m.Unload();
					m.MultiConfigFile = MultiConfigFile;
					m.Load();
				}
        	    break;

			case MarkerType.NFT:
                string NFTDataSetName = EditorGUILayout.TextField("NFT dataset name", m.NFTDataName);
				if (NFTDataSetName != m.NFTDataName) {
					m.Unload();
					m.NFTDataName = NFTDataSetName;
					m.Load();
				}
				float nftScalePrev = m.NFTScale;
				m.NFTScale = EditorGUILayout.FloatField("NFT marker scalefactor", m.NFTScale);
				if (nftScalePrev != m.NFTScale) {
					EditorUtility.SetDirty(m);
				}
				break;
        }
		
        EditorGUILayout.Separator();
		
        showFilterOptions = EditorGUILayout.Foldout(showFilterOptions, "Filter Options");
        if (showFilterOptions) {
			m.Filtered = EditorGUILayout.Toggle("Filtered:", m.Filtered);
			m.FilterSampleRate = EditorGUILayout.Slider("Sample rate:", m.FilterSampleRate, 1.0f, 30.0f);
			m.FilterCutoffFreq = EditorGUILayout.Slider("Cutoff freq.:", m.FilterCutoffFreq, 1.0f, 30.0f);
		}

        EditorGUILayout.BeginHorizontal();

        if (m.Patterns != null) {
            for (int i = 0; i < m.Patterns.Length; i++) {
                GUILayout.Label(new GUIContent("Pattern " + i + ", " + m.Patterns[i].width.ToString("n3") + " m", m.Patterns[i].texture), GUILayout.ExpandWidth(false));      
            }
        }
		
        EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

    }

}