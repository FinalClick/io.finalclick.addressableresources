using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace FinalClick.AddressableResources
{
    public static class AddressableResourcesAPI
    {
        private class AddressableResourcesInternalAPI : ResourcesAPI
        {
            private static Dictionary<string, AssetReference> _assetReferences;
            
            public AddressableResourcesInternalAPI(AddressableResourcesCollection collection)
            {
                _assetReferences = collection.GetAsDictionary();
            }

            protected override Object Load(string path, Type systemTypeInstance)
            {
                if (_assetReferences.TryGetValue(AddressableResourcesAPI.NormalizeKey(path), out var assetReference) == true)
                {
                    Debug.Log($"Loading resource: {path} with addressables.");
#if UNITY_EDITOR
                    if (Application.isPlaying == false)
                    {
                        return assetReference.editorAsset;
                    }
#endif
                    return LoadAssetAsType(systemTypeInstance, assetReference);
                }
                
                return base.Load(path, systemTypeInstance);
            }

            private static Object LoadAssetAsType(Type systemTypeInstance, AssetReference assetReference)
            {
                Object result;
                switch (systemTypeInstance)
                {
                    case Type t when t == typeof(GameObject):
                        result = assetReference.LoadAssetAsync<GameObject>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(Texture2D):
                        result = assetReference.LoadAssetAsync<Texture2D>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(Sprite):
                        result = assetReference.LoadAssetAsync<Sprite>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(Material):
                        result = assetReference.LoadAssetAsync<Material>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(AudioClip):
                        result = assetReference.LoadAssetAsync<AudioClip>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(TMPro.TMP_FontAsset):
                        result = assetReference.LoadAssetAsync<TMPro.TMP_FontAsset>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(TMPro.TMP_SpriteAsset):
                        result = assetReference.LoadAssetAsync<TMPro.TMP_SpriteAsset>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(TMPro.TMP_StyleSheet):
                        result = assetReference.LoadAssetAsync<TMPro.TMP_StyleSheet>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(TMPro.TMP_Settings):
                        result = assetReference.LoadAssetAsync<TMPro.TMP_Settings>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(TMPro.TextMeshProUGUI):
                        result = assetReference.LoadAssetAsync<TMPro.TextMeshProUGUI>().WaitForCompletion();
                        return result;
                    case Type t when t == typeof(TMPro.TextMeshPro):
                        result = assetReference.LoadAssetAsync<TMPro.TextMeshPro>().WaitForCompletion();
                        return result;
                    default:
                        var method = typeof(AssetReference)
                            .GetMethod("LoadAssetAsync", Type.EmptyTypes)?
                            .MakeGenericMethod(systemTypeInstance ?? typeof(Object));
                        var handle = method?.Invoke(assetReference, null);
                        var resultProperty = handle?.GetType().GetProperty("Result");
                        result = resultProperty?.GetValue(handle) as Object;
                        return result;
                }
            }
        }

        public static string NormalizeKey(string key)
        {
            return key.ToLower();
        }

        public static bool IsInitialized()
        {
            return ResourcesAPI.overrideAPI is AddressableResourcesInternalAPI;
        }
        
        
        public static void Initialize(AddressableResourcesCollection collection)
        {
            if (IsInitialized() == true)
            {
                return;
            }
            
            Debug.Log("Initializing Addressable Resources API");
            ResourcesAPI.overrideAPI = new AddressableResourcesInternalAPI(collection);
        }
    }

}