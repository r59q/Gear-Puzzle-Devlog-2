using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    // Settings
    public int teethCount;
    public int resolution;
    public float thickness;
    public float module;
    public Material material;

    [SerializeField]
    private List<Gear> adjecentGears;

    private GearMesh gearMesh;

    // Start is called before the first frame update
    void Start()
    {
        if (GearMesh == null)
        {
            if (GetComponent<GearMesh>() == null)
            {
                gearMesh = gameObject.AddComponent<GearMesh>();
            }
        }

        // Set initial gear settings
        GearMesh.TeethCount = TeethCount;
        GearMesh.Resolution = Resolution;
        GearMesh.Thickness = Thickness;
        GearMesh.Module = Module;
        GearMesh.Material = Material;
        GearMesh.GenerateGear();
    }

    private void Update()
    {
        if (IsInputting)
        {
            RotateIn(InputSpeed);
        } else
        {
            InputSpeed = 0;
        }
    }

    public void RotateIn(float rotationSpeed)
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        foreach (Gear gear in adjecentGears)
        {
            gear.RotateIn( (rotationSpeed * -1) / ((float)gear.TeethCount / (float)TeethCount));
        }
    }

    public void AddAdjecentGear(Gear gear)
    {
        if (adjecentGears == null)
        {
            adjecentGears = new List<Gear>();
        }

        if (gear != this)
        {
            GearChain chain = GearChains.GetChainWith(this);
            if (chain == null)
            {
                chain = GearChains.CreateNewChain();
                chain.AddGear(this);
            }
            if (chain.CanAddGear(gear))
            {
                adjecentGears.Add(gear);
                chain.AddGear(gear);
            } else
            {
                Debug.Log("Warning: Could not add gear");
            }
        }
    }

    public List<Gear> GetAdjecencyList()
    {
        if (adjecentGears == null)
        {
            adjecentGears = new List<Gear>();
        }

        return adjecentGears;
    }

    public float InputSpeed
    {
        get;set;
    }

    public GearMesh GearMesh
    {
        get { return gearMesh; }
    }

    public int TeethCount
    {
        get { return teethCount; }
        set { 
            teethCount = value;
            gearMesh.TeethCount = TeethCount;
        }
    }

    public int Resolution
    {
        get { return resolution; }
        set { 
            resolution = value; 
            gearMesh.Resolution = Resolution;
        }
    }

    public float Thickness
    {
        get { return thickness; }
        set { 
            thickness = value; 
            gearMesh.Thickness = Thickness;
        }
    }

    public bool IsInputting
    {
        get;set;
    }

    public float Module
    {
        get { return module; }
        set { 
            module = value; 
            gearMesh.Module = Module;
        }
    }

    public Material Material
    {
        get { return material; }
        set { 
            material = value;
            gearMesh.Material = Material;
        }
    }
}
