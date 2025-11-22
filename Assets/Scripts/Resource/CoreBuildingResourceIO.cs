using UnityEngine;

[RequireComponent(typeof(CoreBuilding))]
public class CoreBuildingResourceIO : MonoBehaviour, IResourceSink, IResourceSource
{
    private CoreBuilding core;

    private void Awake()
    {
        core = GetComponent<CoreBuilding>();
    }

    // ---- Sink: nhận ore từ băng chuyền / chỗ khác ----
    public bool CanAccept(ResourceStack stack)
    {
        return stack.type == ResourceType.Ore && stack.amount > 0;
    }

    public int Accept(ResourceStack stack)
    {
        if (!CanAccept(stack)) return 0;

        core.AddOre(stack.amount);
        return stack.amount;
    }

    // ---- Source: xuất ore ra băng chuyền nếu cần (sau này dùng) ----
    public bool HasResource(ResourceType type, int minAmount)
    {
        if (type != ResourceType.Ore) return false;
        return core.oreAmount >= minAmount;
    }

    public ResourceStack Take(ResourceType type, int maxAmount)
    {
        if (type != ResourceType.Ore || maxAmount <= 0)
            return ResourceStack.Empty;

        int available = Mathf.Min(core.oreAmount, maxAmount);
        if (available <= 0) return ResourceStack.Empty;

        // TrySpendOre có bảo vệ rồi nên dùng luôn
        if (!core.TrySpendOre(available))
            return ResourceStack.Empty;

        return new ResourceStack(ResourceType.Ore, available);
    }
}
