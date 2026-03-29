using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 全局游戏事件中心：目前只包含食物送达事件。
/// </summary>
public class GameEvents : MonoBehaviour
{
    [LabelText("全局事件中心单例")]
    public static GameEvents current;

    /// <summary>
    /// 初始化单例引用。
    /// </summary>
    private void Awake()
    {
        current = this;
    }

    [LabelText("食物送达事件")]
    public event Action onFoodDelivered;

    /// <summary>
    /// 触发食物送达事件。
    /// </summary>
    public void FoodDelivered()
    {
        if(onFoodDelivered != null)
        {
            onFoodDelivered();
        }
    }
}
