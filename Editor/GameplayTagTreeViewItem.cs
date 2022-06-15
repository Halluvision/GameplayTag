using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Halluvision.GameplayTag
{
    public class GameplayTagTreeViewItem : TreeViewItem
    {
        public GameplayTag tag;

        public GameplayTagTreeViewItem(GameplayTag _tag)
        {
            tag = _tag;
            id = _tag.ID;
            depth = _tag.depth;
            displayName = _tag.tag;
        }
    }
}