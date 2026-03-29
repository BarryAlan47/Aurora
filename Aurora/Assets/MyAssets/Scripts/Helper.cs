using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

/// <summary>
/// Helper 逻辑：自动从仓库取货并补货到货架。
/// </summary>
public class Helper : MonoBehaviour
{
    [LabelText("食物堆叠位置")]
    public Transform foodCollectPos;

    [LabelText("初始堆叠本地坐标")]
    private Vector3 initialFoodCollectPos;

    [LabelText("是否移除过任意食物标记")]
    private bool removedAnyFood;

    [LabelText("是否可以检测路径抵达")]
    private bool canCheck = true;

    [LabelText("动画控制器")]
    public Animator anim;

    [LabelText("是否正在检查到达货架")]
    private bool checkReachedShelf;

    [LabelText("导航代理")]
    public NavMeshAgent agent;

    [LabelText("目标货架位置")]
    private Transform targetShelfPos;

    [LabelText("升级特效")]
    public ParticleSystem upgradeParticle;

    [LabelText("玩家物品管理器")]
    public PlayerManager _PlayerManager;

    [LabelText("垃圾桶 Transform")]
    private Transform trashBin;

    [LabelText("场景中所有货架")]
    private GameObject[] shelfs;

    /// <summary>
    /// 初始化 Helper 的容量、速度及初始状态，并开始寻找货架。
    /// </summary>
    private void Start()
    {
        trashBin = GameObject.FindGameObjectWithTag("TrashBin").transform;

        _PlayerManager.maxFoodPlayerCarry = PlayerPrefs.GetInt("CapacityVal", _PlayerManager.maxFoodPlayerCarry);

        GetComponent<NavMeshAgent>().speed = PlayerPrefs.GetFloat("SpeedVal", GetComponent<NavMeshAgent>().speed);

        initialFoodCollectPos = foodCollectPos.transform.localPosition;

        agent.updateRotation = true;

        FindShelf();
    }

    /// <summary>
    /// 在场景中查找需要补货的货架，并前往对应食物生成点。
    /// </summary>
    private void FindShelf()
    {
        shelfs = GameObject.FindGameObjectsWithTag("Shelf");

        foreach (GameObject shelf in shelfs)
        {
            FoodPlaceManager _FoodPlaceManager = shelf.GetComponent<FoodPlaceManager>();

            int i = _FoodPlaceManager.collectFoodCapacity / 2;

            if (_FoodPlaceManager.collectedFoods.Count < i)
            {
                targetShelfPos = _FoodPlaceManager.HelperPos;

                foreach (FoodSpawner foodSpawner in _FoodPlaceManager.availableFoodSpawners)
                {
                    if (foodSpawner.food.foodName == _FoodPlaceManager.shelfFoodName)
                    {

                        if (foodSpawner.foodObj != null)
                        {
                            _PlayerManager.currentFoodName = _FoodPlaceManager.shelfFoodName;

                            Goto(foodSpawner.transform.position);
                            return;
                        }
                    }

                }
            }
        }

        Invoke("FindShelf", 2);
    }

    /// <summary>
    /// 前往指定目标位置。
    /// </summary>
    private void Goto(Vector3 target)
    {
        agent.SetDestination(target);
        canCheck = true;
    }

    /// <summary>
    /// 更新 Helper 的移动、补货与丢弃逻辑状态机。
    /// </summary>
    private void Update()
    {

        if (ReachedDestinationOrGaveUp() && canCheck)
        {
            if (_PlayerManager.collectedFood.Count >= _PlayerManager.minimumFoodPlayerCarry)
            {
                Goto(targetShelfPos.position);
                checkReachedShelf = true;
                canCheck = false;
            }
        }

        if (ReachedDestinationOrGaveUp() && checkReachedShelf)
        {
            if (_PlayerManager.collectedFood.Count == 0)
            {
                checkReachedShelf = false;
                Invoke("FindShelf", .5f);
            }
            else
            {
                agent.SetDestination(trashBin.position);
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    /// <summary>
    /// 检查是否已经到达导航目标或放弃路径。
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
    /// 在货架触发范围内时，尝试从自身携带的食物中往货架补货。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Shelf"))
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();

            if (shelf.collectedFoods.Count < shelf.collectFoodCapacity)
            {
                int collectedFoodCount = _PlayerManager.collectedFood.Count - 1;

                if (collectedFoodCount >= 0)
                {
                    for (int i = _PlayerManager.collectedFood.Count - 1; i >= 0; i--)
                    {
                        if (_PlayerManager.collectedFood[i].foodName == shelf.shelfFoodName)
                        {
                            removedAnyFood = true;
                            _PlayerManager.collectedFood[i].PlaceFood(shelf.shelfTopTransform);
                            FindObjectOfType<AudioManager>().Play("PlaceFood");

                            shelf.collectedFoods.Add(_PlayerManager.collectedFood[i]);
                            _PlayerManager.collectedFood[i].transform.parent = shelf.transform;
                            shelf.MoveShelfTopTransform();

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
        }
    }

    /// <summary>
    /// 提升 Helper 携带容量并保存到 PlayerPrefs。
    /// </summary>
    /// <param name="increaseVal">增加的容量值。</param>
    public void IncreaseCapacity(int increaseVal)
    {
        _PlayerManager.maxFoodPlayerCarry += increaseVal;

        PlayerPrefs.SetInt("CapacityVal", _PlayerManager.maxFoodPlayerCarry);
    }

    /// <summary>
    /// 提升 Helper 移动速度并保存到 PlayerPrefs。
    /// </summary>
    /// <param name="increaseVal">增加的速度值。</param>
    public void IncreaseSpeed(int increaseVal)
    {
        GetComponent<NavMeshAgent>().speed += increaseVal;

        PlayerPrefs.SetFloat("SpeedVal", GetComponent<NavMeshAgent>().speed);
    }
}
