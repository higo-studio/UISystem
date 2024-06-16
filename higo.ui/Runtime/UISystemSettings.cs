using Higo.UI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Higo.UI
{
    [CreateAssetMenu]
    public class UISystemSettings : ScriptableObject
    {
        public const string SettingPath = "Assets/Settings/Resources";
        public readonly static string Path = $"{SettingPath}/{nameof(UISystemSettings)}.asset";
        public UISystem Prefab;
#if UNITY_EDITOR
        public void Reset()
        {
            Prefab = AssetDatabase.LoadAssetAtPath<UISystem>("Packages/higo.ui/Resources/UI System.prefab");
        }

        [MenuItem("Higo/UI/Create UISystemSettings")]
        public static void CreateAsset()
        {
            Debug.Log(AssetDatabase.AssetPathToGUID(Path));
            if(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(Path, AssetPathToGUIDOptions.OnlyExistingAssets)))
            {
                Directory.CreateDirectory(SettingPath);
                AssetDatabase.Refresh();
                var asset = ScriptableObject.CreateInstance<UISystemSettings>();
                AssetDatabase.CreateAsset(asset, Path);
            }
            else
            {
                Debug.LogError($"UISystemSettings is exsited in {Path}");
            }
        }

        [MenuItem("Higo/UI/Create UI System Template")]
        public static void CreateUISystemTemplate()
        {
            var path = $"{SettingPath}/UI System.prefab";

            Directory.CreateDirectory(SettingPath);
            AssetDatabase.Refresh();
            AssetDatabase.CopyAsset("Packages/higo.ui/Resources/UI System.prefab", path);

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);

        }

#endif
    }
}