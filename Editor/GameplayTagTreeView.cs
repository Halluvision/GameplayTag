using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Halluvision.GameplayTag
{
    public class GameplayTagTreeView : TreeView
    {
        public delegate void OnSelectionChanged(List<int> ids);
        public OnSelectionChanged onSelectionChanged;

        public delegate void OnItemSingleClicked(int id);
        public OnItemSingleClicked onItemSingleClicked;

        bool canEdit = false;

        public GameplayTagTreeView(TreeViewState state, bool _canEdit)
                : base(state)
        {
            canEdit = _canEdit;
            Reload();
            GameplayTagCollection.onTagChanged += Reload;
        }

        ~GameplayTagTreeView()
        {
            GameplayTagCollection.onTagChanged -= Reload;
        }

        protected override TreeViewItem BuildRoot()
        {
            var _root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            var allItems = new List<GameplayTagTreeViewItem>();

            foreach (GameplayTag _tag in GameplayTagCollection.Instance.Tags)
            {
                allItems.Add(new GameplayTagTreeViewItem(_tag));
            }

            if (GameplayTagCollection.Instance.Tags.Count == 0)
            {
                allItems.Add(new GameplayTagTreeViewItem(new GameplayTag("TempTag", 0, 0, -1, "Temp tag")));
            }

            SetupParentsAndChildrenByMyRules(_root, allItems);
            return _root;
        }

        void SetupParentsAndChildrenByMyRules(TreeViewItem _root, List<GameplayTagTreeViewItem> _allItems)
        {
            foreach (var _item in _allItems)
            {
                var _parent = _allItems.Find(i => i.id == _item.tag.ParentID);
                var _children = _allItems.FindAll(i => i.tag.ParentID == _item.tag.Id);
                _item.parent = _parent;
                foreach (var _child in _children)
                {
                    _item.AddChild(_child);
                }
            }
            var _rootChildren = _allItems.FindAll(i => i.tag.Depth == 0);
            foreach (var _child in _rootChildren)
            {
                _root.AddChild(_child);
            }
        }

        protected override void SelectionChanged(IList<int> _selectedIds)
        {
            if (onSelectionChanged != null)
            {
                List<int> _selection = new List<int>(_selectedIds);
                onSelectionChanged(_selection);
            }
        }

        protected override void SingleClickedItem(int id)
        {
            if (onItemSingleClicked != null)
                onItemSingleClicked(id);
        }

        protected override void DoubleClickedItem(int id)
        {
            TreeViewItem item = FindItem(id, rootItem);
            if (CanRename(item))
                BeginRename(item);
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);
            // TODO: expand and collapse selected
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return canEdit;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
            if (args.acceptedRename)
            {
                if (args.newName == args.originalName)
                    return;
                GameplayTagCollection.Instance.RenameTag(args.itemID, args.newName);
            }
        }

        public List<int> GetItemChildrenIDs(int _itemID)
        {
            var _item = FindItem(_itemID, rootItem);
            if (_item.hasChildren)
            {
                var _children = _item.children;
                List<int> _ids = new List<int>();

                foreach (var _child in _children)
                    _ids.Add(_child.id);

                return _ids;
            }
            return null;
        }
    }
}