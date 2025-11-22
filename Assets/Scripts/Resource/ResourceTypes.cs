using UnityEngine;

/// Loại tài nguyên cơ bản (giờ mới có mỗi Ore, sau này bạn thêm IronIngot, CopperOre...)
public enum ResourceType
{
    None = 0,
    Ore = 1
}

[System.Serializable]
public struct ResourceStack
{
    public ResourceType type;
    public int amount;

    public bool IsEmpty => type == ResourceType.None || amount <= 0;

    public ResourceStack(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }

    public static readonly ResourceStack Empty = new ResourceStack(ResourceType.None, 0);
}

/// Nơi **nhận** tài nguyên (Core, belt, kho...)
public interface IResourceSink
{
    /// Kiểm tra sơ bộ xem có thể nhận stack này không
    bool CanAccept(ResourceStack stack);

    /// Thực sự nhận tài nguyên, trả về số lượng đã nhận (<= stack.amount)
    int Accept(ResourceStack stack);
}

/// Nơi **xuất** tài nguyên ra (Drill, kho, Core nếu muốn xuất ngược)
public interface IResourceSource
{
    /// Có đủ ít nhất minAmount tài nguyên type này không?
    bool HasResource(ResourceType type, int minAmount);

    /// Lấy ra tối đa maxAmount tài nguyên type này
    ResourceStack Take(ResourceType type, int maxAmount);
}
