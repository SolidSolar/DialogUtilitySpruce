using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    [Serializable]
    public class DialogUtilityUsagesHandler : ScriptableObject
    {
        public const string DialogUtilityUsagesHandlerPath = "Assets/DialogUtility/DialogUtilityUsagesHandler.asset";
        
        private static DialogUtilityUsagesHandler _instance;

        public static DialogUtilityUsagesHandler Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
                EditorUtility.SetDirty(_instance);
            }
        }
        
        public DialogGraphContainer CurrentContainer { get; set; }
        [SerializeField]
        private DictionaryOfSerializableGuidAndDialogGraphContainer dictionaryOfIdsAndContainers;

        public static DialogUtilityUsagesHandler CreateDialogUtilityUsagesHandler()
        {
            var splitPath = DialogUtilityUsagesHandlerPath.Split('/');
            var subpath = "Assets";
            
            for (int i = 1; i< splitPath.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(subpath + "/" + splitPath[i]))
                {
                    AssetDatabase.CreateFolder(subpath, splitPath[i]);
                }
                subpath += "/" + splitPath[i];
            }
            
            var handler = ScriptableObject.CreateInstance<DialogUtilityUsagesHandler>();
            AssetDatabase.CreateAsset(handler, subpath);
            AssetDatabase.SaveAssets();

            return handler;
        }
        
        /// <summary>
        /// Removes all usages of non-existing containers
        /// </summary>
        /// <param name="charId"></param>
        /// <param name="usages"></param>
        /// <returns></returns>
        public List<SerializableGuid> UpdateCharacterUsages(SerializableGuid charId, List<SerializableGuid> usages)
        {
            return usages.Where(x=>dictionaryOfIdsAndContainers.ContainsKey(x)).ToList();
        }

        public bool ProcessIfCopy(DialogGraphContainer container)
        {
            DialogGraphContainer original = null;
            var result = dictionaryOfIdsAndContainers.ContainsKey(container.id) &&
                   container != dictionaryOfIdsAndContainers[container.id];
            if (result)
            {
                original = dictionaryOfIdsAndContainers[container.id];
                if (!original)
                {
                    dictionaryOfIdsAndContainers.Remove(container.id);
                }
                _processCopy(container, original);
            }

            return result;
        }
        
        private void _processCopy(DialogGraphContainer copy, DialogGraphContainer original)
        {
            EditorUtility.SetDirty(copy);
            copy.id = Guid.NewGuid();
            dictionaryOfIdsAndContainers.Add(copy.id, copy);
            foreach (var data in CharacterList.Instance.globalCharacterDataList)
            {
                data.usages.Add(copy.id); 
            }

            if (original)
            {
                DialogLanguageHandler.Instance.CreateLocalisationResourceCopy(copy, original);
            }

            EditorUtility.ClearDirty(copy);
            AssetDatabase.SaveAssets();
        }
        
        public void UpdateDictionaryOfIdsAndContainers()
        {
            var keys = dictionaryOfIdsAndContainers.Keys.ToList();
            foreach (var key in keys)
            {
                if (!dictionaryOfIdsAndContainers[key] ||!AssetDatabase.Contains(dictionaryOfIdsAndContainers[key]))
                {
                    dictionaryOfIdsAndContainers.Remove(key);
                }
            }
            if (CurrentContainer && !dictionaryOfIdsAndContainers.ContainsKey(CurrentContainer.id))
            {
                dictionaryOfIdsAndContainers.Add(CurrentContainer.id, CurrentContainer);
            }
        }
        
        public List<string> GetUsagesNames(List<SerializableGuid> usages)
        {
            List<string> names = new List<string>();
            foreach (var usage in usages)
            {
                if (dictionaryOfIdsAndContainers.ContainsKey(usage))
                {
                    names.Add(dictionaryOfIdsAndContainers[usage].name);
                }
            }
            return names;
        }
    }
}