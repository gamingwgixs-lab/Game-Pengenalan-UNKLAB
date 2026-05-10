using System.Collections.Generic;
using UnityEngine;

public class GOTogglerReferences : MonoBehaviour
{
    [SerializeField] List<GameObject> goList = new List<GameObject>();

    public void DisableByIndex(int index)
    {
        if (index >= 0 && index < goList.Count)
        {
            if (goList[index] != null) 
                goList[index].SetActive(false);
        }
        else
        {
            Debug.LogWarning("Index di luar jangkauan list!");
        }
    }

    public void EnableByIndex(int index)
    {
        if (index >= 0 && index < goList.Count)
        {
            if (goList[index] != null) 
                goList[index].SetActive(true);
        }
    }

    public void DisableAll()
    {
        foreach (GameObject go in goList)
        {
            if (go != null) go.SetActive(false);
        }
    }
}