using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.AddressableAssets.Settings;
using System.Globalization;

namespace AddressableAssetsTool
{
    [Serializable]
    public class AddressableAssetsItem
    {
        [SerializeField] public DefaultAsset path;          // フォルダ
        [SerializeField] public AssetType assetType;        // 種類
        [SerializeField] public string label;               // ラベル
        [SerializeField] public bool recursive;             // 再帰する
        [SerializeField] public string extensions;          // 拡張子 ',' 区切り

        public static string[] Properties = new[]
        {
            "path", "assetType", "label", "recursive", "extensions"
        };
    }

    [CustomPropertyDrawer(typeof(AddressableAssetsItem))]
    public class AddressableAssetsItemDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUIUtility.labelWidth = 70.0f;

            var labelText = label.text;
            var path = property.FindPropertyRelative("path");
            if (path != null)
            {
                labelText = AssetDatabase.GetAssetOrScenePath(path.objectReferenceValue);
            }

            EditorGUI.LabelField(position, labelText);
            position.y += EditorGUIUtility.singleLineHeight;

            foreach (var p in AddressableAssetsItem.Properties)
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative(p), new GUIContent(p));
                position.y += EditorGUIUtility.singleLineHeight;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }

    [Serializable]
    public class AddressableAssetsInfo : ScriptableObject
    {
        [MenuItem("Assets/Create/AddressableAssetsTool/AddressableAssetsInfo")]
        public static void Create()
        {
            ProjectWindowUtil.CreateAsset(CreateInstance<AddressableAssetsInfo>(), "AddressableAssetsInfo.asset");
        }

        public AddressableAssetGroup local;
        public AddressableAssetGroup remote;

        public bool includeExtension = true;
        public List<ReplaceItem> replaces;
        public List<AddressableAssetsItem> items;
    }

    [Serializable]
    public class ReplaceItem
    {
        public string oldValue;
        public string newValue;
    }
    [CustomPropertyDrawer(typeof(ReplaceItem))]
    public class ReplaceItemDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("oldValue"), new GUIContent("Old Value"));
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("newValue"), new GUIContent("New Value"));
            position.y += EditorGUIUtility.singleLineHeight;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }


    /// <summary>
    /// AddressableAssetsInfo のインスペクタ表示
    /// </summary>
    [CustomEditor(typeof(AddressableAssetsInfo))]
    public class AddressableAssetsInfoEditor : Editor
    {
        private ReorderableList buildRuleList;
        private ReorderableList replaceRuleList;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // group
            EditorGUILayout.PropertyField(serializedObject.FindProperty("local"), new GUIContent("Local"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("remote"), new GUIContent("Remote"));

            // settings
            EditorGUILayout.PropertyField(serializedObject.FindProperty("includeExtension"), new GUIContent("Include Extension"));

            // 置き換えルール
            if (replaceRuleList == null)
            {
                var prop = serializedObject.FindProperty("replaces");

                replaceRuleList = new ReorderableList(serializedObject, prop);

                // タイトル設定
                replaceRuleList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Replace Rule");
                // 高さ取得
                replaceRuleList.elementHeightCallback = index => (EditorGUIUtility.singleLineHeight * 2.5f);
                // アイテム描画
                replaceRuleList.drawElementCallback = (rect, index, isActive, isFocus) =>
                {
                    var element = prop.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };
            }
            replaceRuleList.DoLayoutList();

            // build button
            if (GUILayout.Button("Build", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                AddressableAssetsTool.Build();
                return;
            }

            // ビルドルール
            if (buildRuleList == null)
            {
                var prop = serializedObject.FindProperty("items");

                buildRuleList = new ReorderableList(serializedObject, prop);

                // タイトル設定
                buildRuleList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Build Rule");
                // 高さ取得
                buildRuleList.elementHeightCallback = index => (EditorGUIUtility.singleLineHeight * (AddressableAssetsItem.Properties.Length + 2));
                // アイテム描画
                buildRuleList.drawElementCallback = (rect, index, isActive, isFocus) =>
                {
                    var element = prop.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };
            }
            buildRuleList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
