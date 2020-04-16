using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressableAssetsTool;

public class Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AddressableAssets.BaseUrl = "http://localhost:8000";

        AddressableAssets.GetPreloadSizeAsync().Completed += (size) =>
        {
            if (size.Result > 0)
            {
                var handle = AddressableAssets.PreloadDependenciesAsync();
                StartCoroutine(DownloadWait(handle));
                handle.Completed += (res) =>
                {
                    Load();
                };
            }
            else
            {
                Load();
            }
        };
    }

    IEnumerator DownloadWait(AsyncOperationHandle handle)
    {
        while (!handle.IsDone)
        {
            Debug.Log(handle.PercentComplete);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Load()
    {
        var resource = AddressableAssets.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Chara.prefab");
        resource.Completed += (res) =>
        {
            Instantiate(res.Result, this.transform);
        };
    }
}

