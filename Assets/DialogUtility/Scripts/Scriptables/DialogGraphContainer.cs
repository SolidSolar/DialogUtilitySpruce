using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogUtilitySpruce
{
    public class DialogGraphContainer : ScriptableObject
    {
        public SerializableGuid id = Guid.NewGuid();
        public SerializableGuid startNodeId;
        [SerializeReference]
        public List<NodeLinkData> nodeLinks = new();
        [SerializeReference]
        public List<DialogNodeDataContainer> dialogNodeDataList = new();
        [SerializeReference]
        public LocalisationResource localisationResource;
        public List<CharacterData> characterList = new();

        public static void Copy(DialogGraphContainer origin, DialogGraphContainer destination)
        {
            destination.id = origin.id;
            destination.startNodeId = origin.startNodeId;
            destination.nodeLinks = new List<NodeLinkData>(origin.nodeLinks);
            destination.dialogNodeDataList = new List<DialogNodeDataContainer>(origin.dialogNodeDataList);
            destination.localisationResource = origin.localisationResource;
            destination.characterList = new List<CharacterData>(origin.characterList);
            destination.name = origin.name;
        }
    }
}