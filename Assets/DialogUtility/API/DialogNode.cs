using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogUtilitySpruce.API
{
    [Serializable]
    public class DialogNode
    {
        public DialogNode(DialogNodeData data, CharacterData character = null)
        {
            this.data = data;
            Character = character;
            _choiceOptions = new List<DialogChoiceOption>();
            for (int i = 0; i < data.ports.Count; i++)
            {
                _choiceOptions.Add(new DialogChoiceOption(data.ports[i], i));
            }
        }

        public CharacterData Character { get; private set; }
        public string Text => data.text;
        public Sprite Sprite => data.sprite;
        public List<DialogChoiceOption> ChoiceOptions => _choiceOptions;
        
        [SerializeField]
        private DialogNodeData data;
        [SerializeField]
        private List<DialogChoiceOption> _choiceOptions;
    }

    public class DialogNodeFactory
    {
        private readonly DialogGraphContainer _container;

        public DialogNodeFactory(DialogGraphContainer container)
        {
            _container = container;
        }
        
        public DialogNode GetDialogNode(DialogNodeData data)
        {
            CharacterData character = null;
            if (data.characterId)
            {
                character = _container.characterList.Find(x => x.id == data.characterId);
                if (character == null)
                {
                    Debug.LogError($"character with id {data.characterId} doesnt exist");
                }
            }

            if (character != null)
            {
                character.resource = DialogReaderSettings.CharacterLocalisation;
            }

            var dialogNode = new DialogNode(data, character);
            return dialogNode;
        }
    }

    [Serializable]
    public class DialogChoiceOption
    {
        public DialogChoiceOption(PortConditionData data, int index)
        {
            this.data = data;
            this.index = index;
        }

        public int Index => index;
        public Condition Condition => data.condition;
        
        [SerializeField]
        private PortConditionData data;
        [SerializeField]
        private int index;
    }

}