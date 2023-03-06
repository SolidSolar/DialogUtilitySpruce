using System;

namespace DialogUtilitySpruce.Editor
{
    public static class DialogNodeFactory
    {
        public static DialogNodeModel GetNode(DialogNodeData nodeData = default)
        {
            DialogNodeModel model = new (nodeData ?? new DialogNodeData
            {
                id = Guid.NewGuid(),
                text = ""
            });
            
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