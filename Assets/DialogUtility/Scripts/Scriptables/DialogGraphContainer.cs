using System;
using System.Collections.Generic;
using DialogUtilitySpruce;
using UnityEngine;

namespace DialogUtilitySpruce
{
    [CreateAssetMenu(menuName = "DialogGraph/EventGraphContainer")]
    public class DialogGraphContainer : ScriptableObject
    {
        public SerializableGuid id = Guid.NewGuid();
        
        public SerializableGuid startNodeId;
        [SerializeReference]
        public List<NodeLinkData> nodeLinks = new();
        [SerializeReference]
        public List<DialogNodeDataContainer> dialogNodeDataList = new();
        [SerializeField] 
        public LocalisationResource localisationResource;
        
        public List<CharacterData> characterList = new();

    }
}