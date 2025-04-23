using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace FinalClick.AddressableResources.Editor
{
    public static class AddressableResourcesEditorAPI
    {
        private const string AddressableResourcesFolderName = "AddressableResources";
        
        public static AddressableResourcesCollection CreateProjectAddressableResourcesCollection()
        {
            var folders = GetAllAddressableResourcesFolders();
            var allAddressableReferencesUnderFolders = GetAssetReferencesUnderFolders(folders);

            var collection = new AddressableResourcesCollection();
            
            foreach (var assetReference in allAddressableReferencesUnderFolders)
            {
                // E.g. Assets/Something/AddressableResources/Textures/MyTexture.png -> Textures/MyTexture
                var path = GetPathToAsset(assetReference);
                var pathUnderAddressableResourcesFolder = GetAddressableResourcesPathToAsset(assetReference);
                collection.AddUnique(pathUnderAddressableResourcesFolder, assetReference);
            }

            return collection;
        }

        private static string GetAddressableResourcesPathToAsset(AssetReference assetReference)
        {
            var fullPath = UnityEditor.AssetDatabase.GetAssetPath(assetReference.editorAsset);
            var normalizedFullPath = fullPath.Replace("\\", "/");
            var addressableFolderPath = "/" + AddressableResourcesFolderName + "/";

            var index = normalizedFullPath.IndexOf(addressableFolderPath, StringComparison.Ordinal);
            if (index >= 0)
            {
                var relativePath = normalizedFullPath[(index + addressableFolderPath.Length)..];
                var withoutExtension = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(relativePath) ?? string.Empty,
                    System.IO.Path.GetFileNameWithoutExtension(relativePath));
                return withoutExtension.Replace("\\", "/");
            }
            return string.Empty;
        }

        private static string GetPathToAsset(AssetReference assetReference)
        {
            return UnityEditor.AssetDatabase.GetAssetPath(assetReference.editorAsset);
        }

        private static IReadOnlyCollection<AssetReference> GetAssetReferencesUnderFolders(IReadOnlyCollection<string> folders)
        {
            var assetReferences = new List<AssetReference>();
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return assetReferences;

            foreach (var folder in folders)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("", new[] { folder });
                foreach (var guid in guids)
                {
                    var entry = settings.FindAssetEntry(guid);
                    if (entry != null)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        if (asset != null)
                        {
                            var reference = new AssetReference(guid);
                            reference.SetEditorAsset(asset); // Needed for editor-time access
                            assetReferences.Add(reference);
                        }
                    }
                }
            }

            return assetReferences;
        }

        private static IReadOnlyCollection<string> GetAllAddressableResourcesFolders()
        {
            var folders = new List<string>();
            var allGuids = UnityEditor.AssetDatabase.FindAssets("t:DefaultAsset", new[] { "Assets" });

            foreach (var guid in allGuids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Directory.Exists(path) && System.IO.Path.GetFileName(path) == AddressableResourcesFolderName)
                {
                    folders.Add(path);
                }
            }

            return folders.AsReadOnly();
        }
        
        [InitializeOnLoadMethod]
        private static void InitializeAPIForEditorUsage()
        {
            AddressableResourcesAPI.Initialize(CreateProjectAddressableResourcesCollection());
        }
    }
}