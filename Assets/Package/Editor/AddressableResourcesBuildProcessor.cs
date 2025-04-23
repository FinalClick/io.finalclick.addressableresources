using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FinalClick.AddressableResources.Editor
{
    public class AddressableResourcesBuildProcessor : IProcessSceneWithReport
    {
        public int callbackOrder { get; }
        
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // Only inject into the first scene boot scene
            if (Application.isPlaying == false && SceneManager.GetSceneAt(0) != scene)
            {
                return;
            }

            if(Application.isPlaying == true && AddressableResourcesAPI.IsInitialized() == true)
            {
                return;
            }

            AddressableResourcesCollection collection = AddressableResourcesEditorAPI.CreateProjectAddressableResourcesCollection();
            
            GameObject gameObject = new GameObject("Addressable Resources");
            var initializer = gameObject.AddComponent<AddressableResourcesInitializer>();
            initializer.SetCollection(collection);
        }
    }
}