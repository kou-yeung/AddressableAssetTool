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
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

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
            if(asset.items != null)
            {
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


        /// <summary>
        /// リモートグループを生成する
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        private static AddressableAssetGroup CreateRemoteGroup(this AddressableAssetSettings @this)
        {
            var name = "Remote";
            var group = @this.groups.Find(v => v.name == name);
            if(group == null)
            {
                group = @this.CreateGroup(name, false, false, false, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.BuildPath.SetVariableByName(@this, "RemoteBuildPath");
                schema.LoadPath.SetVariableByName(@this, "RemoteLoadPath");
                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            }
            return group;
        }

        /// <summary>
        /// 便利セットアップ
        /// </summary>
        [MenuItem("AAS/Setup")]
        public static void Setup()
        {
            // Info ファイルを検索
            var infoGuid = AssetDatabase.FindAssets("t:AddressableAssetsInfo", null).FirstOrDefault();
            if (infoGuid == null)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
                    settings = AddressableAssetSettingsDefaultObject.Settings;
                }

                // リモートサーバにカタログを置くように設定する
                settings.BuildRemoteCatalog = true;
                settings.RemoteCatalogBuildPath = settings.RemoteCatalogBuildPath;
                settings.RemoteCatalogLoadPath = settings.RemoteCatalogLoadPath;

                // Remote Load Path の BASE_URL 設定
                settings.profileSettings.SetValue(settings.activeProfileId, "RemoteLoadPath", "BASE_URL/[BuildTarget]");

                // リモートグループを生成
                var remoteGroup = settings.CreateRemoteGroup();

                // 同じ階層にinfo ファイルを生成する
                var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(AddressableAssetSettingsDefaultObject.Settings));
                var info = ScriptableObject.CreateInstance<AddressableAssetsInfo>();
                AssetDatabase.CreateAsset(info, Path.Combine(dir, "AddressableAssetsInfo.asset"));

                // グループをセットする
                info.local = settings.DefaultGroup;
                info.remote = remoteGroup;

                // infoを選択する
                Selection.activeObject = info;
            }
        }
    }
}
