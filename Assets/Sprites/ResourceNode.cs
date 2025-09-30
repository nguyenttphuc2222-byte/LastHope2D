using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public enum ResourceType { Copper, Iron, Graphite }
    public ResourceType type;

    [Tooltip("Số lượng vô hạn thì để -1, còn muốn giới hạn thì điền số.")]
    public int amount = -1;

    void OnDrawGizmos()
    {
        // Vẽ nhãn trong Scene view để dễ phân biệt
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, type.ToString());
    }
}