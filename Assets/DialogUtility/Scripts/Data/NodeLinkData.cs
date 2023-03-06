using System;

namespace DialogUtilitySpruce
{
    [Serializable]
    public class NodeLinkData
    {
        public SerializableGuid baseNodeID;
        public SerializableGuid basePortID;
        public SerializableGuid targetNodeID;
    }
}