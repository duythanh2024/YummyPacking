#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class XcodePostProcess
{
     const string k_TrackingDescription = "Your data will be used to provide you a better and personalized ad experience.";

    [PostProcessBuild(100)] // Chạy sau cùng để ghi đè các thiết lập khác
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToXcode) {
        if (buildTarget == BuildTarget.iOS) {
            string plistPath = pathToXcode + "/Info.plist";
            PlistDocument plistObj = new PlistDocument();
            plistObj.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict plistRoot = plistObj.root;

            // 1. Ép mô tả tiếng Anh
            plistRoot.SetString("NSUserTrackingUsageDescription", k_TrackingDescription);

            // 2. ÉP CỨNG VÙNG PHÁT TRIỂN: Buộc các nút hệ thống hiện tiếng Anh
            plistRoot.SetString("CFBundleDevelopmentRegion", "en");

            // 3. XÓA DANH SÁCH ĐA NGÔN NGỮ: Chỉ cho phép tiếng Anh
            PlistElementArray localizations = plistRoot.CreateArray("CFBundleLocalizations");
            localizations.AddString("en");

            File.WriteAllText(plistPath, plistObj.WriteToString());
           // UnityEngine.Debug.Log("Faze Games: ATT và Ngôn ngữ hệ thống đã được khóa sang English!");
        }
    }
}
#endif