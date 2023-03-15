using UnityEngine;
using UnityEngine.UIElements;

namespace DialogUtilitySpruce.Editor
{
    public class DialogNodeController
    {
        public DialogNodeController(DialogNodeModel model)
        {
            _model = model;
        }
        
        private readonly DialogNodeModel _model;

        public void SetPosition(Rect newPos)
        {
            _model.SetPosition(newPos);
        }
       
        
        public void ChangeText(ChangeEvent<string> @event)
        {
            _model.Text = @event.newValue;
        }
        
        public void ChangeSprite(ChangeEvent<Object> @event)
        {
            _model.Sprite = (Sprite)@event.newValue;
        }

        public void ChangeCharacter(string characterName)
        {
            _model.Character = CharacterList.Instance.FindCharacter(characterName);
        }

        public void AddConditionPort()
        {
            _model.AddConditionPort();
        }

        public void RemoveConditionPort(SerializableGuid id)
        {
            _model.RemoveConditionPort(id);
        }

        public void SetCondition(SerializableGuid id, Condition value)
        {
            _model.SetCondition(id, value);
        }
    }
}