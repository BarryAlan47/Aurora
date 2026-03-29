using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Helper 升级购买点：负责打开/关闭 Helper 升级窗口并在加载时恢复 Helper。
/// </summary>
public class HelperBuy_UpgradePoint : MonoBehaviour
{
    [LabelText("升级窗口物体")]
    public GameObject window;

    /// <summary>
    /// 根据存档决定是否在开始时实例化 Helper。
    /// </summary>
    private void Start()
    {
        HelperSpawner helperSpawner = window.GetComponent<HelperSpawner>();
        GameObject helperPrefab = helperSpawner.helperPrefab;
        Transform helperSpawnPoint = helperSpawner.helperSpawnPoint;

        if (PlayerPrefs.HasKey(helperSpawner.srNo + "Helper"))
        {
            helperSpawner.helper = Instantiate(helperPrefab, helperSpawnPoint.position, helperSpawnPoint.rotation).GetComponent<Helper>();
        }
    }

    /// <summary>
    /// 打开 Helper 升级窗口。
    /// </summary>
    public void OpenWindow()
    {
        window.SetActive(true);
    }

    /// <summary>
    /// 关闭 Helper 升级窗口。
    /// </summary>
    public void CloseWindow()
    {
        window.SetActive(false);
    }
}
