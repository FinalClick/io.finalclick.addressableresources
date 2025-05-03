using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace FinalClick.AddressableResources
{
    public static class AddressableResourcesAPI
    {
        private class AddressableResourcesInternalAPI : ResourcesAPI
        {
            private static Dictionary<string, AssetReference> _assetReferences;
            private static Dictionary<UnityEngine.Object, int> _loadedRefCount = new Dictionary<UnityEngine.Object, int>();
            private static Dictionary<UnityEngine.Object, AsyncOperationHandle> _loadingHandles = new Dictionary<UnityEngine.Object, AsyncOperationHandle>();
            
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
                    if (_loadingHandles.TryGetValue(assetToUnload, out var handle) == true)
                    {
                        Addressables.Release(handle);
                    }
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
                if (assetReference.OperationHandle.IsValid() == true)
                {
                    assetReference.OperationHandle.WaitForCompletion();
                    return assetReference.Asset;
                }
                
                switch (systemTypeInstance)
                {
                    case Type t when t == typeof(GameObject):
                        assetReference.LoadAssetAsync<GameObject>();
                        break;
                    case Type t when t == typeof(Texture2D):
                        assetReference.LoadAssetAsync<Texture2D>();
                        break;
                    case Type t when t == typeof(Sprite):
                        assetReference.LoadAssetAsync<Sprite>();
                        break;
                    case Type t when t == typeof(Material):
                        assetReference.LoadAssetAsync<Material>();
                        break;
                    case Type t when t == typeof(AudioClip):
                        assetReference.LoadAssetAsync<AudioClip>();
                        break;
                    case Type t when t == typeof(TMPro.TMP_FontAsset):
                        assetReference.LoadAssetAsync<TMPro.TMP_FontAsset>();
                        break;
                    case Type t when t == typeof(TMPro.TMP_SpriteAsset):
                        assetReference.LoadAssetAsync<TMPro.TMP_SpriteAsset>();
                        break;
                    case Type t when t == typeof(TMPro.TMP_StyleSheet):
                        assetReference.LoadAssetAsync<TMPro.TMP_StyleSheet>();
                        break;
                    case Type t when t == typeof(TMPro.TMP_Settings):
                        assetReference.LoadAssetAsync<TMPro.TMP_Settings>();
                        break;
                    case Type t when t == typeof(TMPro.TextMeshProUGUI):
                        assetReference.LoadAssetAsync<TMPro.TextMeshProUGUI>();
                        break;
                    case Type t when t == typeof(TMPro.TextMeshPro):
                        assetReference.LoadAssetAsync<TMPro.TextMeshPro>();
                        break;
                    default:
                        var method = typeof(AssetReference)
                            .GetMethod("LoadAssetAsync", Type.EmptyTypes)?
                            .MakeGenericMethod(systemTypeInstance ?? typeof(Object));
                        method?.Invoke(assetReference, null);
                        break;
                }
                
                assetReference.OperationHandle.WaitForCompletion();
                _loadingHandles[(Object) assetReference.OperationHandle.Result] = assetReference.OperationHandle;
                return assetReference.Asset;
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