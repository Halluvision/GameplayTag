using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Halluvision.GameplayTag
{
    [System.Serializable]
    public class GameplayTagCollection
    {
        // Delegates
        public delegate void OnTagChanged();
        public static OnTagChanged onTagChanged;

        // public fields
        public List<GameplayTag> tags;

        // privates
        List<int> iDs;
        Dictionary<int, GameplayTag> tagsDic;

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

        private void Initialize()
        {
            tags = new List<GameplayTag>();
            if (GameplayTagFile.CheckIfFileExists())
            {
                string _json;
                if (GameplayTagFile.ReadFromGameplayTagsFile(out _json))
                    LoadFromJson(_json);
            }

            iDs = new List<int>();
            tagsDic = new Dictionary<int, GameplayTag>();
            foreach (GameplayTag _tag in tags)
            {
                iDs.Add(_tag.ID);
                tagsDic.Add(_tag.ID, _tag);
            }
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void LoadFromJson(string _json)
        {
            if (_json != "{}")
                JsonUtility.FromJsonOverwrite(_json, this);
            else
                tags.Clear();
        }

        public void AddTag(GameplayTag _tag, bool _writeToFile = true)
        {
            if (!tagsDic.ContainsKey(_tag.ID))
                tagsDic.Add(_tag.ID, _tag);

            if (_writeToFile)
                WriteToFile();

            UpdateTagsArray();
        }

        public void AddTag(string _gameplayTag, string _comment = "")
        {
            List<GameplayTag> _tags = GetOrCreateGameplayTagsFromString(_gameplayTag);

            _tags[_tags.Count - 1].comment = _comment;

            for (int i = 0; i < _tags.Count; i++)
                AddTag(_tags[i], false);

            WriteToFile();
            UpdateTagsArray();
        }

        public void RemoveTag(GameplayTag _tag)
        {
            if (tagsDic.ContainsKey(_tag.ID))
                tagsDic.Remove(_tag.ID);

            WriteToFile();
            UpdateTagsArray();
        }

        public void RemoveTag(string _gameplayTag)
        {
            List<string> _result = new List<string>();
            List<int> _ids = ParseStringTag(_gameplayTag, out _result);

            if (tagsDic.ContainsKey(_ids[_ids.Count - 1]))
                tagsDic.Remove(_ids[_ids.Count - 1]);

            WriteToFile();
            UpdateTagsArray();
        }

        public void RemoveTag(int _gameplayTagIds)
        {
            if (tagsDic.ContainsKey(_gameplayTagIds))
                tagsDic.Remove(_gameplayTagIds);

            WriteToFile();
            UpdateTagsArray();
        }

        public void RenameTag(int _gameplayTagIds, string _newName)
        {
            if (tagsDic.ContainsKey(_gameplayTagIds))
                tagsDic[_gameplayTagIds].tag = _newName;

            WriteToFile();
            UpdateTagsArray();
        }

        void WriteToFile()
        {
            UpdateTagsArray();
            string _json = ToJson();
            GameplayTagFile.WriteGameplayTagsToFile(_json, true);
        }

        private List<int> ParseStringTag(string _tagToParse, out List<string> _result)
        {
            _result = _tagToParse.Split('.').ToList();
            int[] _ids = Enumerable.Repeat(-1, _result.Count).ToArray();

            if (tags != null)
            {
                for (int i = 0; i < _result.Count; i++)
                {
                    if (i > 0 && _ids[i - 1] == -1)
                        break;

                    foreach (GameplayTag _tag in tags)
                    {
                        if (i == 0)
                        {
                            if (_tag.tag == _result[i] && _tag.depth == i)
                            {
                                _ids[i] = _tag.ID;
                                break;
                            }
                        }
                        else if (_tag.tag == _result[i] && _tag.depth == i && tagsDic[_tag.parentID].tag == _result[i - 1])
                        {
                            _ids[i] = _tag.ID;
                            break;
                        }
                    }
                }
            }

            return new List<int>(_ids);
        }

        private List<GameplayTag> GetOrCreateGameplayTagsFromString(string _tagToParse)
        {
            List<int> _ids;
            List<string> _result;
            _ids = ParseStringTag(_tagToParse, out _result);

            GameplayTag _root = new GameplayTag(null, 0, -1, -1, "Root");
            List<GameplayTag> _resultTags = new List<GameplayTag>();
            for (int i = 0; i < _result.Count; i++)
            {
                if (_ids[i] != -1)
                {
                    _resultTags.Add(tagsDic[_ids[i]]);
                }
                else if (i > 0)
                {
                    _ids[i] = GenerateNewID();
                    _resultTags.Add(new GameplayTag(_result[i], _ids[i], i, _ids[i - 1]));
                }
                else
                {
                    _ids[i] = GenerateNewID();
                    _resultTags.Add(new GameplayTag(_result[i], _ids[i], i, 0));
                }
            }

            return _resultTags;
        }

        int GenerateNewID()
        {
            for (int i = 1; i < Mathf.Infinity; i++)
                if (!iDs.Contains(i))
                {
                    iDs.Add(i);
                    return i;
                }

            return -1;
        }

        private void UpdateTagsArray()
        {
            tags = tagsDic.Values.ToList();
            onTagChanged?.Invoke();
        }

        public GameplayTag GetTagByID(int _gameplayTagID)
        {
            if (tagsDic.ContainsKey(_gameplayTagID))
                return tagsDic[_gameplayTagID];
            return null;
        }

        public int GetTagIDByString(string _gameplayTagString)
        {
            foreach (var _pair in tagsDic)
            {
                if (_pair.Value.tag == _gameplayTagString)
                    return _pair.Key;
            }
            return -1;
        }

        public string GetTagStringHierarchy(int _tagId)
        {
            if (!tagsDic.ContainsKey(_tagId))
                return "";

            GameplayTag _tag = tagsDic[_tagId];

            if (_tag.depth == 0)
                return _tag.tag;

            int[] _parentsIds = new int[_tag.depth];
            int _currentParentIds = _tag.parentID;

            for (int i = 0; i < _tag.depth; i++)
            {
                _parentsIds[i] = tagsDic[_currentParentIds].ID;
                _currentParentIds = tagsDic[_parentsIds[i]].parentID;
            }

            string _result = "";
            for (int i = _tag.depth - 1; i >= 0; i--)
            {
                _result += tagsDic[_parentsIds[i]].tag + ".";
            }
            _result += _tag.tag;
            return _result;
        }

        public void SetTagInUse(int _tagId, bool _inUse)
        {
            if (tagsDic.ContainsKey(_tagId))
            {
                tagsDic[_tagId].inUse = _inUse;
                WriteToFile();
            }
        }
    }
}