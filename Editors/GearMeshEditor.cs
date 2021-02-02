using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Threading.Tasks;

[CustomEditor(typeof(Gear))]
public class GearEditor : Editor
{
    int teethCount, resolution;
    float module, thickness;
    Gear gear;
    bool autoUpdate = false;
    private void OnEnable()
    {
        gear = (Gear)target;
        SetCheckMemory();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Mesh Manually"))
        {
            gear.GearMesh.GenerateGear();
        }
        if (GUILayout.Button("Auto-generate : " + autoUpdate.ToString()))
        {
            autoUpdate = !autoUpdate;
        }
        if (GUILayout.Button("Rotate : " + gear.IsInputting.ToString()))
        {
            gear.IsInputting = !gear.IsInputting;
        }
        if (GUILayout.Button("Add speed : " + gear.InputSpeed.ToString()))
        {
            gear.InputSpeed += 1f;
        }
        if (GUILayout.Button("Remove speed : " + gear.InputSpeed.ToString()))
        {
            gear.InputSpeed -= 1f;
        }

        // Set memory
        if (SetCheckMemory() && autoUpdate)
        {
            UpdateGearMeshData();
            gear.GearMesh.GenerateGear();
        }
    }

    private void UpdateGearMeshData()
    {
        gear.GearMesh.TeethCount = gear.TeethCount;
        gear.GearMesh.Resolution = gear.Resolution;
        gear.GearMesh.Module = gear.Module;
        gear.GearMesh.Thickness = gear.Thickness;
        gear.GearMesh.Material = gear.Material;
    }

    private bool SetCheckMemory()
    {
        bool result = false;
        if (teethCount  != gear.teethCount ||
            resolution  != gear.resolution ||
            module      != gear.module ||
            thickness   != gear.thickness)
        {
            result = true;
        }
        teethCount  = gear.teethCount;
        resolution  = gear.resolution;
        module      = gear.module;
        thickness   = gear.thickness;
        return result;
    }
}