using System.Runtime.CompilerServices;
using UnityEngine;

public class SwapSkinnedMeshUVs : MonoBehaviour
{
    [SerializeField]
    private int uvChannelToSwap = 1; // 0 for UV0, 1 for UV1, etc.
    void Start()
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
       
        
        
        if (smr != null && smr.sharedMesh != null)
        {
            // Instantiate a unique copy of the mesh so other instances aren't modified
            Mesh uniqueMesh = Instantiate(smr.sharedMesh);

            // Retrieve existing UV channels
            Vector2[] uv0 = uniqueMesh.uv;  // Channel 0
            Vector2[] uv1 = uniqueMesh.uv2; // Channel 1

            // Reassign: Apply UV1 data into Channel 0 (or vice versa)
            if (uv1.Length > 0)
            {
                uniqueMesh.uv = uv1;
            }

            // Assign the modified mesh back to the renderer
            smr.sharedMesh = uniqueMesh;
        }
    }
}