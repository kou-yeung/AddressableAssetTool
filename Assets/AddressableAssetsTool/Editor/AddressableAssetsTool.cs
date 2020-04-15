using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

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

    /// <summary>
    /// AAS でビルドする機能を提供します
    /// </summary>
    public static class AddressableAssetsTool
    {
        [MenuItem("AAS/RefreshAssetsList")]
        public static void RefreshAssetsList()
        {
            var guid = AssetDatabase.FindAssets("t:AddressableAssetsInfo", null).FirstOrDefault();
            if (string.IsNullOrEmpty(guid)) return;

            var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetsInfo>(AssetDatabase.GUIDToAssetPath(guid));

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entries = new HashSet<string>();

            // 使用するラベルを追加する
            foreach (var label in Enum.GetNames(typeof(AssetType)))
            {
                settings.AddLabel(label, true);
            }

            // 走査してグループに登録し、ラベル付けます
            foreach (var item in asset.items)
            {
                if (item.path == null) continue;

                string path = AssetDatabase.GetAssetOrScenePath(item.path);
                foreach (var extension in item.extensions.Split(';'))
                {
                    var option = item.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    foreach (var fn in Directory.GetFiles(path, extension, option))
                    {
                        var group = (item.assetType == AssetType.Include) ? asset.local : asset.remote;
                        var e = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(fn), group, false, true);
                        e.labels.Clear();
                        e.SetLabel(item.assetType.ToString(), true);
                        entries.Add(e.guid);
                    }
                }
            }

            // 対象外のアセットを削除する
            foreach (var group in settings.groups)
            {
                var ids = group.entries.Where(v => !entries.Contains(v.guid)).Select(v => v.guid).ToArray();
                foreach (var id in ids)
                {
                    settings.RemoveAssetEntry(id);
                }
            }
        }

        /// <summary>
        /// ビルド
        /// 内部で初回ビルドか、アップデータビルドかを自動的切り替える
        /// </summary>
        /// <returns></returns>
        [MenuItem("AAS/Build")]
        public static string Build()
        {
            RefreshAssetsList();

            var path = ContentUpdateScript.GetContentStateDataPath(false);
            var res = "";
            if (File.Exists(path))
            {
                // すでにStateDataがあれば、ContentUpdate(差分)ビルドします
                var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
                res = result.Error;
            }
            else
            {
                // StateDataがなければ、初回ビルドする
                AddressableAssetSettings.BuildPlayerContent();
            }
            return res;
        }

        /// <summary>
        /// クリーンしてからビルドする
        /// </summary>
        /// <returns></returns>
        [MenuItem("AAS/Clean Build")]
        public static string CleanBuild()
        {
            Clean();
            return Build();
        }
        /// <summary>
        /// ビルドしたデータをクリアする
        /// </summary>
        public static void Clean()
        {
            AddressableAssetSettings.CleanPlayerContent(null);
            BuildCache.PurgeCache(false);
        }
    }
}
