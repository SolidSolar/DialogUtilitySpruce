using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogUtilitySpruce.Editor
{
    public class DialogUtilityWindow : EditorWindow
    {
        private string ContainerName
        {
            get => _graphContainer.name;
            set => _graphContainer.name = value;
        }
        private DialogUtilitySaveUtility SaveUtility => DialogUtilitySaveUtility.GetDialogUtilitySaveUtilityInstance(_graphView);

        private string _fileName;
        private DialogGraphView _graphView;
        private DialogGraphContainer _graphContainer;
        private VisualElement _characterListView;
        private VisualElement _window;
        private DropdownField _languageDropdown;

        private const string ManageLanguages = "Manage languages...";
        [MenuItem("Window/DialogUtilitySpruce")]
        public static DialogUtilityWindow OpenDialogUtilityWindow()
        {
            var window = GetWindow<DialogUtilityWindow>();
            _initialize(window);
            return window;
        }

        public static DialogUtilityWindow OpenDialogUtilityWindow(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                EditorUtility.DisplayDialog("Invalid filename: "+ filename, "Enter valid filename and try again", "ok");
                return null;
            }
            
            var window = GetWindow<DialogUtilityWindow>();
            window._fileName = filename;
            _initialize(window);
            return window;
        }

        private void OnDisable()
        {
            if (_graphView != null)
            {
                rootVisualElement.Remove(_window);
            }
        }

        private void _saveGraph()
        {
            if (string.IsNullOrEmpty(ContainerName))
            {
                EditorUtility.DisplayDialog("Invalid filename: "+ ContainerName, "Enter valid filename and try again", "ok");
                return;
            }
            SaveUtility.Save(ContainerName);
        }

        private void _loadGraph()
        {
            _graphContainer = SaveUtility.Load(_fileName);
        }
        
        private void _constructWindow()
        {
            _window =  DialogGraphUXMLData.Instance.dialogGraphWindow.CloneTree();
            _graphView = _window.Q<DialogGraphView>("graphView");
            _characterListView = _window.Q<VisualElement>("charactersListContainer");
            var fileNameField = _window.Q<TextField>("fileName");
            if (string.IsNullOrEmpty(_fileName))
            {
                fileNameField.SetValueWithoutNotify("");
                fileNameField.RegisterValueChangedCallback((a) => { ContainerName = a.newValue; });
            }
            else
            {
                fileNameField.SetValueWithoutNotify(_fileName);
                fileNameField.RegisterValueChangedCallback((a) => { ContainerName = a.newValue; });
            }

            var addNodeButton = _window.Q<Button>("addNode");
            addNodeButton.clicked += () => { _graphView.AddNode(); };
            
            var saveButton = _window.Q<Button>("save");
            saveButton.clicked += _saveGraph;

            _languageDropdown = _window.Q<DropdownField>("language");
           
            _window.StretchToParentSize();
            rootVisualElement.Add(_window);
        }

        private void _setLanguage()
        {
            var languageHandler = DialogLanguageHandler.Instance;
            _languageDropdown.choices = languageHandler.GetLanguagesList();
            _languageDropdown.choices.Add(ManageLanguages);
            _languageDropdown.RegisterCallback<PointerDownEvent>(_ =>
            {
                _languageDropdown.choices = languageHandler.GetLanguagesList();
                _languageDropdown.choices.Add(ManageLanguages);
            });
            _languageDropdown.RegisterCallback<ChangeEvent<string>>(c=>
            {
                if(c.newValue!=ManageLanguages){
                    languageHandler.SetLanguage(c.newValue);
                }else{
                    _languageDropdown.SetValueWithoutNotify(languageHandler.Language);
                    DialogLanguageHandler.Instance.SelectLanguageSettings();
                }
            });
            _languageDropdown.SetValueWithoutNotify(languageHandler.Language);
        }
        
        private static void _initialize(DialogUtilityWindow window)
        {
            window.minSize = new Vector2(600f, 600f);
            if (window._graphView!=null)
            {
                window.Close();
                window = GetWindow<DialogUtilityWindow>();
            }
            window.titleContent = new GUIContent("DialogUtilitySpruce");
            window._constructWindow();
            window._loadGraph();
            window._characterListView.Add(CharactersListViewFactory.GetList());
            window._setLanguage();
        }
    }
}