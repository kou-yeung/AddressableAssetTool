using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressableAssetsTool;
using UnityEngine.AddressableAssets;

public class Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AddressableAssets.Init("http://localhost:8000");

        AddressableAssets.GetSizeAsync().Completed += (size) =>
        {
            if (size.Result > 0)
            {
                var handle = AddressableAssets.DownloadDependenciesAsync();
                StartCoroutine(DownloadWait(handle));
                handle.Completed += (res) =>
                {
                    Load();
                    Addressables.Release(res);
                };
            }
            else
            {
                Load();
            }
            Addressables.Release(size);
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

