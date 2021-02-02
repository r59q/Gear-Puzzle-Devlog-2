using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearChain : MonoBehaviour
{
    private void Awake()
    {
        Gears = new List<Gear>();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < Gears.Count; i++)
        {
            Gizmos.color = Color.red;

            foreach (Gear gear in Gears[i].GetAdjecencyList())
            {
                Gizmos.DrawLine(Gears[i].transform.position + Vector3.forward * Gears[i].thickness, gear.transform.position + Vector3.forward * gear.thickness);
            }

        }
    }

    public List<Gear> Gears
    {
        get;private set;
    }

    public bool Contains(Gear gear)
    {
        return Gears.Contains(gear);
    }

    public void AddGear(Gear gear)
    {
        Gears.Add(gear);
        if (!ValidateChain())
        {
            Debug.Log("Warning! Cycle in gear chain");
        }
    }

    // Wasteful. I'll keep it for now.
    public bool CanAddGear(Gear gear)
    {
        Gears.Add(gear);
        bool result = ValidateChain();
        Gears.Remove(gear);
        return result;
    }

    public bool ValidateChain()
    {
        List<Gear> visitedGears = new List<Gear>();
        foreach (Gear gear in Gears)
        {
            visitedGears.Add(gear);
            foreach (Gear item in gear.GetAdjecencyList())
            {
                if (visitedGears.Contains(item))
                {
                    return false;
                }
            }
        }
        return true;
    }

}
