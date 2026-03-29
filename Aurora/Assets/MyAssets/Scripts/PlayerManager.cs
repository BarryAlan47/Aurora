using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 玩家物品管理：负责玩家当前携带的食物列表与堆叠位置。
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    [LabelText("当前携带食物列表")]
    public List<Food> collectedFood;

    [LabelText("玩家可携带食物上限")]
    public int maxFoodPlayerCarry;

    [LabelText("触发前往货架的最小携带数量")]
    public int minimumFoodPlayerCarry;

    [HideInInspector]
    [LabelText("初始食物堆叠位置")]
    public Vector3 initialFoodCollectPos;

    [LabelText("当前食物堆叠位置变换")]
    public Transform foodCollectPos;

    [HideInInspector]
    [LabelText("当前目标食物名称")]
    public string currentFoodName;

    /// <summary>
    /// 记录初始食物堆叠位置。
    /// </summary>
    private void Start()
    {
        initialFoodCollectPos = foodCollectPos.transform.localPosition;
    }

    /// <summary>
    /// 触发与垃圾桶的碰撞时丢弃所有食物。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TrashBin"))
        {
            for (int i = collectedFood.Count - 1; i >= 0; i--)
            {
                AudioManager.Instance.Play("FoodPlace");

                collectedFood[i].GotoTrashBin(other.transform);
                collectedFood.Remove(collectedFood[i]);
            }

            foodCollectPos.localPosition = initialFoodCollectPos;
        }
    }
}
