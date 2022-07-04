using System.Collections.Generic;
using UnityEngine;

namespace Halluvision.GameplayTag
{
    public class GameplayTagAttribute : PropertyAttribute
    {
        public GameplayTagAttribute() { }
    }

    [System.Serializable]
    public class GameplayTag
    {
        public int Id;
        public string Tag;
        public int Depth;
        public int ParentID;
        public List<int> ChildrenIDs;
        public string Comment;

        public GameplayTag(string tag, int id, int depth, int parentID, string comment = "")
        {
            Tag = tag;
            Id = id;
            Depth = depth;
            ParentID = parentID;
            Comment = comment;
            ChildrenIDs = new List<int>();
        }

        public bool IsInHierarchy(int parentId)
        {
            var newTag = GameplayTagCollection.Instance.GetTagByID(parentId);
            if (newTag == null)
                return false;
            int index = newTag.ChildrenIDs.FindIndex(t => t == Id);
            if (index == -1)
                return false;
            else
                return true;
        }
    }
}