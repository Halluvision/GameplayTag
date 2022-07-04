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
            if (IsValidTag(tag)) return;
            
            if (_tagsDic.ContainsKey(tag.Id))
                _tagsDic[tag.Id] = tag;
            else
                _tagsDic.Add(tag.Id, tag);
        }

        public void AddTag(string gameplayTag, string comment = "")
        {
            List<GameplayTag> tags = GetOrCreateGameplayTagsFromString(gameplayTag);
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
        private List<int> ParseStringTag(string tagToParse, out List<string> result)
        {
            result = tagToParse.Split('.').ToList();
            int[] ids = Enumerable.Repeat(-1, result.Count).ToArray();

            for (int i = 0; i < result.Count; i++)
            {
                if (i > 0 && ids[i - 1] == -1)
                    break;

                foreach (GameplayTag _tag in _tagsDic.Values)
                {
                    if (i == 0)
                    {
                        if (_tag.Tag == result[i] && _tag.Depth == i)
                        {
                            ids[i] = _tag.Id;
                            break;
                        }
                    }
                    else if (_tag.Tag == result[i] && _tag.Depth == i && _tagsDic[_tag.ParentID].Tag == result[i - 1])
                    {
                        ids[i] = _tag.Id;
                        break;
                    }
                }
            }

            return new List<int>(ids);
        }

        /// <summary>
        /// Create unprocessed tags list from a dot-separated string
        /// </summary>
        /// <param name="tagToParse"></param>
        /// <returns></returns>
        private List<GameplayTag> GetOrCreateGameplayTagsFromString(string tagToParse)
        {
            List<int> ids;
            List<string> result;
            ids = ParseStringTag(tagToParse, out result);

            List<GameplayTag> resultTags = new List<GameplayTag>();
            for (int i = 0; i < result.Count; i++)
            {
                if (ids[i] != -1)
                {
                    resultTags.Add(_tagsDic[ids[i]]);
                    continue;
                }

                ids[i] = GetAvailableTagID();
                resultTags.Add(new GameplayTag(result[i], ids[i], i, -1));
            }

            return resultTags;
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
            int length = gameplayTags.Count;
            for (int i = 0; i < length; i++)
            {
                if (i < length - 1)
                {
                    gameplayTags[i].ChildrenIDs.Add(gameplayTags[i + 1].Id);
                }
                if (i > 0 && gameplayTags[i].ParentID == -1)
                {
                    gameplayTags[i].ParentID = gameplayTags[i - 1].Id;
                }
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