using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 自动门滑动：玩家或顾客靠近时开门，离开时关门。
/// </summary>
public class DoorSliding : MonoBehaviour
{
    [LabelText("右门与左门 Transform")]
    public Transform doorR, doorL;

    [LabelText("开关门动画时长")]
    public float duration;

    [LabelText("右门初始 X 位置")]
    private float initialXPos;

    /// <summary>
    /// 记录右门的初始局部 X 坐标。
    /// </summary>
    private void Start()
    {
        initialXPos = doorR.localPosition.x;
    }

    /// <summary>
    /// 玩家或顾客在触发区域内时，滑动开门。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Customer") || other.CompareTag("Player"))
        {
            doorR.DOLocalMoveX(initialXPos*2, duration);
            doorL.DOLocalMoveX(-initialXPos*2, duration);
        }
    }

    /// <summary>
    /// 玩家或顾客离开触发区域时，滑动关门。
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Customer") || other.CompareTag("Player"))
        {
            doorR.DOLocalMoveX(initialXPos, duration);
            doorL.DOLocalMoveX(-initialXPos, duration);
        }
    }
}
