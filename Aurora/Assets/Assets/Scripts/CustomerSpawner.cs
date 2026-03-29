using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 顾客生成器：按货架数量与随机时间生成顾客。
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [LabelText("顾客预制体")]
    public GameObject customerPrefab;

    [LabelText("顾客离场出口 Transform")]
    public Transform exitTransform;

    /// <summary>
    /// 根据货架数量，随机延时生成初始顾客。
    /// </summary>
    void Start()
    {
        int shelfsCount = GameObject.FindGameObjectsWithTag("Shelf").Length;

        for(int i = 0; i< shelfsCount; i++)
        {
            int spawnTime = Random.Range(1, 4);

            Invoke("SpawnCustomer", spawnTime);
        }
    }

    /// <summary>
    /// 生成一个顾客并设置其出口目标。
    /// </summary>
    public void SpawnCustomer()
    {
        GameObject customer = Instantiate(customerPrefab, transform.position, transform.rotation);

        customer.GetComponent<Customer>().exitTransform = exitTransform;
    }
}
