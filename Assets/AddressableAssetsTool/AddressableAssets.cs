using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        /// RemoteLoadPath : AddressableProfilesから参照されます
        /// </summary>
        public static string RemoteLoadPath { get; private set; }

        public static void Init(string remoteLoadPath)
        {
            RemoteLoadPath = remoteLoadPath;
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
    }
}
