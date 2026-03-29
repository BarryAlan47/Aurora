using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 顾客站位点：标记是否已被占用。
/// </summary>
public class CustomerPoints : MonoBehaviour
{
    [LabelText("是否已被顾客占用")]
    public bool fill;
}
