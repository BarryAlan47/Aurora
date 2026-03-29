#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class NavMeshBakeEditor : MonoBehaviour
{
#if UNITY_EDITOR

    public BuyPoint[] buyPoints;
    private int i = 0;
    public bool activeObjects;
    //public FoodSpawner[] foodSpawners;

    //public List<FoodSpawner> Tomato;
    //public List<FoodSpawner> Egg;
    //public List<FoodSpawner> Beans;
    //public List<FoodSpawner> Milk;
    //public List<FoodSpawner> Bread;
    //public List<FoodSpawner> FlourBag;
    //public List<FoodSpawner> Wheat;
    //public List<FoodSpawner> Sauce;

    void Start()
    {
        if(buyPoints.Length == 0)
        buyPoints = FindObjectsOfType<BuyPoint>();

        //foodSpawners = FindObjectsOfType<FoodSpawner>();

        //Tomato        =  new List<FoodSpawner>();
        //Egg           =  new List<FoodSpawner>();
        //Beans         =  new List<FoodSpawner>();
        //Milk          =  new List<FoodSpawner>();
        //Bread         =  new List<FoodSpawner>();
        //FlourBag      =  new List<FoodSpawner>();
        //Wheat         =  new List<FoodSpawner>();
        //Sauce = new List<FoodSpawner>();


        //foreach (FoodSpawner foodSpawner in foodSpawners)
        //{
        //    if(foodSpawner.food.name == "Tomato")
        //    {
        //        Tomato.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Egg")
        //    {
        //        Egg.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Sauce")
        //    {
        //        Sauce.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Beans")
        //    {
        //        Beans.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Wheat")
        //    {
        //        Wheat.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Milk")
        //    {
        //        Milk.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Flour")
        //    {
        //        FlourBag.Add(foodSpawner);
        //    }

        //    if (foodSpawner.food.name == "Bread")
        //    {
        //        Bread.Add(foodSpawner);
        //    }  

        //}

        foreach (BuyPoint buyPoint in buyPoints)
        {
            if(activeObjects)
                buyPoint.objectToUnlock.SetActive(true);
            else
                buyPoint.objectToUnlock.SetActive(false);

            buyPoint.gameObject.name = "BuyPoint " + i;
            i++;
            buyPoint.srNo = i;

            PrefabUtility.RecordPrefabInstancePropertyModifications(buyPoint.gameObject);
            EditorUtility.SetDirty(buyPoint);

        }

        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif

}
