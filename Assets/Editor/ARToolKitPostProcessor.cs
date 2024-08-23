using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class ARToolKitPostProcessor {
#if !UNITY_IPHONE
    [PostProcessBuild(1500)]
    public static void OnPostProcessBuild(BuildTarget target, string path) { }
#else
    private class IosFramework {
        public  string Name, Id, RefId, LastKnownFileType, FormattedName, Path, SourceTree;

        public IosFramework(string name, string id, string refId, string lastKnownFileType, string formattedName, string path, string sourceTree) {
            Name              = name;
            Id                = id;
            RefId             = refId;
            LastKnownFileType = lastKnownFileType;
            Path              = path;
            SourceTree        = sourceTree;
            FormattedName     = formattedName;
        }
    }

    private delegate void ProcessTask(ref string source);

    private const string LOGFILE_NAME                          = "postprocess.log";
    private const string PBXJPROJ_FILE_PATH                    = "Unity-iPhone.xcodeproj/project.pbxproj";

    private const string PBXBUILDFILE_SECTION_END              = "/* End PBXBuildFile section */";
    private const string PBXBUILDFILE_STRING_FORMAT            = "\t\t{0} /* {1} in Frameworks */ = {{isa = PBXBuildFile; fileRef = {2} /* {1} */; }};\n";

    private const string PBXFILEREFERENCE_SECTION_END          = "/* End PBXFileReference section */";
    private const string PBXFILEREFERENCE_STRING_FORMAT        = "\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = {2}; name = {3}; path = {4}; sourceTree = {5}; }};\n";
    
    private const string PBXFRAMEWORKSBUILDPHASE_SECTION_BEGIN = "/* Begin PBXFrameworksBuildPhase section */";
    private const string PBXFRAMEWORKSBUILDPHASE_SUBSET        = "files = (";
    private const string PBXFRAMEWORKSBUILDPHASE_STRING_FORMAT = "\n\t\t\t\t{0} /* {1} in Frameworks */,";

    private const string PBXGROUP_SECTION_BEGIN                = "/* Begin PBXGroup section */";
    private const string PBXGROUP_SUBSET_1                     = "/* Frameworks */ = {";
    private const string PBXGROUP_SUBSET_2                     = "children = (";
    private const string PBXGROUP_STRING_FORMAT                = "\n\t\t\t\t{0} /* {1} */,";

    private static IosFramework[] iosFrameworks = {
        new IosFramework("libstdc++.6.dylib",    "E0005ED91B047A0C00FEB577", "E0005ED81B047A0C00FEB577", "\"compiled.mach-o.dylib\"",
                         "\"libstdc++.6.dylib\"", "\"usr/lib/libstdc++.6.dylib\"",                  "SDKROOT"),
        new IosFramework("Accelerate.framework", "E0005ED51B04798800FEB577", "E0005ED41B04798800FEB577", "wrapper.framework",
                         "Accelerate.framework",  "System/Library/Frameworks/Accelerate.framework", "SDKROOT")
    };

    private static StreamWriter streamWriter = null;

    [PostProcessBuild(int.MaxValue)]
    public static void OnPostProcessBuild(BuildTarget target, string path) {
        string logPath = Path.Combine(path, LOGFILE_NAME);
        if (target != BuildTarget.iPhone) {
            Debug.LogError("ARToolKitPostProcessor::OnIosPostProcess - Called on non iOS build target!");
            return;
        } else if (File.Exists(logPath)) {
			streamWriter = new StreamWriter(logPath, true);
			streamWriter.WriteLine("OnIosPostProcess - Beginning iOS post-processing.");
			streamWriter.WriteLine("OnIosPostProcess - WARNING - Attempting to process directory that has already been processed. Skipping.");
			streamWriter.WriteLine("OnIosPostProcess - Aborted iOS post-processing.");
			streamWriter.Close();
			streamWriter = null;
        } else {
            streamWriter = new StreamWriter(logPath);
            streamWriter.WriteLine("OnIosPostProcess - Beginning iOS post-processing.");

            try {
                string pbxprojPath = Path.Combine(path, PBXJPROJ_FILE_PATH);
                if (File.Exists(pbxprojPath)) {
                    string pbxproj = File.ReadAllText(pbxprojPath);
                      
                    streamWriter.WriteLine("OnIosPostProcess - Modifying file at " + pbxprojPath);
                      
                    string pbxBuildFile            = string.Empty;
                    string pbxFileReference        = string.Empty;
                    string pbxFrameworksBuildPhase = string.Empty;
                    string pbxGroup                = string.Empty;
                    for (int i = 0; i < iosFrameworks.Length; ++i) {
                        if (pbxproj.Contains(iosFrameworks[i].Path)) {
                            streamWriter.WriteLine("OnIosPostProcess - Project already contains reference to " + iosFrameworks[i].Name + " - skipping.");
                            continue;
                        }
                        pbxBuildFile            += string.Format(PBXBUILDFILE_STRING_FORMAT,            new object[] { iosFrameworks[i].Id,            iosFrameworks[i].Name, iosFrameworks[i].RefId });
                        pbxFileReference        += string.Format(PBXFILEREFERENCE_STRING_FORMAT,        new object[] { iosFrameworks[i].RefId,         iosFrameworks[i].Name, iosFrameworks[i].LastKnownFileType,
                                                                                                                       iosFrameworks[i].FormattedName, iosFrameworks[i].Path, iosFrameworks[i].SourceTree });
                        pbxFrameworksBuildPhase += string.Format(PBXFRAMEWORKSBUILDPHASE_STRING_FORMAT, new object[] { iosFrameworks[i].Id,            iosFrameworks[i].Name });
                        pbxGroup                += string.Format(PBXGROUP_STRING_FORMAT,                new object[] { iosFrameworks[i].RefId,         iosFrameworks[i].Name });
                        streamWriter.WriteLine("OnPostProcessBuild - Processed " + iosFrameworks[i].Name);
                    }

                    int index = pbxproj.IndexOf(PBXBUILDFILE_SECTION_END);
                    pbxproj = pbxproj.Insert(index, pbxBuildFile);
                    streamWriter.WriteLine("OnPostProcessBuild - Injected PBXBUILDFILE");

                    index = pbxproj.IndexOf(PBXFILEREFERENCE_SECTION_END);
                    pbxproj = pbxproj.Insert(index, pbxFileReference);
                    streamWriter.WriteLine("OnPostProcessBuild - Injected PBXFILEREFERENCE");

                    index = pbxproj.IndexOf(PBXFRAMEWORKSBUILDPHASE_SECTION_BEGIN);
                    index = pbxproj.IndexOf(PBXFRAMEWORKSBUILDPHASE_SUBSET, index) + PBXFRAMEWORKSBUILDPHASE_SUBSET.Length;
                    pbxproj = pbxproj.Insert(index, pbxFrameworksBuildPhase);
                    streamWriter.WriteLine("OnPostProcessBuild - Injected PBXFRAMEWORKSBUILDPHASE");

                    index = pbxproj.IndexOf(PBXGROUP_SECTION_BEGIN);
                    index = pbxproj.IndexOf(PBXGROUP_SUBSET_1, index);
                    index = pbxproj.IndexOf(PBXGROUP_SUBSET_2, index) + PBXGROUP_SUBSET_2.Length;
                    pbxproj = pbxproj.Insert(index, pbxGroup);
                    streamWriter.WriteLine("OnPostProcessBuild - Injected PBXGROUP");

                    File.Delete(pbxprojPath);
                    File.WriteAllText(pbxprojPath, pbxproj);

                    streamWriter.WriteLine("OnIosPostProcess - Ending iOS post-processing successfully.");
                } else {
                    streamWriter.WriteLine("OnIosPostProcess - ERROR - File " + pbxprojPath + " does not exist!");
                }
            } catch (System.Exception e) {
                streamWriter.WriteLine("ProcessSection - ERROR - " + e.Message + " : " + e.StackTrace);

            } finally {
                streamWriter.Close();
                streamWriter = null;
            }
        }
    }

#endif
}
