using System;
using UnityEngine;

public class SelectUVChannelMultiMesh : MonoBehaviour
{
    public enum UVChannelOption
    {
        Unwrapped_UV0 = 0,
        LowPolyUnique_UV1 = 1,
        LowPolyShared_UV2 = 2
    }

    [System.Serializable]
    public class MeshMaterialMapping
    {
        [Tooltip("The target SkinnedMeshRenderer to modify.")]
        public SkinnedMeshRenderer renderer;

        [Header("Unique Materials per Channel")]
        public Material unwrappedMaterial;
        public Material lowPolyUniqueMaterial;
        public Material lowPolySharedMaterial;
    }

    [Header("Global Selection")]
    [Tooltip("Select which UV layout to apply to all target meshes.")]
    public UVChannelOption selectedUV = UVChannelOption.Unwrapped_UV0;

    [Header("Mesh Targets & Materials")]
    [Tooltip("Add as many meshes as needed. Each mesh uses its own material assignments.")]
    public MeshMaterialMapping[] meshTargets;

    void Start()
    {
        ApplyUVAndMaterials(selectedUV);
    }

    public void ApplyUVAndMaterials(UVChannelOption option)
    {
        if (meshTargets == null || meshTargets.Length == 0)
        {
            Debug.LogWarning($"No mesh targets assigned on {gameObject.name}.");
            return;
        }

        foreach (var target in meshTargets)
        {
            if (target == null || target.renderer == null || target.renderer.sharedMesh == null)
                continue;

            // 1. Instantiate unique mesh copy and reassign UVs
            Mesh mesh = Instantiate(target.renderer.sharedMesh);
            Vector2[] targetUVs = GetUVChannelData(mesh, option);

            if (targetUVs != null && targetUVs.Length > 0)
            {
                mesh.uv = targetUVs;
                target.renderer.sharedMesh = mesh;
            }
            else
            {
                Debug.LogWarning($"UV channel ({option}) on {target.renderer.name} is empty or unassigned.");
            }

            // 2. Assign the target's specific material for this option
            Material targetMaterial = GetMaterialForOption(target, option);
            if (targetMaterial != null)
            {
                target.renderer.material = targetMaterial;
            }
            else
            {
                Debug.LogWarning($"No material assigned for ({option}) on target renderer '{target.renderer.name}'.");
            }
        }
    }

    private Vector2[] GetUVChannelData(Mesh mesh, UVChannelOption option)
    {
        switch (option)
        {
            case UVChannelOption.Unwrapped_UV0: return mesh.uv;
            case UVChannelOption.LowPolyUnique_UV1: return mesh.uv2;
            case UVChannelOption.LowPolyShared_UV2: return mesh.uv3;
            default: return mesh.uv;
        }
    }

    private Material GetMaterialForOption(MeshMaterialMapping target, UVChannelOption option)
    {
        switch (option)
        {
            case UVChannelOption.Unwrapped_UV0: return target.unwrappedMaterial;
            case UVChannelOption.LowPolyUnique_UV1: return target.lowPolyUniqueMaterial;
            case UVChannelOption.LowPolyShared_UV2: return target.lowPolySharedMaterial;
            default: return null;
        }
    }
}