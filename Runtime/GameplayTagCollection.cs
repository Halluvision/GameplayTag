using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace Halluvision.GameplayTag
{
    [System.Serializable]
    public class GameplayTagCollection
    {
        // Delegates
        public delegate void OnTagChanged();
        public static OnTagChanged onTagChanged;
        
        [SerializeField]
        private Dictionary<int, GameplayTag> _tagsDic;

        public List<GameplayTag> Tags 
        { 
            get 
            {
                if (_tagsDic == null)
                {
                    _tagsDic = new Dictionary<int, GameplayTag>();
                }

                return _tagsDic.Values.ToList<GameplayTag>();
            } 
        }

        private static GameplayTagCollection instance = null;
        public static GameplayTagCollection Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameplayTagCollection();
                }
                return instance;
            }
        }

        private GameplayTagCollection()
        {
            Initialize();
        }

        public static void ReloadGameplayTags()
        {
            Instance.Initialize();
        }

        private void Initialize()
        {
            if (GameplayTagFile.CheckIfFileExists())
            {
                _tagsDic = new Dictionary<int, GameplayTag>();
                string json;
                if (GameplayTagFile.ReadFromGameplayTagsFile(out json))
                    LoadTagsFromJson(json);
            }
        }

        private string ToJson()
        {
            return JsonConvert.SerializeObject(_tagsDic, Formatting.Indented);
        }

        private void LoadTagsFromJson(string json)
        {
            _tagsDic = new Dictionary<int, GameplayTag>();
            _tagsDic = JsonConvert.DeserializeObject<Dictionary<int, GameplayTag>>(json);
        }

        private bool IsValidTag(GameplayTag tag)
        {
            if (tag == null) return false;
            return IsValidTag(tag.Id);
        }

        private bool IsValidTag(int tagId)
        {
            if (_tagsDic.ContainsKey(tagId))
                if (_tagsDic[tagId] == null)
                    return false;
            if (!_tagsDic.ContainsKey(tagId)) return false;
            return true;
        }

        private List<GameplayTag> Traverse(int tagId)
        {
            List<GameplayTag> tags = new List<GameplayTag>();
            var tag = GetTagByID(tagId);
            foreach (var childTagId in tag.ChildrenIDs)
            {
                if (IsValidTag(childTagId))
                {
                    var childTag = GetTagByID(childTagId);
                    tags.Add(childTag);
                    tags.AddRange(Traverse(childTagId));
                }
            }
            return tags;
        }

        private void AddTag(GameplayTag tag)
        {
            if (IsValidTag(tag))
            {
                _tagsDic[tag.Id] = tag;
                return;
            }
            
            if (_tagsDic.ContainsKey(tag.Id))
                _tagsDic[tag.Id] = tag;
            else
                _tagsDic.Add(tag.Id, tag);
        }

        public void AddTag(string gameplayTag, string comment = "")
        {
            List<GameplayTag> tags = ParseStringTag(gameplayTag);
            ProcessTagsHierarchy(ref tags);

            tags[tags.Count - 1].Comment = comment;

            foreach (var tag in tags)
            {
                AddTag(tag);
            }

            WriteToFile();
        }

        private void RemoveTag(GameplayTag tag)
        {
            if (!IsValidTag(tag)) return;
            
            _tagsDic.Remove(tag.Id);
        }

        public void RemoveTag(int gameplayTagId)
        {
            List<GameplayTag> tags = Traverse(gameplayTagId);
            var mainTag = GetTagByID(gameplayTagId);
            tags.Add(mainTag);

            if (_tagsDic.ContainsKey(mainTag.ParentID))
            {
                var parentTag = GetTagByID(mainTag.ParentID);
                parentTag.ChildrenIDs.RemoveAll(i => i == gameplayTagId);
            }

            foreach (var tag in tags)
            {
                RemoveTag(tag);
            }

            WriteToFile();
        }

        public void RenameTag(int gameplayTagId, string newName)
        {
            if (IsValidTag(gameplayTagId))
            {
                _tagsDic[gameplayTagId].Tag = newName;

                WriteToFile();
            }
        }

        void WriteToFile()
        {
            string _json = ToJson();
            GameplayTagFile.WriteGameplayTagsToFile(_json, true);
            onTagChanged?.Invoke();
        }

        /// <summary>
        /// Take a dot-separated string and checks if it belongs to a previous tag hierarchy.
        /// The return list of integers indicated the hierarchy until it reaches -1 which means its new tag 
        /// </summary>
        /// <param name="tagToParse"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<GameplayTag> ParseStringTag(string tagToParse)
        {
            List<string> result = tagToParse.Split('.').ToList();
            GameplayTag[] outTags = new GameplayTag[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                outTags[i] = new GameplayTag("", -1, -1, -1, "");
                outTags[i].Tag = result[i];
                outTags[i].Depth = i;
                outTags[i].ChildrenIDs = new List<int>();
            }

            bool isInDict = _tagsDic.Values.Any(t => t.Depth == 0 && t.Tag == outTags[0].Tag);
            if (!isInDict)
                return new List<GameplayTag>(outTags);

            var parentTag = _tagsDic.Values.First(t => t.Depth == 0 && t.Tag == outTags[0].Tag);
            outTags[0] = parentTag;

            GameplayTag foundedTag = parentTag;
            for (int j = 1; j < outTags.Length; j++)
            {
                foreach (int id in outTags[j - 1].ChildrenIDs)
                {
                    if (_tagsDic[id].Tag == outTags[j].Tag)
                    {
                        outTags[j] = _tagsDic[id];
                        break;
                    }
                }
            }

            return new List<GameplayTag>(outTags);
        }

        /// <summary>
        /// Find next available integer in tags ids
        /// </summary>
        /// <returns></returns>
        int GetAvailableTagID()
        {
            var keys = _tagsDic.Keys;
            int id = -1;
            for (int i = 1; i < Mathf.Infinity; i++)
                if (!keys.Contains(i))
                {
                    id = i;
                    _tagsDic.Add(id, null);
                    return id;
                }

            return -1;
        }

        /// <summary>
        /// Add children and parent ids to new tags
        /// </summary>
        /// <param name="gameplayTags"></param>
        private void ProcessTagsHierarchy(ref List<GameplayTag> gameplayTags)
        {
            for (int i = 0; i < gameplayTags.Count; i++)
            {
                if (gameplayTags[i].Id == -1)
                    gameplayTags[i].Id = GetAvailableTagID();

                if (i > 0)
                {
                    gameplayTags[i].ParentID = gameplayTags[i - 1].Id;
                    gameplayTags[i - 1].ChildrenIDs.Add(gameplayTags[i].Id);
                }
            }
        }

        private void ValidateAllTags()
        {
            foreach (var tag in _tagsDic.Values)
            {
                if (_tagsDic.ContainsKey(tag.ParentID))
                    if (!_tagsDic[tag.ParentID].ChildrenIDs.Contains(tag.Id))
                        _tagsDic[tag.ParentID].ChildrenIDs.Add(tag.Id);
            }
        }

        public GameplayTag GetTagByID(int gameplayTagID)
        {
            if (_tagsDic.ContainsKey(gameplayTagID))
                return _tagsDic[gameplayTagID];
            return null;
        }

        public int GetTagIDByString(string gameplayTagString)
        {
            foreach (var _pair in _tagsDic)
            {
                if (_pair.Value.Tag == gameplayTagString)
                    return _pair.Key;
            }
            return -1;
        }

        public string GetTagStringHierarchy(int tagId)
        {
            if (!_tagsDic.ContainsKey(tagId))
                return "";

            GameplayTag tag = _tagsDic[tagId];

            if (tag.Depth == 0)
                return tag.Tag;

            int[] parentsIds = new int[tag.Depth];
            int currentParentIds = tag.ParentID;

            for (int i = 0; i < tag.Depth; i++)
            {
                parentsIds[i] = _tagsDic[currentParentIds].Id;
                currentParentIds = _tagsDic[parentsIds[i]].ParentID;
            }

            string result = "";
            for (int i = tag.Depth - 1; i >= 0; i--)
            {
                result += _tagsDic[parentsIds[i]].Tag + ".";
            }
            result += tag.Tag;
            return result;
        }
    }
}