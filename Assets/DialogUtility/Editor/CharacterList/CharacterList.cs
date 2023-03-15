using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class CharacterList : ScriptableObject
    {
        public const string CharacterListPath = "Assets/DialogUtility/CharacterList.asset";
        
        private static CharacterList _instance;
        public static CharacterList Instance
        {
            get => _instance;
            set
            {
                _instance = value;
                EditorUtility.SetDirty(_instance);
                _instance.GlobalCharacterList =
                    _instance.globalCharacterDataList.Select(x => new CharacterModel(x)).ToList();
                _instance.OnCharacterChanged += _ => AssetDatabase.SaveAssets();;
                _instance.OnCharacterChanged += _ => AssetDatabase.SaveAssets();;
            }
        }

        public Action OnLocalListChanged { get; set; }
        public Action<CharacterModel> OnCharacterChanged { get; set; }
        public Action<CharacterModel> OnCharacterDeleted { get; set; }
        
        private DialogUtilityUsagesHandler UsagesHandler => DialogUtilityUsagesHandler.Instance;
        [HideInInspector]
        [SerializeField]
        public List<CharacterData> globalCharacterDataList; 
        [NonSerialized]
        public List<CharacterModel> GlobalCharacterList = new(); 
        [NonSerialized] 
        private List<CharacterModel> _localCharacterList = new();

        public static CharacterList CreateCharacterList()
        {
            var splitPath = CharacterListPath.Split('/');
            var subpath = "Assets";
            
            for (int i = 1; i< splitPath.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(subpath + "/" + splitPath[i]))
                {
                    AssetDatabase.CreateFolder(subpath, splitPath[i]);
                }
                subpath += "/" + splitPath[i];
            }
            
            var list = CreateInstance<CharacterList>();
            AssetDatabase.CreateAsset(list, subpath);
            AssetDatabase.SaveAssets();

            return list;
        }
        
        public CharacterModel FindCharacter(SerializableGuid id)
        {
            return GlobalCharacterList.Find(x => x.Id == id);
        }
        
        public CharacterModel FindCharacter(string charName)
        {
            return GlobalCharacterList.Find(x => x.Name == charName);
        }
        
        public void AddCharacterToLocal(CharacterModel data)
        {
            if (!_localCharacterList.Contains(data))
            {
                _localCharacterList.Add(data);
            }
            OnLocalListChanged?.Invoke();
        }

        public void RemoveCharacterFromLocal(CharacterModel data)
        {
            if (_localCharacterList.Contains(data))
            {
                _localCharacterList.Remove(data);
            }
            OnLocalListChanged?.Invoke();
        }

        public void UpdateLocalList(List<CharacterData> newLocalList)
        {
            _localCharacterList = GlobalCharacterList.Where(x => newLocalList.Exists(y=>y.id == x.Id)).ToList();
        }
        
        public List<string> GetLocalCharacterNames()
        {
            return _localCharacterList.Select(x => x.Name).ToList();
        }
        
        public List<string> GetGlobalCharacterNames()
        {
            return globalCharacterDataList.Select(x => x.Name).ToList();
        }

        public List<CharacterData> GetLocalCharactersListCopy()
        {
            return new List<CharacterData>(_localCharacterList.Select(x=>x.GetCharacterData()));
        }

        public CharacterModel GetCharacter(int index)
        {
            return GlobalCharacterList[index];
        }

        public void CreateCharacter()
        {
            CharacterModel data = new CharacterModel();
            globalCharacterDataList.Add(data.GetCharacterData());
            GlobalCharacterList.Add(data);
            OnLocalListChanged?.Invoke();
        }

        public bool DeleteCharacter(CharacterModel data)
        {
            bool decision = true;

            if (data.Usages.Count > 0)
            {
                var usagesNames = UsagesHandler.GetUsagesNames(data.Usages);
                var result = "";
                usagesNames.ForEach(x =>
                {
                    result += x + ", ";
                });
                decision = usagesNames.Count == 0 || EditorUtility.DisplayDialog(
                    title: "Delete character",
                    message: $"Character is used in dialogs: {result}do you want to delete it anyway?",
                    ok: "Yes",
                    cancel: "No"
                );
            }

            if (decision)
            {
                if (_localCharacterList.Contains(data))
                    _localCharacterList.Remove(data);
                if (GlobalCharacterList.Contains(data))
                    GlobalCharacterList.Remove(data);
                if (globalCharacterDataList.Contains(data.GetCharacterData()))
                    globalCharacterDataList.Remove(data.GetCharacterData());
                OnLocalListChanged?.Invoke();
                OnCharacterDeleted?.Invoke(data);
            }

            return decision;
        }

    }
}