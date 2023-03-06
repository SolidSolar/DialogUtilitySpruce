using System;
using System.Collections.Generic;
using DialogUtilitySpruce;
using UnityEngine;

namespace DialogUtilitySpruce
{
    [Serializable]
    public class CharacterData
    {
        public SerializableGuid id = Guid.NewGuid();
        [SerializeField]
        public List<SerializableGuid> usages = new();

        public string Name
        {
            get => resource.texts[id];
            set => resource.texts[id] = value;
        }
        public Sprite icon;
        public LocalisationResource resource;
    }

}