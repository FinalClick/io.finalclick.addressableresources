using UnityEngine;

namespace FinalClick.AddressableResources
{
    [DefaultExecutionOrder(-100)]
    public class AddressableResourcesInitializer : MonoBehaviour
    {
        [SerializeField, HideInInspector] private AddressableResourcesCollection _collection = new();

        public void SetCollection(AddressableResourcesCollection collection)
        {
            _collection = collection;
        }
        
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            AddressableResourcesAPI.Initialize(_collection);
        }
    }
}
