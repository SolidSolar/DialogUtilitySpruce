using UnityEngine;

namespace DialogUtilitySpruce.Examples
{
    [CreateAssetMenu(menuName = "DialogGraph/Examples/SomethingExistsCondition")]
    public class SomethingExistsCondition : Condition
    {
        public string[] objectName;
        public override bool IsTrue(int conditionIndex)
        {
            return GameObject.Find(objectName[conditionIndex]);
        }
    }
}