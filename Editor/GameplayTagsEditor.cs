using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Halluvision.GameplayTag
{
    public class GameplayTagsEditor : EditorWindow
    {
        static GameplayTagsEditor window;

        [SerializeField] TreeViewState treeViewState;

        GameplayTagTreeView treeView;
        string input;
        string comment;
        List<int> selectionIds;

        [MenuItem("Halluvision/GameplayTags Editor")]
        static void Init()
        {
            window = GetWindow<GameplayTagsEditor>();
            window.titleContent = new GUIContent("Gameplay Tags");
            window.Show();
        }

        void OnEnable()
        {
            GameplayTagCollection.ReloadGameplayTags();

            if (treeViewState == null)
                treeViewState = new TreeViewState();
            
            CreateTreeView();
        }

        void CreateTreeView()
        {
            treeView = new GameplayTagTreeView(treeViewState, true);
            selectionIds = new List<int>();
            treeView.onSelectionChanged = OnUserSelectionChange;
        }

        void OnUserSelectionChange(List<int> _ids)
        {
            selectionIds = _ids;
        }

        void OnGUI()
        {
            DoToolbar();
            DoTreeTopButtons();
            DoTreeView();
            DoTreeBottomButtons();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            treeView?.OnGUI(rect);
        }

        public void UpdateTree()
        {
            treeView?.Reload();
        }

        void DoToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DoTreeTopButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GameplayTag", EditorStyles.boldLabel);
            GUILayout.Label("Comment", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            input = EditorGUILayout.TextField(input);
            comment = EditorGUILayout.TextField(comment);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Add"))
            {
                GameplayTagCollection.Instance.AddTag(input, comment);
            }
        }

        void DoTreeBottomButtons()
        {
            if (GUILayout.Button("Remove Selected Tags"))
            {
                if (selectionIds.Count == 0)
                    return;

                if (selectionIds.Count > 1)
                {
                    EditorUtility.DisplayDialog("Error", "Please select one entity each time.", "OK");
                    return;
                }

                if (!EditorUtility.DisplayDialog("Are You Sure?", "If this GameplayTag is in use you might get an error later.", "Remove", "Cancel"))
                    return;

                GameplayTagCollection.Instance.RemoveTag(selectionIds[0]);
            }
        }
    }
}