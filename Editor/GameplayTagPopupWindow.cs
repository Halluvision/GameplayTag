using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System;

namespace Halluvision.GameplayTag
{
    public class GameplayTagPopupWindow : PopupWindowContent
    {
        public Action<string, int> OnWindowClosed;

        [SerializeField] TreeViewState m_TreeViewState;

        GameplayTagTreeView m_TreeView;
        string propertyPath;
        int selectedItem;

        public GameplayTagPopupWindow(string _propertyPath)
        {
            propertyPath = _propertyPath;
            selectedItem = -1;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 500);
        }

        public override void OnOpen()
        {
            Initialize();
        }

        void Initialize()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            m_TreeView = new GameplayTagTreeView(m_TreeViewState, false);
            m_TreeView.onItemSingleClicked = ItemSingleClicked;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Choose from GameplayTags", EditorStyles.boldLabel);
            DoTreeView();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
        }

        public void UpdateTree()
        {
            m_TreeView.Reload();
        }

        void ItemSingleClicked(int _id)
        {
            selectedItem = _id;
        }

        public override void OnClose()
        {
            if (OnWindowClosed != null)
            {
                OnWindowClosed.Invoke(propertyPath, selectedItem);
            }
        }
    }
}