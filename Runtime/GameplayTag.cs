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
        public int ID;
        public string tag;
        public int depth;
        public int parentID;
        public List<int> childrenIDs;
        public string comment;
        public bool inUse;

        public GameplayTag()
        {
        }

        public GameplayTag(string _tag, int _ID, int _depth, int _parentID, string _comment = "")
        {
            tag = _tag;
            ID = _ID;
            depth = _depth;
            parentID = _parentID;
            comment = _comment;
        }

        public void AddChild(int _childID)
        {
            childrenIDs.Add(_childID);
        }
    }
}