using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AddressableAssetsTool
{
    /// <summary>
    /// アセットの種類:Label に設定される
    /// </summary>
    public enum AssetType
    {
        Local,      // アプリに内蔵する
        Remode,     // サーバに置く
    }

    public static class AddressableAssets
    {
        /// <summary>
        /// replace "BASE_URL"
        /// </summary>
        static string BaseUrl = "";

        public static void Init(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        static string CustomTransform(IResourceLocation location)
        {
            return location.InternalId.Replace("BASE_URL", BaseUrl);
        }

        [RuntimeInitializeOnLoadMethod]
        static void SetInternalIdTransform()
        {
            Addressables.InternalIdTransformFunc = CustomTransform;
        }

        /// <summary>
        /// 指定してラベルのサイズ取得
        /// </summary>l
        /// <returns></returns>
        public static AsyncOperationHandle<long> GetSizeAsync(string label = "Preload")
        {
            return Addressables.GetDownloadSizeAsync(label);
        }

        /// <summary>
        /// 指定したラベルのデータをダウンロードする
        /// </summary>
        /// <param name="key"></param>
        /// <param name="autoReleaseHandle"></param>
        /// <returns></returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(string label = "Preload")
        {
            return Addressables.DownloadDependenciesAsync(label, true);
        }

        /// <summary>
        /// ロードする
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
        {
            return Addressables.LoadAssetAsync<TObject>(key);
        }

        ///// <summary>
        ///// デバッグ情報を表示する
        ///// </summary>
        //public static void ShowInfo()
        //{
        //    Debug.Log(Addressables.RuntimePath);
        //}
    }
}
