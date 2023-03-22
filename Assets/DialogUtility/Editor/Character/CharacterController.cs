using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class CharacterController
    {
        public CharacterController(CharacterModel model)
        {
            _model = model;
        }

        private DialogGraphContainer _container => DialogUtilityUsagesHandler.Instance.CurrentContainer;
        
        public bool Delete()
        {
            return _model.DeleteSelf();
        }

        public void IncludeInCurrentDialog(bool include)
        {
            if (include)
            {
                CharacterList.Instance.AddCharacterToLocal(_model);
                _model.Usages.Add(_container.id);
            }
            else
            {
                CharacterList.Instance.RemoveCharacterFromLocal(_model);
                _model.Usages.Remove(_model.Usages.Find(x=>x == _container.id));
            }
        }

        public void SetSprite(Sprite sprite)
        {
            _model.Icon = sprite;
            CharacterList.Instance.OnCharacterChanged?.Invoke(_model);
        }

        public bool SetText(string text)
        {
            var characterList = CharacterList.Instance;
            if (!string.IsNullOrWhiteSpace(text) && !characterList.GetGlobalCharacterNames().Contains(text))
            {
                _model.Name = text;
                characterList.OnCharacterChanged?.Invoke(_model);
                return true;
            }

            return false;
        }
        
        private CharacterModel _model;
    }
}