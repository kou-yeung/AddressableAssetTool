using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
#endif

public class Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Addressables.GetDownloadSizeAsync("Preload").Completed += (size) =>
        {
            if (size.Result > 0)
            {
                var handle = Addressables.DownloadDependenciesAsync("Preload", true);
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
        var resource = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Chara.prefab");
        resource.Completed += (res) =>
        {
            Instantiate(res.Result, this.transform);
        };

    }
}
