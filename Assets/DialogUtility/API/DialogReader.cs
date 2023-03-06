using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DialogUtilitySpruce.API
{
    public class DialogReader : MonoBehaviour
    {
        public UnityEvent<DialogNode> OnNextMessageShown { get; } = new();
        public UnityEvent<DialogNode> OnDialogEnded { get; } = new();
        public UnityEvent<DialogNode> OnDialogStarted { get; } = new();
        public Dictionary<DialogNode, Dictionary<DialogChoiceOption, DialogNode>> Graph { get; private set; }
        
        public bool IsActive { get; private set; }
        
        private DialogGraphContainer _container;
        private DialogNodeFactory _nodeFactory;
        private DialogNode _currentNode;
        private DialogNode _startNode;
        
        private const string LocalisationPath = "Assets/Resources/DialogUtility/ContainerLocalisation/{0}/{1}.asset";
        private const string CharacterLocalisationPath = "Assets/Resources/DialogUtility/CharacterLocalisation/{0}.asset";
        private void Awake()
        {
            DialogReaderSettings.Init(DialogReaderSettings.DefaultLanguage, AssetDatabase
                    .LoadAssetAtPath<LocalisationResource>(string.Format(CharacterLocalisationPath, DialogReaderSettings.DefaultLanguage)));
        }

        /// <summary>
        /// Loads dialog graph container into reader and loads localisation for it.
        /// </summary>
        /// <param name="container"></param>
        public void BeginDialog(DialogGraphContainer container)
        {
            if (IsActive)
            {
                Debug.LogWarning("You are trying to begin dialog already begun");
                return;
            }

            _container = container;
            LocalisationResource localisationResource = 
                AssetDatabase.LoadAssetAtPath<LocalisationResource>(string.Format(LocalisationPath, DialogReaderSettings.Language ,_container.name));
            _container.localisationResource = localisationResource;
            _nodeFactory = new DialogNodeFactory(container);
            
            _load();

            _currentNode = _startNode;
            IsActive = true;
            OnDialogStarted?.Invoke(_currentNode);
        }

        /// <summary>
        /// Dialog Reader iterates to next message in a dialog tree, calling OnNextMessageShown with current dialog node as parameter
        /// </summary>
        /// <param name="choice">Choice used to navigate in dialog tree</param>
        public void NextMessage(DialogChoiceOption choice = null)
        {
            if (!IsActive)
            {
                Debug.LogWarning("You are trying to show next message for dialog not begun or reset");
                return;
            }
            
            OnNextMessageShown?.Invoke(_currentNode);
            
            if (Graph[_currentNode].Count == 0)
            {
                EndDialog();
                return;
            }
            
            if (choice == null)
            {
                var viableOptions = Graph[_currentNode].Keys
                    .Count(option => !option.Condition || option.Condition.IsTrue(option.Index));
                choice = viableOptions > 0 ? Graph[_currentNode].Keys.First(x => !x.Condition || x.Condition.IsTrue(x.Index)) : null;
                
                if (choice == null)
                {
                    EndDialog();
                    return;
                }
            }
            _currentNode = Graph[_currentNode][choice];
        }

        /// <summary>
        /// Ends dialog and calls OnDialogEnded with last DialogNode as a parameter.
        /// </summary>
        public void EndDialog()
        {
            if (!IsActive)
            {
                Debug.LogWarning("You are trying to end dialog not begun or reset");
                return;
            }
            IsActive = false;
            OnDialogEnded?.Invoke(_currentNode);
        }

        private void _load()
        {
            Graph = new Dictionary<DialogNode, Dictionary<DialogChoiceOption, DialogNode>>();
            var nodeDictionary = new Dictionary<SerializableGuid, Tuple<DialogNode, DialogNodeData>>();
            _container.dialogNodeDataList.ForEach(x =>
            {
                var data = x.GetDataCopy(_container.localisationResource);
                nodeDictionary.Add(x.Id, new Tuple<DialogNode, DialogNodeData>(_nodeFactory.GetDialogNode(data), data)); 
            });
            nodeDictionary.Values.ToList().ForEach(node =>
            {
                Graph.Add(node.Item1, new Dictionary<DialogChoiceOption, DialogNode>());
                if (node.Item2.id == _container.startNodeId)
                    _startNode = node.Item1;

                foreach (NodeLinkData link in _container.nodeLinks)
                {
                    for (int j = 0; j < node.Item2.ports.Count; j++)
                    {
                        if (link.basePortID == node.Item2.ports[j].id)
                        {
                            Graph[node.Item1][node.Item1.ChoiceOptions[j]] = nodeDictionary[link.targetNodeID].Item1;
                        }
                    }
                }
            });
        }
    }
}