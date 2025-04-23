using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FinalClick.AddressableResources
{
    [Serializable]
    public class AddressableResourcesCollection
    {
        [SerializeField] private List<string> _keys = new List<string>();
        [SerializeField] private List<AssetReference> _references = new List<AssetReference>();

        public void AddUnique(string key, AssetReference reference)
        {
            string normalizedKey = AddressableResourcesAPI.NormalizeKey(key);
            
            if (_keys.Contains(normalizedKey) == true)
            {
                return;
            }

            _keys.Add(normalizedKey);
            _references.Add(reference);
        }

        public void RemoveWithKey(string key)
        {
            string normalizedKey = AddressableResourcesAPI.NormalizeKey(key);
            
            int index = _keys.IndexOf(normalizedKey);

            // if not found
            if(index == -1)
            {
                return;
            }
            
            _keys.RemoveAt(index);
            _references.RemoveAt(index);
        }

        public Dictionary<string, AssetReference> GetAsDictionary()
        {
            var dictionary = new Dictionary<string, AssetReference>();

            for (int i = 0; i < _keys.Count; i++)
            {
                var key = _keys[i];
                var reference = _references[i];
                dictionary.Add(key, reference);
            }
            
            return dictionary;
        }

        public void Clear()
        {
            _keys.Clear();
            _references.Clear();
        }
    }
}