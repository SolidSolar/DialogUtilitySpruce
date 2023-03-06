using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class CharacterModel
    {
        public CharacterModel(CharacterData data = null)
        {
            _data = data ?? new CharacterData();
            DialogLanguageHandler languageHandler = DialogLanguageHandler.Instance;
            languageHandler.OnLanguageChanged += _ =>
            {
                var oldRes = _data.resource;
                _data.resource = languageHandler.GetCharacterLocalisationResource();
                if (!_data.resource.texts.ContainsKey(Id))
                {
                    _data.resource.texts[Id] = oldRes.texts[Id];
                }
                OnTextChanged?.Invoke(Name);
            };
            _data.resource = languageHandler.GetCharacterLocalisationResource();
            if (!_data.resource.texts.ContainsKey(Id))
            {
                _data.resource.texts[Id] = "Unnamed_" + Guid.NewGuid().ToString().Substring(0, 6);
            }

            _data.usages = usagesHandler.UpdateCharacterUsages(_data.id, _data.usages);
        }

        public Action OnDelete { get; set; }
        public Action<string> OnTextChanged;
        public SerializableGuid Id => _data.id;
        public string Name
        {
            get =>_data.resource.texts[Id];
            set
            {
                _data.resource.texts[Id] = value;
                OnTextChanged?.Invoke(value);
            }
        }

        public Sprite Icon
        {
            get => _data.icon;
            set => _data.icon = value;
        }
        
        public List<SerializableGuid> Usages => _data.usages;

        private DialogUtilityUsagesHandler usagesHandler => DialogUtilityUsagesHandler.Instance;
        private CharacterData _data;

        public CharacterData GetCharacterData()
        {
            return _data;
        }

        public bool IsIncludedInCurrentDialog()
        {
            return CharacterList.Instance.GetLocalCharacterNames().Exists(x => x == Name);
        }

        public bool DeleteSelf()
        {
            _data.usages = usagesHandler.UpdateCharacterUsages(Id, Usages);
            if (CharacterList.Instance.DeleteCharacter(this))
            {
                OnDelete?.Invoke();
                return true;
            }

            return false;
        }
    }
}