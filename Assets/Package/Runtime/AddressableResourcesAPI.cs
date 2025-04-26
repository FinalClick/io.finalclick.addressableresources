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
            private static Dictionary<UnityEngine.Object, int> _loadedRefCount = new Dictionary<UnityEngine.Object, int>();
            
            public AddressableResourcesInternalAPI(AddressableResourcesCollection collection)
            {
                _assetReferences = collection.GetAsDictionary();
            }

            protected override void UnloadAsset(Object assetToUnload)
            {
                if (IsLoadedByAddressableResources(assetToUnload) == false)
                {
                    base.UnloadAsset(assetToUnload);
                    return;
                }
                
                if (DecreaseReferenceToObject(assetToUnload) == true)
                {
                    Addressables.Release(assetToUnload);
                }
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
                    
                    var loadedObject = LoadAssetAsType(systemTypeInstance, assetReference);
                    if (loadedObject != null)
                    {
                        IncreaseReferenceToObject(loadedObject);
                    }
                }
                
                return base.Load(path, systemTypeInstance);
            }

            private void IncreaseReferenceToObject(Object loadedObject)
            {
                if (_loadedRefCount.TryGetValue(loadedObject, out var count) == false)
                {
                    _loadedRefCount.Add(loadedObject, 1);
                    return;
                }
                
                _loadedRefCount[loadedObject] = count + 1;
            }

            private bool IsLoadedByAddressableResources(Object loadedObject)
            {
                return _loadedRefCount.ContainsKey(loadedObject);
            }
            
            /// <summary>
            /// Decreases the reference count for a given object and performs cleanup if the reference count reaches zero.
            /// </summary>
            /// <param name="loadedObject">The object for which the reference count should be decreased.</param>
            /// <returns>
            /// Returns true if the reference count of the object reaches zero and it should be unloaded; otherwise, false.
            /// </returns>
            /// <exception cref="ArgumentException">Thrown if the object is not found in the reference count dictionary.</exception>
            private bool DecreaseReferenceToObject(Object loadedObject)
            {
                if (_loadedRefCount.TryGetValue(loadedObject, out var count) == false)
                {
                    throw new ArgumentException("Object not found in reference count");
                }

                if (count == 1)
                {
                    _loadedRefCount.Remove(loadedObject);
                    return true;
                }

                _loadedRefCount[loadedObject] = count - 1;
                return false;
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