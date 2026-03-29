using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

/// <summary>
/// 农民 NPC：在鸡舍/货架间取货、补货，逻辑与 Helper 类似但针对带 Chicken 标签的货架。
/// </summary>
public class Farmer : MonoBehaviour
{
    [LabelText("初始食物堆叠本地坐标")]
    private Vector3 initialFoodCollectPos;

    [LabelText("食物堆叠位置 Transform")]
    public Transform foodCollectPos;

    [LabelText("待机站立位置")]
    private Transform standPos;

    [LabelText("导航代理")]
    public NavMeshAgent agent;

    [LabelText("目标鸡舍/货架站位点")]
    private Transform targetChickenPos;

    [LabelText("是否允许检测到达目标")]
    private bool canCheck = true;

    [LabelText("是否已触发货架区域")]
    private bool reachedShelf;

    [LabelText("动画控制器")]
    public Animator anim;

    [LabelText("是否刚移除过食物标记")]
    private bool removedAnyFood;

    [LabelText("玩家物品管理器")]
    public PlayerManager _PlayerManager;

    [LabelText("随机数生成器")]
    private System.Random _random = new System.Random();

    /// <summary>
    /// 初始化待机点、堆叠位置并开始寻找需要补货的鸡舍。
    /// </summary>
    private void Start()
    {
        standPos = GameObject.Find("StandPos").transform;

        initialFoodCollectPos = foodCollectPos.transform.localPosition;

        agent.updateRotation = true;

        FindChicken();
    }

    [LabelText("场景中所有带 Chicken 标签的对象")]
    private GameObject[] chickens;

    /// <summary>
    /// 查找食物不足一半的鸡舍，并前往对应食物生成点。
    /// </summary>
    private void FindChicken()
    {
        chickens = GameObject.FindGameObjectsWithTag("Chicken");

        for(int i =0; i< chickens.Length; i++)
        {
            FoodPlaceManager chicken = chickens[i].GetComponent<FoodPlaceManager>();

            int j = chicken.collectFoodCapacity / 2;

            if (chicken.collectedFoods.Count < j)
            {
                targetChickenPos = chicken.HelperPos;
                FindFoodSpawner(chicken.shelfFoodName);
                return;
            }
        }

        Invoke("FindChicken", 2);
    }

    /// <summary>
    /// 按食物名称随机打乱后查找匹配的 <see cref="FoodSpawner"/> 并前往。
    /// </summary>
    /// <param name="_foodName">货架所需食物名称。</param>
    private void FindFoodSpawner(string _foodName)
    {
        FoodSpawner[] availableFoodSpawners = FindObjectsOfType<FoodSpawner>();

        Shuffle(availableFoodSpawners);

        foreach (FoodSpawner foodSpawner in availableFoodSpawners)
        {
            if (foodSpawner.food.foodName == _foodName)
            {
                Goto(foodSpawner.transform.position);
                return;
            }
        }

        FindChicken();
    }

    /// <summary>
    /// Fisher-Yates 打乱食物生成器数组顺序。
    /// </summary>
    void Shuffle(FoodSpawner[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = _random.Next(0, n);
            FoodSpawner t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }

    /// <summary>
    /// 设置导航目标并允许检测到达。
    /// </summary>
    private void Goto(Vector3 target)
    {
        agent.SetDestination(target);
        canCheck = true;
    }

    /// <summary>
    /// 更新寻路、补货后重新寻找鸡舍，以及跑步动画。
    /// </summary>
    private void Update()
    {
        if (ReachedDestinationOrGaveUp() && canCheck)
        {
            if (_PlayerManager.collectedFood.Count == _PlayerManager.maxFoodPlayerCarry)
            {
                Goto(targetChickenPos.position);
                canCheck = false;
            }
        }

        if (reachedShelf)
        {
            reachedShelf = false;
            Invoke("FindChicken", 3);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    /// <summary>
    /// 是否已到达导航目标或放弃路径。
    /// </summary>
    private bool ReachedDestinationOrGaveUp()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 在鸡舍触发区内时，将携带的匹配食物摆放到货架上。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        print("Collided  " + other.gameObject.name);

        if (other.CompareTag("Chicken"))
        {
            FoodPlaceManager ChickenShelf = other.GetComponent<FoodPlaceManager>();

            if (ChickenShelf.collectedFoods.Count < ChickenShelf.collectFoodCapacity)
            {
                int collectedFoodCount = _PlayerManager.collectedFood.Count - 1;

                if (collectedFoodCount >= 0)
                {
                    for (int i = _PlayerManager.collectedFood.Count - 1; i >= 0; i--)
                    {
                        if (_PlayerManager.collectedFood[i].foodName == ChickenShelf.shelfFoodName)
                        {
                            removedAnyFood = true;
                            _PlayerManager.collectedFood[i].PlaceFood(ChickenShelf.shelfTopTransform);
                            FindObjectOfType<AudioManager>().Play("PlaceFood");

                            ChickenShelf.collectedFoods.Add(_PlayerManager.collectedFood[i]);
                            _PlayerManager.collectedFood[i].transform.parent = ChickenShelf.transform;
                            ChickenShelf.MoveShelfTopTransform();

                            _PlayerManager.collectedFood[i].goToCustomer = true;
                            _PlayerManager.collectedFood.Remove(_PlayerManager.collectedFood[i]);
                            break;
                        }
                    }

                    if (removedAnyFood)
                    {
                        foodCollectPos.localPosition = initialFoodCollectPos;

                        foreach (Food food in _PlayerManager.collectedFood)
                        {
                            food.transform.localPosition = foodCollectPos.localPosition;
                            foodCollectPos.localPosition = new Vector3(foodCollectPos.transform.localPosition.x, foodCollectPos.transform.localPosition.y + 1, foodCollectPos.transform.localPosition.z);
                        }

                        removedAnyFood = false;
                    }
                }
            }
            else
            {
                Goto(standPos.position);
            }
        }
    }

    /// <summary>
    /// 进入鸡舍区域时标记已到达货架。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chicken"))
            reachedShelf = true;
    }

    /// <summary>
    /// 离开鸡舍区域时清除到达标记。
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chicken"))
            reachedShelf = false;
    }
}
