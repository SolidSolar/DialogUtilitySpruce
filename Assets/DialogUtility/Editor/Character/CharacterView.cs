using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DialogUtilitySpruce.Editor
{
    public class CharacterView
    {
        private EventCallback<ChangeEvent<Object>> _spriteFieldCallback;
        private EventCallback<ChangeEvent<bool>> _toggleCallback;
        private EventCallback<ChangeEvent<string>> _textFieldCallback;
        private Action _deleteButtonCallback;
        private Action _confirmDeleteButtonCallback;
        private Action _rejectDeleteButtonCallback;

        private CharacterController _controller;
        private CharacterModel _model;
        
        private ObjectField _iconField;
        private TextField _textField;
        private Toggle _includeToggle;
        private Button _deleteButton;
        private Button _confirmDeleteButton;
        private Button _rejectDeleteButton;
        private VisualElement _view;

        public CharacterView()
        {
            _view = DialogGraphUXMLData.Instance.characterItem.CloneTree();
        }

        /// <summary>
        /// workaround for open issue
        /// https://forum.unity.com/threads/layout-update-is-struggling-to-process-current-layout.1277303/
        /// </summary>
        /// <returns></returns>
        public VisualElement GetView()
        {
            return _view;
        }
        
        public void SetItemData(CharacterModel model)
        {
            _model = model;
            _controller = new(_model);
            _iconField = _view.Q<ObjectField>("image");
            if(_spriteFieldCallback != null)
                _iconField.UnregisterCallback(_spriteFieldCallback);
            
            _spriteFieldCallback = evt =>
            {
                _controller.SetSprite((Sprite) evt.newValue);
                _iconField.Q<VisualElement>(classes: "unity-object-field-display").style.backgroundImage =
                    Background.FromSprite((Sprite) evt.newValue);
            };
            _iconField.RegisterCallback(_spriteFieldCallback);
            
            //setting value doesnt trigger callback here for some reason in some unity versions. 
            _iconField.value = _model.Icon;
            _iconField.Q<VisualElement>(classes: "unity-object-field-display").style.backgroundImage =
                Background.FromSprite(_model.Icon);
            
            _textField = _view.Q<TextField>("characterName");
            
            if(_textFieldCallback != null)
                _textField.UnregisterCallback(_textFieldCallback);
            
            _textFieldCallback = evt =>
            {
                if (_controller.SetText(evt.newValue)) return;
                EditorUtility.DisplayDialog("Error","Character name must be unique and can't be empty!", "Ok");
                _textField.SetValueWithoutNotify(_model.Name);
            };
            
            _textField.RegisterCallback(_textFieldCallback);
            _textField.SetValueWithoutNotify(_model.Name);
            _model.OnTextChanged = _ => _textField.SetValueWithoutNotify(_model.Name);
            
            _includeToggle = _view.Q<Toggle>("includeToggle");
            if(_toggleCallback != null)
                _includeToggle.UnregisterCallback(_toggleCallback);
                
            _toggleCallback = evt =>
            {
                bool decision = evt.newValue || EditorUtility.DisplayDialog(
                    title: "Remove character from local pool",
                    message: $"Character will be removed from local character pool, this action cannot be undone. Proceed anyway?",
                    ok: "Yes",
                    cancel: "No"
                );
                if(decision)
                    _controller.IncludeInCurrentDialog(evt.newValue);
                else
                {
                    _includeToggle.SetValueWithoutNotify(true);
                }
            };
            _includeToggle.RegisterCallback(_toggleCallback);
            
            
            _includeToggle.SetValueWithoutNotify(_model.IsIncludedInCurrentDialog());
            
            _deleteButton = _view.Q<Button>("delete");
            _confirmDeleteButton = _view.Q<Button>("deleteYes");
            _rejectDeleteButton = _view.Q<Button>("deleteNo");
            
            if(_deleteButtonCallback != null)
                _deleteButton.clicked -= _deleteButtonCallback;
            _deleteButtonCallback = _requestDelete;
            _deleteButton.clicked += _deleteButtonCallback;
            
            if(_confirmDeleteButtonCallback!=null)
                _confirmDeleteButton.clicked -= _confirmDeleteButtonCallback;
            _confirmDeleteButtonCallback = () =>
            {
                if (_controller.Delete()) return;
                _rejectDeleteButtonCallback.Invoke();
            };
            _confirmDeleteButton.clicked += _confirmDeleteButtonCallback;
            
            if(_rejectDeleteButtonCallback!=null)
                _rejectDeleteButton.clicked -= _rejectDeleteButtonCallback;

            _rejectDeleteButtonCallback = _rejectDelete;
            _rejectDeleteButton.clicked += _rejectDeleteButtonCallback;
        }

        private void _requestDelete()
        {
            _deleteButton.style.visibility = Visibility.Hidden;
            _deleteButton.style.overflow = Overflow.Hidden;
            _deleteButton.style.display = DisplayStyle.None;
            _confirmDeleteButton.style.visibility = Visibility.Visible;
            _confirmDeleteButton.style.overflow = Overflow.Visible;
            _confirmDeleteButton.style.display = DisplayStyle.Flex;
            _rejectDeleteButton.style.visibility = Visibility.Visible;
            _rejectDeleteButton.style.overflow = Overflow.Visible;
            _rejectDeleteButton.style.display = DisplayStyle.Flex;
        }
        
        private void _rejectDelete()
        {
            _deleteButton.style.visibility = Visibility.Visible;
            _deleteButton.style.overflow = Overflow.Visible;
            _deleteButton.style.display = DisplayStyle.Flex;
            _confirmDeleteButton.style.visibility = Visibility.Hidden;
            _confirmDeleteButton.style.overflow = Overflow.Hidden;
            _confirmDeleteButton.style.display = DisplayStyle.None;
            _rejectDeleteButton.style.visibility = Visibility.Hidden;
            _rejectDeleteButton.style.overflow = Overflow.Hidden;
            _rejectDeleteButton.style.display = DisplayStyle.None;
        }
    }
}