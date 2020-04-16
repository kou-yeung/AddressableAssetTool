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

                var extensions = item.extensions;
                if (string.IsNullOrEmpty(extensions)) extensions = "*.*";

                foreach (var extension in extensions.Split(';'))
                {
                    var option = item.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    foreach (var fn in Directory.GetFiles(path, extension, option))
                    {
                        if (Path.GetExtension(fn) == ".meta") continue; // meta データ弾く
                        var group = (item.assetType == AssetType.Include) ? asset.local : asset.remote;
                        var e = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(fn), group, false, true);
                        if (e != null)
                        {
                            e.labels.Clear();
                            e.SetLabel(item.assetType.ToString(), true);
                            entries.Add(e.guid);
                        }
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
                if (result != null)
                {
                    res = result.Error;
                }
                else
                {
                    // エラーが発生したため、初回ビルドとして処理する
                    AddressableAssetSettings.BuildPlayerContent();
                    Debug.Log("BuildContentUpdate Error:初回ビルドで処理する");
                }
            }
            else
            {
                // StateDataがなければ、初回ビルドする
                AddressableAssetSettings.BuildPlayerContent();
            }
            return res;
        }

        ///// <summary>
        ///// クリーンしてからビルドする
        ///// </summary>
        ///// <returns></returns>
        //[MenuItem("AAS/Clean Build")]
        //public static string CleanBuild()
        //{
        //    Clean();
        //    AddressableAssetSettings.BuildPlayerContent();
        //    return "";
        //}
        ///// <summary>
        ///// ビルドしたデータをクリアする
        ///// </summary>
        //public static void Clean()
        //{
        //    AddressableAssetSettings.CleanPlayerContent(null);
        //    BuildCache.PurgeCache(false);
        //}
    }
}
