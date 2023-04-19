using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DialogUtilitySpruce.Editor
{
    public class DialogUtilitySaveUtility
    {
        public static DialogUtilitySaveUtility GetDialogUtilitySaveUtilityInstance(DialogGraphView graphView)
        {
            return new DialogUtilitySaveUtility
            {
                _graphView = graphView
            };
        }

        private DialogGraphContainer _virtualContainer;
        private List<DialogNode> Nodes => _graphView.nodes.ToList().Cast<DialogNode>().ToList();
        private List<Edge> Edges => _graphView.edges.ToList();
        private DialogGraphView _graphView;
        private DialogGraphContainer _graphContainer;
        private DialogUtilityUsagesHandler _usagesHandler => DialogUtilityUsagesHandler.Instance;
        private const string Path = "Assets/Resources/DialogUtility/Containers/{0}.asset";
        
        public void Save(string filename)
        {
            var path = string.Format(Path, filename);
            if (!AssetDatabase.Contains(_graphContainer))
            {
                var splitPath = path.Split('/');
                var subpath = "Assets";
                
                for (int i = 1; i< splitPath.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(subpath + "/" +splitPath[i]))
                    {
                        AssetDatabase.CreateFolder(subpath, splitPath[i]);
                    }
                    subpath += "/" + splitPath[i];
                }
                
                _graphContainer = ScriptableObject.CreateInstance<DialogGraphContainer>();
                AssetDatabase.CreateAsset(_graphContainer, subpath);
                AssetDatabase.SaveAssets();
            }else
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_graphContainer),
                    _virtualContainer.name);
            }
            EditorUtility.SetDirty(_graphContainer);
            DialogGraphContainer.Copy(_virtualContainer,_graphContainer);
            
            //clear deleted nodes
            _graphContainer.dialogNodeDataList.RemoveAll(x =>
            {
                return !Nodes.Exists(y => y.Model.Id == x.Id);
            });
            _graphContainer.nodeLinks.Clear();
            
            var connectedPorts = Edges.Where(x => x.input.node != null||x.output.node!=null).ToArray();
            foreach (Edge edge in connectedPorts)
            {
                var output = (DialogNode) edge.output.node;
                var input = (DialogNode) edge.input.node;
                _graphContainer.nodeLinks.Add(new NodeLinkData { 
                    baseNodeID = output.Model.Id,
                    basePortID = Guid.Parse(edge.output.Q<Port>().name),
                    targetNodeID = input.Model.Id
                });
            }
            
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_graphContainer));
            if (assets.Length > 0)
            {
                foreach (var v in assets)
                {
                    if (v is null || v is DialogNodeDataContainer dataContainer && 
                        !_graphContainer.dialogNodeDataList.Exists(x=>x.Id == dataContainer.Id))
                    {
                        Object.DestroyImmediate(v, true);
                    }
                }
            }
            
            foreach(var item in Nodes)
            {
                DialogNodeDataContainer dataContainer = _graphContainer.dialogNodeDataList.Find(x => x.Id == item.Model.Id);
                if (dataContainer == null)
                {
                    dataContainer = ScriptableObject.CreateInstance<DialogNodeDataContainer>();
                    _graphContainer.dialogNodeDataList.Add(dataContainer);
                    AssetDatabase.AddObjectToAsset(dataContainer, _graphContainer);
                }else if (!AssetDatabase.Contains(dataContainer))
                {
                    AssetDatabase.AddObjectToAsset(dataContainer, _graphContainer);
                }
                var dialogData = item.Model.GetDialogNodeData();
                dataContainer.SetData(dialogData);
            }
            _graphContainer.localisationResource = DialogLanguageHandler.Instance.GetLocalisationResource();
            _graphContainer.characterList = CharacterList.Instance.GetLocalCharactersListCopy();
            DialogLanguageHandler.Instance.Save(_graphContainer);
            _usagesHandler.CurrentContainer = _graphContainer;
            _usagesHandler.UpdateDictionaryOfIdsAndContainers();
            AssetDatabase.SaveAssets();
            EditorUtility.ClearDirty(_graphContainer);
        }
        
        public DialogGraphContainer Load(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var path = string.Format(Path, filename);
                _graphContainer = AssetDatabase.LoadAssetAtPath<DialogGraphContainer>(path);
                if (!_graphContainer)
                {
                    EditorUtility.DisplayDialog("File not found: " + filename, "File might not exist", "Ok");
                }
            }
            else
            {
                _graphContainer = ScriptableObject.CreateInstance<DialogGraphContainer>();
            }

            var handler = AssetDatabase.LoadAssetAtPath<DialogUtilityUsagesHandler>(DialogUtilityUsagesHandler.DialogUtilityUsagesHandlerPath);
            if (!handler)
            {
                handler = DialogUtilityUsagesHandler.CreateDialogUtilityUsagesHandler();
            }

            DialogUtilityUsagesHandler.Instance = handler;
            DialogUtilityUsagesHandler.Instance.CurrentContainer = _graphContainer;
            DialogLanguageHandler.Instance = new DialogLanguageHandler(_graphContainer);
            var charList = AssetDatabase.LoadAssetAtPath<CharacterList>(CharacterList.CharacterListPath);
            if (!charList)
            {
                charList = CharacterList.CreateCharacterList();
            }
            CharacterList.Instance = charList;
            DialogUtilityUsagesHandler.Instance.ProcessIfCopy(_graphContainer);

            if (!_virtualContainer)
            {
                _virtualContainer = ScriptableObject.CreateInstance<DialogGraphContainer>();
            }
            DialogGraphContainer.Copy(_graphContainer, _virtualContainer);
            _virtualContainer.localisationResource = DialogLanguageHandler.Instance.GetLocalisationResource();
            DialogUtilityUsagesHandler.Instance.UpdateDictionaryOfIdsAndContainers();
            
            CharacterList.Instance.UpdateLocalList(_virtualContainer.characterList);
            
            return _virtualContainer;
        }
    }
}