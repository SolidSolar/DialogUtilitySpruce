using System;
using DialogUtilitySpruce;
using UnityEngine;

[Serializable]
public class LocalisationResource : ScriptableObject
{
    public DictionaryOfSerializableGuidAndString texts = new ();
    
    public string GetText(SerializableGuid guid)
    {
        return texts[guid];
    }
}
