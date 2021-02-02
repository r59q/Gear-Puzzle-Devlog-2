using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggingScript : MonoBehaviour
{
    public Gear[] gears;

    private void Start()
    {
        for (int i = 1; i < gears.Length; i++)
        {
            gears[i - 1].AddAdjecentGear(gears[i]);
        }
    }

}
