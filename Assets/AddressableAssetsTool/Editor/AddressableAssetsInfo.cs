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
        [SerializeField] public bool recursive;             // 再帰する
        [SerializeField] public string extensions;          // 拡張子 ',' 区切り

        public static string[] Properties = new[]
        {
            "path", "assetType", "recursive", "extensions"
        };
    }

    [CustomPropertyDrawer(typeof(AddressableAssetsItem))]
    public class PersonDataDrawer : PropertyDrawer
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
        [MenuItem("Assets/Create/AAS/AddressableAssetsInfo")]
        public static void Create()
        {
            ProjectWindowUtil.CreateAsset(CreateInstance<AddressableAssetsInfo>(), "AddressableAssetsInfo.asset");
        }

        [SerializeField]
        public AddressableAssetGroup local;
        public AddressableAssetGroup remote;

        [SerializeField]
        public List<AddressableAssetsItem> items;
    }

    /// <summary>
    /// AddressableAssetsInfo のインスペクタ表示
    /// </summary>
    [CustomEditor(typeof(AddressableAssetsInfo))]
    public class AddressableAssetsInfoEditor : Editor
    {
        private ReorderableList reorderableList;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // group
            EditorGUILayout.PropertyField(serializedObject.FindProperty("local"), new GUIContent("Local"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("remote"), new GUIContent("Remote"));

            // build button
            if (GUILayout.Button("Build", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                AddressableAssetsTool.Build();
                return;
            }

            // ビルドルール
            if (reorderableList == null)
            {
                var prop = serializedObject.FindProperty("items");

                reorderableList = new ReorderableList(serializedObject, prop);

                // タイトル設定
                reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Build Rule");
                // 高さ取得
                reorderableList.elementHeightCallback = index => (EditorGUIUtility.singleLineHeight * (AddressableAssetsItem.Properties.Length + 2));
                // アイテム描画
                reorderableList.drawElementCallback = (rect, index, isActive, isFocus) =>
                {
                    var element = prop.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };
            }
            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
