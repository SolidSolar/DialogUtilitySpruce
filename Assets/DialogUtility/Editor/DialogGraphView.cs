using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogUtilitySpruce.Editor
{
    public class DialogGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogGraphView, UxmlTraits> { }

        public SerializableGuid StartNodeId;
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
                StartNodeId = guid;
            };

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
                    }
                }

                return change;
            };
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port=> {
                if (startPort.node != port.node && startPort.direction!=port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
        
        public DialogNode AddNode(DialogNodeData nodeData = null)
        {
            var model = DialogNodeFactory.GetNode(nodeData);
            var node = model.View;
            var radioButton = node.Q<RadioButton>("startNode");
            radioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue)
                {
                    _onStartNodeIdChanged?.Invoke(model.Id);
                }
            });
            radioButton.SetValueWithoutNotify(StartNodeId == model.Id);
            
            _onStartNodeIdChanged += node.OnStartNodeIdChanged;

            if (!StartNodeId)
            {
                _onStartNodeIdChanged?.Invoke(model.Id);
            }
            
            CharacterList.Instance.OnCharacterChanged += model.OnCharacterUpdate;
            deleteSelection += (_, _) =>
            {
                DeleteNode(node);
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
            CharacterList.Instance.OnCharacterChanged -= node.Model.OnCharacterUpdate;
            _onStartNodeIdChanged -= node.OnStartNodeIdChanged;
            DeleteSelection();
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        { 
            evt.menu.AppendAction(
                    "Add",
                    _ => AddNode());
            evt.menu.AppendAction(
                "Delete",
                _ =>
                {
                    var selections = selection.ToList();
                    foreach (ISelectable selectable in selections)
                    {
                        DeleteNode((DialogNode)selectable);
                    }
                });
        }
    }
}