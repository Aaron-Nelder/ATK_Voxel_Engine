using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [field: SerializeField] public Camera Camera { get; private set; }
    [SerializeField] RenderTexture camTexture;

    //Highlight variables
    [SerializeField] GameObject highlightVoxelPrefab;


    void Awake()
    {
        if (Camera == null)
            Camera = GetComponent<Camera>();

        Instantiate(highlightVoxelPrefab).GetComponent<VoxelHighlight>().Init();
    }
}