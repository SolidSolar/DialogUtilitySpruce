using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DialogUtilitySpruce.Editor
{
    public static class CharactersListViewFactory
    {
        private static Dictionary<VisualElement, CharacterView> _itemsData = new();
        public static VisualElement GetList()
        {
            _itemsData = new();
            var element =  DialogGraphUXMLData.Instance.characterList.CloneTree();
            var globalList = element.Q<ListView>("globalList");
            var addButton = element.Q<Button>("add");
            
            addButton.clicked += () =>
            {
                CharacterList.Instance.CreateCharacter();
                globalList.Rebuild();
            };
            
            Func<VisualElement> makeItem = () =>
            {
                globalList.style.height = CharacterList.Instance.GlobalCharacterList.Count * globalList.fixedItemHeight;
                globalList.MarkDirtyRepaint();
                CharacterView item = new ();
                _itemsData[item.GetView()] = item;
                return item.GetView();
            };

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                globalList.style.height = CharacterList.Instance.GlobalCharacterList.Count * globalList.fixedItemHeight;
                globalList.MarkDirtyRepaint();
                CharacterModel model = CharacterList.Instance.GetCharacter(i);
                model.OnDelete = () =>
                {
                    globalList.style.height =
                        CharacterList.Instance.GlobalCharacterList.Count * globalList.fixedItemHeight;
                    globalList.Rebuild();
                    globalList.MarkDirtyRepaint();
                };
                _itemsData[e].SetItemData(model);
            };
            globalList.style.height = CharacterList.Instance.GlobalCharacterList.Count * globalList.fixedItemHeight;
            globalList.itemsSource = CharacterList.Instance.GlobalCharacterList;
            globalList.makeItem = makeItem;
            globalList.bindItem = bindItem;
            globalList.selectionType = SelectionType.Multiple;
            globalList.style.flexGrow = 1f;
            return element;
        }

    }
    
    
}