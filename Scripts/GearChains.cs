using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearChains : MonoBehaviour
{
    public static GearChains instance;
    
    private void Awake()
    {
        instance = this;
        ChainList = new List<GearChain>();
    }

    public static GearChain GetChainWith(Gear gear)
    {
        foreach (GearChain chain in ChainList)
        {
            if (chain.Contains(gear))
            {
                return chain;
            }
        }
        return null;
    }

    public static GearChain CreateNewChain()
    {
        GearChain chain = new GameObject("GearChain").AddComponent<GearChain>();
        Debug.Log("Chain created");
        ChainList.Add(chain);

        return chain;
    }

    public static List<GearChain> ChainList
    {
        get;private set;
    }

}
