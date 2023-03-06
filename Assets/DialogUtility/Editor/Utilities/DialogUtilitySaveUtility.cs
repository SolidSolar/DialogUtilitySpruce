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
        
        private List<DialogNode> Nodes => _graphView.nodes.ToList().Cast<DialogNode>().ToList();
        private List<Edge> Edges => _graphView.edges.ToList();
        private DialogGraphView _graphView;
        private DialogGraphContainer _graphContainer;
        private DialogUtilityUsagesHandler _usagesHandler = DialogUtilityUsagesHandler.Instance;
        private const string Path = "Assets/Resources/DialogUtility/Containers/{0}.asset";
        
        public void Save(string filename)
        {
            var path = string.Format(Path, filename);
            var dialogGraphContainer = _usagesHandler.CurrentContainer;
            if (!AssetDatabase.Contains(dialogGraphContainer))
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
                
                dialogGraphContainer = ScriptableObject.CreateInstance<DialogGraphContainer>();
                AssetDatabase.CreateAsset(dialogGraphContainer, subpath);
                AssetDatabase.SaveAssets();
            }else
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(dialogGraphContainer),
                        dialogGraphContainer.name);
            }
            
            dialogGraphContainer.dialogNodeDataList.RemoveAll(x =>
            {
                return !Nodes.Exists(y => y.Model.Id == x.Id);
            });
            dialogGraphContainer.nodeLinks.Clear();
            
            var connectedPorts = Edges.Where(x => x.input.node != null||x.output.node!=null).ToArray();
            foreach (Edge edge in connectedPorts)
            {
                var output = (DialogNode) edge.output.node;
                var input = (DialogNode) edge.input.node;
                dialogGraphContainer.nodeLinks.Add(new NodeLinkData { 
                    baseNodeID = output.Model.Id,
                    basePortID = Guid.Parse(edge.output.Q<Port>().name),
                    targetNodeID = input.Model.Id
                });
            }
            
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(dialogGraphContainer));
            if (assets.Length > 0)
            {
                foreach (var v in assets)
                {
                    if (v is null || v is DialogNodeDataContainer dataContainer && 
                        !dialogGraphContainer.dialogNodeDataList.Exists(x=>x.Id == dataContainer.Id))
                    {
                        Object.DestroyImmediate(v, true);
                    }
                }
            }
            
            foreach(var item in Nodes)
            {
                DialogNodeDataContainer dataContainer = dialogGraphContainer.dialogNodeDataList.Find(x => x.Id == item.Model.Id);
                if (dataContainer == null)
                {
                    dataContainer = ScriptableObject.CreateInstance<DialogNodeDataContainer>();
                    dialogGraphContainer.dialogNodeDataList.Add(dataContainer);
                    AssetDatabase.AddObjectToAsset(dataContainer, dialogGraphContainer);
                }
                dataContainer.SetData(item.Model.GetDialogNodeData());
            }
            dialogGraphContainer.localisationResource = DialogLanguageHandler.Instance.GetLocalisationResource();
            dialogGraphContainer.startNodeId = _graphView.StartNodeId;
            dialogGraphContainer.characterList = CharacterList.Instance.GetLocalCharactersListCopy();
            DialogLanguageHandler.Instance.Save(dialogGraphContainer);
            _usagesHandler.CurrentContainer = dialogGraphContainer;
            _usagesHandler.UpdateDictionaryOfIdsAndContainers();
            AssetDatabase.SaveAssets();
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

            _clearGraph();
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
            //copy dialog file situation
            if (DialogUtilityUsagesHandler.Instance.IsCopy(_graphContainer, out DialogGraphContainer original))
            {
                _processCopy(_graphContainer, original);
            }
            _graphContainer.localisationResource = DialogLanguageHandler.Instance.GetLocalisationResource();
            DialogUtilityUsagesHandler.Instance.UpdateDictionaryOfIdsAndContainers();
            
            CharacterList.Instance.UpdateLocalList(_graphContainer.characterList);
            _graphView.StartNodeId = _graphContainer.startNodeId;
            _createNodes();
            _connectNodes();
            return _graphContainer;
        }

        private void _processCopy(DialogGraphContainer copy, DialogGraphContainer original)
        {
            copy.id = Guid.NewGuid();
            foreach (var data in CharacterList.Instance.globalCharacterDataList)
            {
                data.usages.Add(copy.id); 
            }
            
            DialogLanguageHandler.Instance.CreateLocalisationResourceCopy(copy, original);
        }

        private void _connectNodes()
        {
            foreach (DialogNode node in Nodes)
            {
                var connections = _graphContainer.nodeLinks.Where(x => x.baseNodeID == node.Model.Id).ToList();
                foreach (NodeLinkData link in connections)
                {
                    var targetNodeGuid = link.targetNodeID;
                    var targetNode = Nodes.First(x => x.Model.Id == targetNodeGuid);
                    var tmpEdge = new Edge
                    {
                        output = node.outputContainer.Q<Port>(link.basePortID.Value),
                        input = targetNode.inputContainer.Q<Port>()
                    };
                    tmpEdge.input.Connect(tmpEdge);
                    tmpEdge.output.Connect(tmpEdge);
                    _graphView.Add(tmpEdge);
                }
            }
        }
        
        private void _createNodes()
        {
            foreach (var nodeData in _graphContainer.dialogNodeDataList)
            {
                var data = nodeData.GetDataCopy(_graphContainer.localisationResource);
                var tmpNode = _graphView.AddNode(data);
                tmpNode.SetPosition(new Rect(data.position, Vector2.zero));
                _graphView.AddElement(tmpNode);
            }
        }

        private void _clearGraph()
        {
            foreach (var node in Nodes)
            {
                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _graphView.Remove(edge));
                _graphView.RemoveElement(node);
            }
        }
    }
}