using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    [Serializable]
    public class DialogUtilityLanguageSettings : ScriptableObject
    {
        public const string DefaultLanguage = "English";
        
        public List<string> Languages = new(){DefaultLanguage};
        [HideInInspector]
        public int CurrentLanguageIndex;

        private void OnValidate()
        {
            List<string> duplicates = Languages.GroupBy(x => x).SelectMany(g => g.Skip(1)).ToList();

            Languages = Languages.Distinct().ToList();
            
            int someInt = 1;
            for (int i = 0; i< duplicates.Count(); i++)
            {
                if (duplicates[i][^1] >= '0' && duplicates[i][^1] <= '9')
                    duplicates[i] = duplicates[i].Substring(0, duplicates[i].Length-1) + (char)(duplicates[i][^1]+1);
                else
                {
                    duplicates[i] += " " + someInt;
                }
                someInt++;
            }

            Languages = Languages.Concat(duplicates).ToList();

            if (Languages.Count < 1)
            {
                Languages.Add(DefaultLanguage);
            }
                
            CurrentLanguageIndex = 0;
        }
    }
}