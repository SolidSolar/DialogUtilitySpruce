using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class DialogLanguageHandler
    {
        public DialogLanguageHandler(DialogGraphContainer container)
        {
            _container = container;
            _load();
        }
        
        public static DialogLanguageHandler Instance { get; set; }
        
        public Action<string> OnLanguageChanged;
        public string Language;

        private DialogGraphContainer _container;
        private readonly Dictionary<string, LocalisationResource> _resource = new();
        private LocalisationResource _characterResource;
        private DialogUtilityLanguageSettings _languageSettings;

        public void SelectLanguageSettings()
        {
            Selection.objects = new[] {_languageSettings};
        }

        
        
        public void Save(DialogGraphContainer container)
        {
            _container = container;
            
            var currentLanguage = Language;
            foreach (var lang in _languageSettings.Languages)
            {
                Language = lang;
                if (_resource.ContainsKey(Language))
                {
                    LocalisationResourceSaveUtility.DeleteUnusedLocalisation(_container, _resource[Language], _characterResource);
                    LocalisationResourceSaveUtility.SaveLocalisationResource(_resource[Language], Language, _container.name);
                }
            }

            Language = currentLanguage;
        }
        
        public void SetLanguage(string language)
        {
            _languageSettings.CurrentLanguageIndex = _languageSettings.Languages.FindIndex(x=>x ==language);
            Language = language;
            if (!_resource.ContainsKey(Language))
            {
                _resource[Language] = LocalisationResourceSaveUtility.LoadLocalisationResource(_container.name, Language);
            }

            _characterResource = LocalisationResourceSaveUtility.LoadCharacterLocalisationResource(Language);
            OnLanguageChanged?.Invoke(Language);
        }

        public LocalisationResource GetLocalisationResource()
        {
            return _resource[Language];
        }
        
        public LocalisationResource GetCharacterLocalisationResource()
        {
            return _characterResource;
        }

        public List<string> GetLanguagesList()
        {
            return new List<string>(_languageSettings.Languages);
        }

        public void DeleteAllRelatedLocalisation(string containerName)
        {
            foreach (var lang in GetLanguagesList())
            {
                LocalisationResourceSaveUtility.DeleteLocalisation(containerName, lang);
            }
        }

        public void CreateLocalisationResourceCopy(DialogGraphContainer copy, DialogGraphContainer original)
        {
            Dictionary<string, LocalisationResource> resource = new();
            var currentLanguage = Language;
            foreach (var lang in _languageSettings.Languages)
            {
                resource[lang] =
                    LocalisationResourceSaveUtility.CopyLocalisationResource(Language, copy.name, original.name);
                Language = lang;
                if (_resource.ContainsKey(Language))
                {
                    var old = _resource[lang];
                    _resource[lang] = resource[lang];
                    LocalisationResourceSaveUtility.SaveLocalisationResource(_resource[Language], Language, copy.name);
                    _resource[lang] = old;
                }
            }

            Language = currentLanguage;
            AssetDatabase.SaveAssets();
        }
        
        private void _load()
        {
            _languageSettings = LocalisationResourceSaveUtility.LoadLanguageSettings();
            
            if (_languageSettings.Languages.Count == 0)
            {
                _languageSettings.Languages.Add(DialogUtilityLanguageSettings.DefaultLanguage);
            }

            Language = _languageSettings.CurrentLanguageIndex < _languageSettings.Languages.Count
                ? _languageSettings.Languages[_languageSettings.CurrentLanguageIndex] : _languageSettings.Languages[0];

            _resource[Language] = LocalisationResourceSaveUtility.LoadLocalisationResource(_container.name, Language);
            _characterResource = LocalisationResourceSaveUtility.LoadCharacterLocalisationResource(Language);
        }

    }
}