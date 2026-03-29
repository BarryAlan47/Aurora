using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 货架管理器：维护货架容量、当前食物以及顾客站位点。
/// </summary>
public class FoodPlaceManager : MonoBehaviour
{
    [LabelText("Helper 站位点")]
    public Transform HelperPos;

    [LabelText("货架食物容量")]
    public int collectFoodCapacity;

    [LabelText("本货架对应食物名称")]
    public string shelfFoodName;

    [LabelText("货架顶部位置 Transform")]
    public Transform shelfTopTransform;

    [LabelText("顾客站位点列表")]
    public List<CustomerPoints> customerPoints;

    [HideInInspector]
    [LabelText("当前货架上现有食物列表")]
    public List<Food> collectedFoods;

    [LabelText("货架每一层的位置列表")]
    public List<Transform> shelfPos;

    [LabelText("可用的食物生成器数组")]
    public FoodSpawner[] availableFoodSpawners;

    /// <summary>
    /// 根据解锁标记激活货架。
    /// </summary>
    private void Awake()
    {
        if (PlayerPrefs.HasKey("Unlocked"))
            gameObject.SetActive(true);
    }

    /// <summary>
    /// 根据当前食物数量移动货架顶点位置。
    /// </summary>
    public void MoveShelfTopTransform()
    {
        if(collectedFoods.Count < collectFoodCapacity)
        shelfTopTransform.position = shelfPos[collectedFoods.Count].position;
    }
}
