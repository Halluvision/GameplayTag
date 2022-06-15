using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Halluvision.GameplayTag
{
    public class GameplayTagFile
    {
        const string dir = "/Resources/GameplayTags/";
        const string fileName = "GameplayTags.txt";

        public static void CreateGameplayTagsFile()
        {
            string _fullPath = Application.dataPath + dir;
            if (!File.Exists(_fullPath + fileName))
            {
                Debug.Log("Creating " + dir + fileName + " for first time.");
                
                if (!Directory.Exists(_fullPath))
                    Directory.CreateDirectory(_fullPath);
                var sr = File.CreateText(_fullPath + fileName);
                sr.Close();
            }
        }

        public static bool CheckIfFileExists()
        {
            string _fullPath = Application.dataPath + dir;
            if (!File.Exists(_fullPath + fileName))
            {
                return false;
            }
            return true;
        }

        public static bool ReadFromGameplayTagsFile(out string _json)
        {
            string _fullPath = Application.dataPath + dir;
            if (CheckIfFileExists())
            {
                _json = "";
                var sr = File.OpenText(_fullPath + fileName);
                var line = sr.ReadLine();
                while (line != null)
                {
                    _json += line;
                    line = sr.ReadLine();
                }
                return true;
            }
            else
            {
                CreateGameplayTagsFile();
                _json = null;
                return false;
            }
        }

        public static bool WriteGameplayTagsToFile(string _json, bool _overwrite = false)
        {
            string _fullPath = Application.dataPath + dir;
            if (!_overwrite)
            {
                if (File.Exists(_fullPath + fileName))
                {
                    Debug.Log(fileName + " already exist.");
                    return false;
                }
            }

            if (!Directory.Exists(_fullPath))
                Directory.CreateDirectory(_fullPath);

            var sr = File.CreateText(_fullPath + fileName);
            sr.WriteLine(_json);
            sr.Close();
            return true;
        }
    }
}