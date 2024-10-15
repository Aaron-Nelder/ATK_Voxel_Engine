using UnityEngine;

[CreateAssetMenu(fileName = "Combined_Material", menuName = DebugHelper.MENU_NAME + "/Combined_Material")]
public class CombinedMaterial_SO : ScriptableObject
{
    [SerializeField] Material material;
    public Material Material => material;

    [SerializeField] int gridSize;
    public int GridSize => gridSize;
}
