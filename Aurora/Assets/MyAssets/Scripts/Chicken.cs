using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 鸡逻辑：从货架吃食物并通过 FoodSpawner 产出蛋类食物。
/// </summary>
public class Chicken : MonoBehaviour
{
    [LabelText("是否当前可以吃东西")]
    private bool canEat = true;

    [LabelText("动画控制器")]
    public Animator anim;

    [LabelText("鸡对应的货架")]
    public FoodPlaceManager shelf;

    [LabelText("产出食物的生成器数组")]
    public FoodSpawner[] foodSpawners;

    [LabelText("当前使用的生成器")]
    private FoodSpawner currentFoodSpawner;

    private void Start()
    {

    }

    /// <summary>
    /// 检查货架是否有食物以及是否可以进食。
    /// </summary>
    private void Update()
    {
        if (shelf.collectedFoods.Count > 0 && canEat)
        {
            for(int i = 0; i< foodSpawners.Length; i++)
            {
                if (foodSpawners[i].foodObj == null)
                {
                    canEat = false;
                    Invoke("Eat", 2);
                    currentFoodSpawner = foodSpawners[i];
                    break;
                }
            }
        }   
    }

    /// <summary>
    /// 执行吃货架上最后一个食物并准备产蛋。
    /// </summary>
    private void Eat()
    { 
        if(anim)
        anim.SetTrigger("Play");

        Food food = shelf.collectedFoods[shelf.collectedFoods.Count - 1];
        shelf.collectedFoods.Remove(food);
        MoveShelfTopTransform();

        Destroy(food.gameObject, 1);

        Invoke("SpawnEgg", 1);
    }

    /// <summary>
    /// 调用当前 FoodSpawner 生成新食物（蛋）。
    /// </summary>
    private void SpawnEgg()
    {
        print("Eat");

        currentFoodSpawner.Spawn();
        canEat = true;
    }

    /// <summary>
    /// 更新货架顶部 Transform 位置。
    /// </summary>
    public void MoveShelfTopTransform()
    {
        if (shelf.collectedFoods.Count < shelf.collectFoodCapacity)
            shelf.shelfTopTransform.position = shelf.shelfPos[shelf.collectedFoods.Count].position;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (shelf.collectedFoods.Count > 0 && canEat)
    //        {
                
    //            canEat = false;
    //            Invoke("Wait", 4);
    //        }
    //    }
    //}
}
