using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 食物生成器：负责按需在指定位置生成食物。
/// </summary>
public class FoodSpawner : MonoBehaviour
{
    [LabelText("生成的食物原型")]
    public Food food;

    [LabelText("生成动画控制器")]
    public Animator anim;

    [LabelText("当前场景中的食物实例")]
    public GameObject foodObj;

    [LabelText("是否在开始时立即生成")]
    public bool spawnAtStart;

    /// <summary>
    /// 根据配置决定是否在加载时立即生成一个食物。
    /// </summary>
    private void Awake()
    {
        if(spawnAtStart)
            SpawnFood();
    }

    /// <summary>
    /// 延时调用生成食物。
    /// </summary>
    public void SpawnFood()
    {
        Invoke("Spawn", 1);
    }

    /// <summary>
    /// 实际生成食物实例并设置父物体。
    /// </summary>
    public void Spawn()
    {
        if (anim)
            anim.SetTrigger("Play");

        foodObj = Instantiate(food.gameObject, transform.position, transform.rotation);
        foodObj.GetComponent<Food>().foodName = food.foodName;
        foodObj.transform.parent = this.transform;
    }
}
