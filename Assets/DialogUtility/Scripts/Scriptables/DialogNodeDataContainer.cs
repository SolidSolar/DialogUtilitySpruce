using UnityEngine;

namespace DialogUtilitySpruce
{
    /// <summary>
    /// Containers are ScriptableObjects and Data objects are not. This is required to prevent accidental
    /// changes in data when using editor runtime
    /// </summary>
    public class DialogNodeDataContainer : ScriptableObject
    {
        public SerializableGuid Id => data.id;
        
        [SerializeField]
        private DialogNodeData data = new DialogNodeData();

        
        /// <summary>
        /// Returns copy of stored DialogNodeData object
        /// </summary>
        /// <returns></returns>
        public DialogNodeData GetDataCopy(LocalisationResource resource)
        {
            DialogNodeData copy = DialogNodeData.GetCopy(data);
            if (resource)
            {
                if (!resource.texts.ContainsKey(copy.id))
                {
                    resource.texts[copy.id] = "";
                }
                copy.text = resource.texts[copy.id];
            }

            return copy;
        }
        
        public DialogNodeData GetData()
        {
            return data;
        }
        
        public void SetData(DialogNodeData data)
        {
            this.data = data;
        }
    }
}