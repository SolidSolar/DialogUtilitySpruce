using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class DialogNodeModel
    {
        public DialogNodeModel(DialogNodeData data)
        {
            DialogLanguageHandler languageHandler = DialogLanguageHandler.Instance;
            CharacterList characterList = CharacterList.Instance;
            languageHandler.OnLanguageChanged += _ =>
            {
                var oldRes = _resource; 
                _resource = languageHandler.GetLocalisationResource();
                if (!_resource.texts.ContainsKey(Id))
                {
                    if (oldRes is not null) _resource.texts[Id] = oldRes.texts[Id];
                    else
                    {
                        Debug.LogWarning("loading old localisation failed!");
                        _resource.texts[Id] = "";
                    }
                }
            };
            _data = data;
            CharacterModel characterModel = characterList.FindCharacter(data.characterId);
            if(characterModel!= null)
                Character = characterModel;
            _resource = languageHandler.GetLocalisationResource();
            if (!_resource.texts.ContainsKey(Id))
            {
                _resource.texts[Id] = "";
            }
            
            characterList.OnLocalListChanged += () =>
            {
                if (characterModel!=null && !characterList.GetLocalCharacterNames().Contains(characterModel.Name))
                {
                    characterModel = null;
                }
            };
        }

        public DialogNode View { get; set; }
        public Action<CharacterModel> OnCharacterUpdate { get; set; }
        public Action<string> OnTextUpdate { get; set; }
        public Action<Sprite> OnSpriteUpdate { get; set; }
        public Action<List<PortConditionData>> OnPortsUpdate { get; set; }
        public SerializableGuid Id => _data.id;
        public CharacterModel Character
        {
            get => _character;
            set
            {
                _character = value;
                _data.characterId = _character.Id;
                OnCharacterUpdate?.Invoke(_character);
            }
        }
        public string Text
        {
            get =>_resource.texts[Id];
            set
            {
                _resource.texts[Id] = value;
                OnTextUpdate?.Invoke(value);
            }
        }

        public Sprite OverrideSprite
        {
            get => _data.sprite;
            set
            {
                _data.sprite = value;
                OnSpriteUpdate?.Invoke(value);
            }
        }

        public List<PortConditionData> Ports => _data.ports;

        public void AddConditionPort()
        {
            Ports.Add(new PortConditionData());
            OnPortsUpdate?.Invoke(Ports);
        }

        public void RemoveConditionPort(SerializableGuid id)
        {
            Ports.RemoveAll(x => x.id == id);
            OnPortsUpdate?.Invoke(Ports);
        }

        public void SetCondition(SerializableGuid portId, Condition condition)
        {
            Ports.Find(x => x.id == portId).condition = condition;
        }
        
        public DialogNodeData GetDialogNodeData()
        {
            return _data;
        }

        public void SetPosition(Vector2 newPos)
        {
            _data.position = newPos;
        }
        
        private readonly DialogNodeData _data;
        private CharacterModel _character;
        private LocalisationResource _resource;
    }
}