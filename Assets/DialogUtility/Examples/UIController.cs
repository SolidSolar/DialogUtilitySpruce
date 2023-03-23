using System;
using UnityEngine;

namespace DialogUtilitySpruce.Examples
{
    public class UIController : MonoBehaviour
    {
        public static Action OnNext;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnNext?.Invoke();
            }
        }
    }
}