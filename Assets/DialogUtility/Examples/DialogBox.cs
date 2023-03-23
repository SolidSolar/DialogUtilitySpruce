using System;
using System.Collections;
using DialogUtilitySpruce.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogUtilitySpruce.Examples
{
    public class DialogBox : MonoBehaviour
    {
        public static DialogBox Instance { get; private set; }

        public DialogReader dialogReader;
        public GameObject textContainer;
        public Image image;
        public TextMeshProUGUI textBox;

        [SerializeField] private int charactersPerMessage;
        [SerializeField] private DialogGraphContainer _currentDialog;
        private int _currentMessage;
        private int _messagesShown;
        private bool _ended = false;

        private Action _nextMessageFunc;

        // Use this for initialization
        void Start()
        {
            Instance = this;
            _nextMessageFunc = NextMessage;
            ShowDialog(_currentDialog);
        }


        public void NextMessage()
        {
            if (!_ended || _messagesShown != 0)
            {
                if (_messagesShown == 0)
                {
                    dialogReader.NextMessage();
                }
                else
                {
                    _currentMessage++;
                }
            }
            else
            {
                End();
            }
        }

        public void ShowDialog(DialogGraphContainer dialog)
        {
            _ended = false;
            _currentDialog = dialog;
            dialogReader.OnNextMessageShown.AddListener(UpdateTextBox);
            dialogReader.BeginDialog(dialog);
            UIController.OnNext += _nextMessageFunc;
            dialogReader.OnDialogEnded.AddListener(_markEnded);
        }


        private void UpdateTextBox(DialogNode data)
        {
            textContainer.SetActive(true);
            image.sprite = data.Sprite ? data.Sprite : data.Character?.icon;

            StartCoroutine(ShowMessage(data.Text));
        }

        private void _markEnded(DialogNode node)
        {
            _ended = true;
        }

        private void End()
        {
            textContainer.SetActive(false);
            image.gameObject.SetActive(false);
            dialogReader.OnNextMessageShown.RemoveListener(UpdateTextBox);
            UIController.OnNext -= _nextMessageFunc;
            dialogReader.OnDialogEnded.RemoveListener(_markEnded);
        }

        private void OnDisable()
        {
            if (textBox != null && dialogReader.IsActive)
                dialogReader.EndDialog();
        }


        private IEnumerator ShowMessage(string message)
        {
            float u = ((float) message.Length) / charactersPerMessage;
            if (u == 0)
                u = 1;
            int st = 0;
            if (message.Length == 0)
                yield break;
            for (int i = 0; i < u; i++)
            {
                if (st >= message.Length)
                    break;
                string endStr = st + charactersPerMessage >= message.Length
                    ? message.Substring(st)
                    : message.Substring(st, charactersPerMessage);

                int r = 0;
                while (endStr[endStr.Length - 1] != '.' && endStr[endStr.Length - 1] != ' ' &&
                       endStr[endStr.Length - 1] != '?' && endStr[endStr.Length - 1] != '!' &&
                       st + charactersPerMessage + r < message.Length)
                {
                    endStr += message[st + charactersPerMessage + r];
                    r++;
                }

                _messagesShown++;
                st += charactersPerMessage + r;
                textBox.text = endStr;
                if (u - i > 1)
                {
                    yield return new WaitUntil(() => _currentMessage >= _messagesShown);
                }
            }

            _messagesShown = 0;
            _currentMessage = 0;
        }
    }
}