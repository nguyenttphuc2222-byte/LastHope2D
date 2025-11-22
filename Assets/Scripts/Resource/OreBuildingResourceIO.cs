using UnityEngine;

[RequireComponent(typeof(OreBuilding))]
public class OreBuildingResourceIO : MonoBehaviour, IResourceSource, IResourceSink
{
    private OreBuilding drill;

    private void Awake()
    {
        drill = GetComponent<OreBuilding>();
    }

    // Drill hiện tại không nhận ore từ bên ngoài, chỉ xuất ra
    public bool CanAccept(ResourceStack stack)
    {
        // Sau này nếu bạn muốn cho drill nhập ore (ví dụ dùng làm nhiên liệu) thì sửa hàm này
        return false;
    }

    public int Accept(ResourceStack stack)
    {
        // Không nhận gì cả ở phiên bản hiện tại
        return 0;
    }

    public bool HasResource(ResourceType type, int minAmount)
    {
        if (type != ResourceType.Ore) return false;
        return drill.storedOre >= minAmount;
    }

    public ResourceStack Take(ResourceType type, int maxAmount)
    {
        if (type != ResourceType.Ore || maxAmount <= 0)
            return ResourceStack.Empty;

        int taken = drill.TakeOre(maxAmount);
        if (taken <= 0) return ResourceStack.Empty;

        return new ResourceStack(ResourceType.Ore, taken);
    }
}
