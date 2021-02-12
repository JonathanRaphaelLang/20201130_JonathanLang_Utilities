#if UNITY_EDITOR
namespace Ganymed.Utils.Helper
{
    public class AssetImportHelper : UnityEditor.AssetPostprocessor
    {
        public static bool IsImporting => isImporting;
        private static volatile bool isImporting = false;

        private void OnPreprocessAsset()
        {
            isImporting = true;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            isImporting = false;
        }
    }
}
#endif
