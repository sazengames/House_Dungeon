using UnityEngine;

public class SwapUVChannels : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        // Retrieve UV data from different channels
        Vector2[] uv0 = mesh.uv;  // Primary channel (TEXCOORD0)
        Vector2[] uv1 = mesh.uv2; // Secondary channel (TEXCOORD1)

        // Assign UV1 data into UV0 (or vice versa)
        mesh.uv = uv1;
    }
}