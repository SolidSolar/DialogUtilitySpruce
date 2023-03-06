using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class LocalisationResourceSaveUtility
    {
        private const string SettingsPath = "Assets/Resources/DialogUtility/Settings/Settings.asset";
        private const string LocalisationPath = "Assets/Resources/DialogUtility/ContainerLocalisation/{0}/{1}.asset";
        private const string CharacterLocalisationPath = "Assets/Resources/DialogUtility/CharacterLocalisation/{0}.asset";
        
        public static void SaveLocalisationResource(LocalisationResource resource, string language, string containerName)
        {
            var path = string.Format(LocalisationPath, language, containerName);
            var data = resource;
            
            if(!AssetDatabase.Contains(data))
            {
                var splitPath = path.Split('/');
                var subpath = "Assets";

                for (int i = 1; i < splitPath.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(subpath+ "/" + splitPath[i]))
                    {
                        AssetDatabase.CreateFolder(subpath, splitPath[i]);
                    }

                    subpath += "/" + splitPath[i];
                }
                AssetDatabase.CreateAsset(resource, subpath);
            }
            else
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(data),
                    data.name);
            }
            
            
            AssetDatabase.SaveAssets();
        }

        
        public static void DeleteUnusedLocalisation(DialogGraphContainer container, LocalisationResource resource, LocalisationResource characterResource)
        {
            var keys = characterResource.texts
                .Where(x => ! CharacterList.Instance.globalCharacterDataList
                    .Exists(y =>y.id == x.Key)).Select(x=>x.Key)
                .ToList();
            foreach (var key in keys) 
            {
                characterResource.texts.Remove(key);
            }

            if (resource)
            {
                keys = resource.texts
                    .Where(x => !container.dialogNodeDataList
                        .Exists(y => y.Id == x.Key)).Select(x => x.Key)
                    .ToList();
                foreach (var key in keys)
                {
                    resource.texts.Remove(key);
                }
            }
            else
            {
                Debug.LogError("Trying to clean unused localisation from null resource");
            }
        }
        
        public static LocalisationResource LoadLocalisationResource(string containerName, string language)
        {
            var path = string.Format(LocalisationPath, language, containerName);
            var localisationData = AssetDatabase.LoadAssetAtPath<LocalisationResource>(path);

            if (!localisationData)
            {
                localisationData = ScriptableObject.CreateInstance<LocalisationResource>();
            }
            
            return localisationData;
        }

        public static LocalisationResource LoadCharacterLocalisationResource(string language)
        {
            var characterPath = string.Format(CharacterLocalisationPath, language);
            var characterLocalisationData = AssetDatabase.LoadAssetAtPath<LocalisationResource>(characterPath);

            if (!characterLocalisationData)
            {
                var splitPath = characterPath.Split('/');
                var subpath = "Assets";

                for (int i = 1; i < splitPath.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(subpath+ "/" + splitPath[i]))
                    {
                        AssetDatabase.CreateFolder(subpath, splitPath[i]);
                    }

                    subpath += "/" + splitPath[i];
                }
                characterLocalisationData = ScriptableObject.CreateInstance<LocalisationResource>();
                characterLocalisationData.name = language;
                AssetDatabase.CreateAsset(characterLocalisationData, subpath);
                AssetDatabase.SaveAssets();
            }
            EditorUtility.SetDirty(characterLocalisationData);
            return characterLocalisationData;
        }
        
        public static DialogUtilityLanguageSettings LoadLanguageSettings()
        {
            var data = AssetDatabase.LoadAssetAtPath<DialogUtilityLanguageSettings>(SettingsPath);

            if (!data)
            {
                var splitPath = SettingsPath.Split('/');
                var subpath = "Assets";
                
                for (int i = 1; i< splitPath.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(subpath + "/" + splitPath[i]))
                    {
                        AssetDatabase.CreateFolder(subpath, splitPath[i]);
                    }
                    subpath += "/" + splitPath[i];
                }
                
                data = ScriptableObject.CreateInstance<DialogUtilityLanguageSettings>();
                AssetDatabase.CreateAsset(data, subpath);
                AssetDatabase.SaveAssets();
            }

            return data;
        }

        public static void DeleteLocalisation(string containerName, string language)
        {
            var path = string.Format(LocalisationPath, language, containerName);
            AssetDatabase.DeleteAsset(path);
        }
        
        public static LocalisationResource CopyLocalisationResource(string language, string copyName, string originalName)
        {
            var p = string.Format(LocalisationPath, language, copyName);
            var p2 = string.Format(LocalisationPath, language, originalName);
            AssetDatabase.CopyAsset(p2, p);
            AssetDatabase.SaveAssets();
            var res = AssetDatabase.LoadAssetAtPath<LocalisationResource>(p);
            if (res) return res;
            Debug.LogError("Localisation resource copy failed!");
            return null;

        }
        
    }
}