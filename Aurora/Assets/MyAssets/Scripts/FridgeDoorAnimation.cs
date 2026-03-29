using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 冰箱门动画：玩家或顾客在触发区内时播放开门动画，离开时关门。
/// </summary>
public class FridgeDoorAnimation : MonoBehaviour
{
    [LabelText("门动画控制器（需含 Open 布尔参数）")]
    public Animator anim;

    /// <summary>
    /// 进入触发区（当前未使用，可扩展）。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
     
    }

    /// <summary>
    /// 玩家或顾客在触发区内时保持开门状态。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Customer"))
        {
            if (anim)
            {
                anim.SetBool("Open", true);
            }
        }
    }

    /// <summary>
    /// 玩家或顾客离开触发区时关门。
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Customer"))
        {
            if (anim)
            {
                anim.SetBool("Open", false);
            }
        }
    }
}
