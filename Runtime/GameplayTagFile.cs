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
        const string _dir = "/Resources/GameplayTags/";
        const string _fileName = "GameplayTags.json";

        public static string FullPath
        { 
            get
            {
                return Application.dataPath + _dir;
            }
        }

        public static void CreateGameplayTagsFile()
        {
            if (!File.Exists(FullPath + _fileName))
            {
                Debug.Log("Creating " + _dir + _fileName + " for first time.");
                
                if (!Directory.Exists(FullPath))
                    Directory.CreateDirectory(FullPath);
                var sr = File.CreateText(FullPath + _fileName);
                sr.Close();
            }
        }

        public static bool CheckIfFileExists()
        {
            if (!File.Exists(FullPath + _fileName))
            {
                return false;
            }
            return true;
        }

        public static bool ReadFromGameplayTagsFile(out string _json)
        {
            if (CheckIfFileExists())
            {
                _json = "";
                var sr = File.OpenText(FullPath + _fileName);
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
            if (!_overwrite)
            {
                if (File.Exists(FullPath + _fileName))
                {
                    Debug.Log(_fileName + " already exist.");
                    return false;
                }
            }

            if (!Directory.Exists(FullPath))
                Directory.CreateDirectory(FullPath);

            using (var sr = new StreamWriter(File.Create(FullPath + _fileName)))
            {
                sr.WriteLine(_json);
            }

            return true;
        }
    }
}