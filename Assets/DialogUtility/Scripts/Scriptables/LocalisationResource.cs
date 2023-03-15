using System;
using System.Linq;
using DialogUtilitySpruce;
using UnityEngine;

[Serializable]
public class LocalisationResource : ScriptableObject
{
    public DictionaryOfSerializableGuidAndString texts = new ();
    
    public string GetText(SerializableGuid guid)
    {
        if (!texts.ContainsKey(guid))
        {
            return "";
        }
        return texts[guid];
    }
}
