using System;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public static class DialogNodeFactory
    {
        public static DialogNodeModel GetNode(DialogNodeDataContainer nodeData)
        {
            DialogNodeModel model = new (nodeData);
            
            DialogNodeController controller = new (model);

            DialogNode node = new (controller, model)
            {
                title = "Dialog item"
            };
            
            model.View = node;
            return model;
        }
        
    }
}