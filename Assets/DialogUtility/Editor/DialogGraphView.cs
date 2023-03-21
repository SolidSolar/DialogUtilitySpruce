using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogUtilitySpruce.Editor
{
    public class DialogGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogGraphView, UxmlTraits> { }

        public DialogGraphContainer DialogGraphContainer;
        private Action<SerializableGuid> _onStartNodeIdChanged;

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }
        
        public DialogGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            _onStartNodeIdChanged += guid =>
            {
                DialogGraphContainer.startNodeId = guid;
            };

            Undo.undoRedoPerformed += () =>
            {
                ClearGraph();
                foreach (var nodeData in DialogGraphContainer.dialogNodeDataList)
                {
                    AddElement(AddNode(nodeData));
                }
                ConnectNodes(DialogGraphContainer.nodeLinks);
            };
            
            void OnDeleteSelection(string operationName, AskUser askUser)
            {
                for (int i = 0; i< selection.Count; i++)
                {
                    if (selection[i] is Edge edge)
                    {
                        Undo.RecordObject(DialogGraphContainer, "Remove edge");
                        DeleteSelection();
                    }
                }
            }

            deleteSelection += OnDeleteSelection;
            
            graphViewChanged += change =>
            {
                if (change.elementsToRemove != null)
                {
                    foreach (var el in change.elementsToRemove)
                    {
                        if (el is Port port)
                        {
                            DeleteElements(port.connections);
                        }
                        
                        if (el is Edge ed)
                        {
                            //removing all deleted edges from container node links if presented
                            var bId = ((DialogNode) ed.output.node).Model.Id;
                            var tId =  ((DialogNode) ed.input.node).Model.Id;
                            var bpId =  (SerializableGuid) Guid.Parse(ed.output.Q<Port>().name);
                            var l = DialogGraphContainer.nodeLinks.Find(x =>
                                x.baseNodeID == bId && x.basePortID == bpId && x.targetNodeID == tId);
                            if (l!=null)
                            {
                                Undo.RecordObject(DialogGraphContainer, "Remove edge");
                                DialogGraphContainer.nodeLinks.Remove(l);
                            }
                        }
                    }
                }

                if (change.edgesToCreate != null)
                {
                    //adding all created edges to container node links if none presented
                    foreach (var ed in change.edgesToCreate)
                    {
                        var bId = ((DialogNode) ed.output.node).Model.Id;
                        var tId =  ((DialogNode) ed.input.node).Model.Id;
                        var bpId =  (SerializableGuid) Guid.Parse(ed.output.Q<Port>().name);
                        if (!DialogGraphContainer.nodeLinks.Exists(x =>
                            x.baseNodeID == bId && x.basePortID == bpId && x.targetNodeID == tId))
                        {
                            Undo.RecordObject(DialogGraphContainer, "Add edge");
                            DialogGraphContainer.nodeLinks.Add(new NodeLinkData
                            {
                                baseNodeID = ((DialogNode) ed.output.node).Model.Id,
                                basePortID = Guid.Parse(ed.output.Q<Port>().name),
                                targetNodeID = ((DialogNode) ed.input.node).Model.Id
                            });
                        }
                    }
                }

                return change;
            };
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port=> {
                if (startPort.node != port.node && startPort.direction!=port.direction && 
                    !edges.ToList().Exists(x=>x.input == startPort &&x.output == port||x.input == port &&x.output == startPort))
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
        
        public DialogNode AddNode(DialogNodeDataContainer nodeData = null, Vector3 position = default)
        {
            nodeData = nodeData ? nodeData : ScriptableObject.CreateInstance<DialogNodeDataContainer>();
            var model = DialogNodeFactory.GetNode(nodeData);
            var node = model.View;
            if (position != default)
            {
                nodeData.GetData().position = position;
            }
            node.SetPosition(new Rect(nodeData.GetData().position, Vector2.zero));
            var radioButton = node.Q<RadioButton>("startNode");
            
            if (!DialogGraphContainer.dialogNodeDataList.Contains(nodeData))
            {
                Undo.RecordObject(DialogGraphContainer, "Add node");
                DialogGraphContainer.dialogNodeDataList.Add(nodeData);
                
                if(AssetDatabase.Contains(DialogGraphContainer))
                    AssetDatabase.AddObjectToAsset(nodeData, DialogGraphContainer);
            }

            radioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue)
                {
                    _onStartNodeIdChanged?.Invoke(model.Id);
                }
            });
            radioButton.SetValueWithoutNotify(DialogGraphContainer.startNodeId == model.Id);
            
            _onStartNodeIdChanged += node.OnStartNodeIdChanged;

            if (!DialogGraphContainer.startNodeId)
            {
                _onStartNodeIdChanged?.Invoke(model.Id);
            }

            void OnDeleteSelection(string operationName, AskUser askUser)
            {
                if (selection.Contains(node))
                {
                    Undo.RecordObject(DialogGraphContainer, "Delete node");
                    DialogGraphContainer.dialogNodeDataList.Remove(nodeData);
                    DeleteNode(node);
                    deleteSelection -= OnDeleteSelection;
                }
            }

            deleteSelection += OnDeleteSelection;

            Undo.undoRedoPerformed += () =>
            {
                if (DialogGraphContainer.dialogNodeDataList.Contains(nodeData) && 
                    !nodes.Cast<DialogNode>().ToList().Exists(x=>x.Model.Id == nodeData.Id))
                {
                    AddNode(nodeData);
                    ConnectNodes(DialogGraphContainer.nodeLinks);
                }
                if (!DialogGraphContainer.dialogNodeDataList.Contains(nodeData) && 
                    nodes.Cast<DialogNode>().ToList().Exists(x=>x.Model.Id == nodeData.Id))
                {
                    DeleteNode(node);
                    deleteSelection -= OnDeleteSelection;
                }
            };
            
            node.OnPortDelete += p =>
            {
                DeleteElements(p.connections);
            };
            AddElement(node);
            return node;
        }
        
        private void DeleteNode(DialogNode node)
        {
            _onStartNodeIdChanged -= node.OnStartNodeIdChanged;
            DeleteSelection();
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        { 
            Vector3 screenMousePosition = evt.localMousePosition;
            evt.menu.AppendAction(
                    "Add",
                    _ =>
                    {
                        Vector2 worldMousePosition = screenMousePosition - contentViewContainer.transform.position;
                        worldMousePosition *= 1 / contentViewContainer.transform.scale.x;
                        AddNode(position: worldMousePosition);
                    });
            evt.menu.AppendAction(
                "Delete",
                _ =>
                {
                    var selections = selection.ToList();
                    foreach (ISelectable selectable in selections)
                    {
                        if (selectable is DialogNode)
                        {
                            DeleteNode((DialogNode) selectable);
                        }
                        if (selectable is Edge edge)
                        {
                            DeleteElements(new []{ edge });
                        }
                    }
                });
        }
        
        public void ConnectNodes(List<NodeLinkData> nodeLinks)
        {
            var dialogNodes = nodes.Cast<DialogNode>().ToList();
            foreach (DialogNode node in dialogNodes)
            {
                var connections = nodeLinks.Where(x => x.baseNodeID == node.Model.Id).ToList();
                foreach (NodeLinkData link in connections)
                {
                    var targetNodeGuid = link.targetNodeID;
                    var targetNode = dialogNodes.First(x => x.Model.Id == targetNodeGuid);
                    var tmpEdge = new Edge
                    {
                        output = node.outputContainer.Q<Port>(link.basePortID.Value),
                        input = targetNode.inputContainer.Q<Port>()
                    };
                    tmpEdge.input.Connect(tmpEdge);
                    tmpEdge.output.Connect(tmpEdge);
                    if(!edges.ToList().Exists(x=>x.input == tmpEdge.input && x.output == tmpEdge.output))
                        Add(tmpEdge);
                    tmpEdge.UpdatePresenterPosition();
                    
                }
            }
        }
        
        public void ClearGraph()
        {
            foreach (var node in nodes)
            {
                edges.ToList().ForEach(x=>x.RemoveFromHierarchy());
                RemoveElement(node);
            }
        }
    }
}