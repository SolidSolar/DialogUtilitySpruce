using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class DialogNodeModel
    {
        public DialogNodeModel(DialogNodeDataContainer data)
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
            _dataContainer = data;
            _character = characterList.FindCharacter(Data.characterId);
            _resource = languageHandler.GetLocalisationResource();
            if (!_resource.texts.ContainsKey(Id))
            {
                _resource.texts[Id] = "";
            }
            
            characterList.OnLocalListChanged += () =>
            {
                if (_character!=null && !characterList.GetLocalCharacterNames().Contains(_character.Name))
                {
                    _character = null;
                }
            };

            Undo.undoRedoPerformed += () =>
            {
                OnTextUpdate?.Invoke(Text);
                _character = CharacterList.Instance.FindCharacter(Data.characterId);
                OnCharacterUpdate?.Invoke(_character);
                OnSpriteUpdate?.Invoke(Sprite);
                OnPortsUpdate?.Invoke(Ports);
            };
        }

        public DialogNode View { get; set; }
        public Action<CharacterModel> OnCharacterUpdate { get; set; }
        public Action<string> OnTextUpdate { get; set; }
        public Action<Sprite> OnSpriteUpdate { get; set; }
        public Action<List<PortConditionData>> OnPortsUpdate { get; set; }
        public SerializableGuid Id => Data.id;
        public CharacterModel Character
        {
            get => _character;
            set
            {
                Undo.RecordObject(_dataContainer, "Set node character");
                _character = value;
                Data.characterId = _character?.Id ?? Guid.Empty;
                OnCharacterUpdate?.Invoke(_character);
            }
        }
        public string Text
        {
            get => _resource.GetText(Id);
            set
            {
                Undo.RecordObject(_resource, "Set node text");
                _resource.texts[Id] = value;
                
                OnTextUpdate?.Invoke(value);
            }
        }

        public Sprite Sprite
        {
            get => Data.sprite;
            set
            {
                Undo.RecordObject(_dataContainer, "Set node sprite");
                Data.sprite = value;
                OnSpriteUpdate?.Invoke(value);
            }
        }

        public List<PortConditionData> Ports => Data.ports;

        public void AddConditionPort()
        {
            Undo.RecordObject(_dataContainer, "Add node port");
            Ports.Add(new PortConditionData());
            OnPortsUpdate?.Invoke(Ports);
        }

        public void RemoveConditionPort(SerializableGuid id)
        {
            Undo.RecordObject(_dataContainer, "Remove node port");
            Ports.RemoveAll(x => x.id == id);
            OnPortsUpdate?.Invoke(Ports);
        }

        public void SetCondition(SerializableGuid portId, Condition condition)
        {
            Ports.Find(x => x.id == portId).condition = condition;
        }
        
        public DialogNodeData GetDialogNodeData()
        {
            return Data;
        }

        public void SetPosition(Rect newPos)
        {
            Data.position = newPos.position;
        }

        private DialogNodeData Data => _dataContainer.GetData();
        
        private readonly DialogNodeDataContainer _dataContainer;
        private CharacterModel _character;
        private LocalisationResource _resource;
    }
}