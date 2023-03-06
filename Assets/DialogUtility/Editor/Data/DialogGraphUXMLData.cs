using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogUtilitySpruce.Editor
{
    [CreateAssetMenu(menuName = "DialogUtility/Create UXML Data object", fileName = "UXMLData.asset")]
    public class DialogGraphUXMLData : ScriptableObject
    {
        [NonSerialized] 
        private static DialogGraphUXMLData _instance;
        public static DialogGraphUXMLData Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<DialogGraphUXMLData>("Assets/DialogUtility/UXMLData.asset");
                }

                return _instance;
            }
        }
        public VisualTreeAsset characterItem;
        public VisualTreeAsset dialogGraphWindow;
        public VisualTreeAsset characterList;
        public VisualTreeAsset dialogNode;
        public VisualTreeAsset dialogNodePortItem;
        public VisualTreeAsset dialogNodeAddPortButton;
    }
}