using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogUtilitySpruce
{
    [Serializable]
    public class DialogNodeData
    {
        public static DialogNodeData GetCopy(DialogNodeData data)
        {
            return (DialogNodeData) data.MemberwiseClone();
        }
        
        public SerializableGuid id = Guid.NewGuid();
        public SerializableGuid characterId;
        public string text;
        public Sprite sprite;
        public Vector2 position;
        [SerializeReference]
        public List<PortConditionData> ports = new();
    }

    [Serializable]
    public class PortConditionData
    {
        public SerializableGuid id = Guid.NewGuid();
        public Condition condition;
    }

}

