using System;
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

    public static void Copy(LocalisationResource origin, LocalisationResource destination)
    {
        destination.texts = new DictionaryOfSerializableGuidAndString();
        foreach (var kvp in origin.texts)
        {
            destination.texts.Add(kvp.Key, kvp.Value);
        }
    }
}
