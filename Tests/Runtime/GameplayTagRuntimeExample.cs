using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Halluvision.GameplayTag;

public class GameplayTagRuntimeExample : MonoBehaviour
{
    [SerializeField, GameplayTag]
    int _tag;

    void Start()
    {
        var tag = GameplayTagCollection.Instance.GetTagByID(_tag);
        bool isInHierarchy = tag.IsInHierarchy(tag.ParentID);
        Debug.Log(isInHierarchy);
    }

    void Update()
    {
    }
}
