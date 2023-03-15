using System;
using System.Collections.Generic;
using DialogUtilitySpruce;
using UnityEngine;

namespace DialogUtilitySpruce
{
    public class DialogGraphContainer : ScriptableObject
    {
        [HideInInspector]
        public SerializableGuid id = Guid.NewGuid();
        [HideInInspector]
        public SerializableGuid startNodeId;
        [HideInInspector]
        [SerializeReference]
        public List<NodeLinkData> nodeLinks = new();
        [HideInInspector]
        [SerializeReference]
        public List<DialogNodeDataContainer> dialogNodeDataList = new();
        [SerializeField] 
        public LocalisationResource localisationResource;
        [HideInInspector]
        public List<CharacterData> characterList = new();

    }
}