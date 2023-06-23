using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace AddressableAssetsTool
{
    /// <summary>
    /// AddressableAssetsTool でビルドする機能を提供します
    /// </summary>
    public static class AddressableAssetsTool
    {
        [MenuItem("AddressableAssetsTool/RefreshAssetsList")]
        public static void RefreshAssetsList()
        {
            var guid = AssetDatabase.FindAssets("t:AddressableAssetsInfo", null).FirstOrDefault();
            if (string.IsNullOrEmpty(guid)) return;

            var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetsInfo>(AssetDatabase.GUIDToAssetPath(guid));

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entries = new HashSet<string>();

            // 走査してグループに登録し、ラベル付けます
            if (asset.items != null)
            {
                // ラベル一覧を追加
                var labels = new HashSet<string>();
                foreach (var item in asset.items)
                {
                    if (!string.IsNullOrEmpty(item.label)) settings.AddLabel(item.label, true);
                }

                // アドレスをリストアップし、設定する
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
                            var group = (item.assetType == AssetType.Local) ? asset.local : asset.remote;
                            var e = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(fn), group, false, true);
                            if (e != null)
                            {
                                // 一旦リセット
                                e.address = e.AssetPath;

                                // 置き換えルールによって整形する
                                foreach (var replace in asset.replaces)
                                {
                                    e.address = e.address.Replace(replace.oldValue, replace.newValue);
                                }
                                // 拡張子を含まない場合
                                if (!asset.includeExtension) e.address = Path.ChangeExtension(e.address, null);

                                e.labels.Clear();
                                if (!string.IsNullOrEmpty(item.label)) e.SetLabel(item.label, true);
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
        [MenuItem("AddressableAssetsTool/Build")]
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
        [MenuItem("AddressableAssetsTool/Setup")]
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

                // Remote の Load Path設定
                settings.profileSettings.SetValue(settings.activeProfileId, "Remote", "Custom");
                settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", "{AddressableAssetsTool.AddressableAssets.RemoteLoadPath}/[BuildTarget]");

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
