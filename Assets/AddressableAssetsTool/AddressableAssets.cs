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
        Include,    // アプリに内蔵する
        Preload,    // 追加ダウンロード(チュートリアル後か、タイトル後
        Async,      // 随時ダウンロード
    }

    public static class AddressableAssets
    {
        /// <summary>
        /// replace "BASE_URL"
        /// </summary>
        public static string BaseUrl = "";

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
        /// Preloadのサイズ取得
        /// </summary>l
        /// <returns></returns>
        public static AsyncOperationHandle<long> GetPreloadSizeAsync()
        {
            return Addressables.GetDownloadSizeAsync(AssetType.Preload.ToString());
        }

        /// <summary>
        /// Preloadラベルのデータをダウンロードする
        /// </summary>
        /// <param name="key"></param>
        /// <param name="autoReleaseHandle"></param>
        /// <returns></returns>
        public static AsyncOperationHandle PreloadDependenciesAsync()
        {
            return Addressables.DownloadDependenciesAsync(AssetType.Preload.ToString(), true);
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
