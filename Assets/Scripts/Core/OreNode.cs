using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OreNode : MonoBehaviour
{
    [Tooltip("Tổng số quặng trong mỏ. Nếu < 0 nghĩa là vô hạn.")]
    public int totalOre = -1;

    public int Mine(int amount)
    {
        if (amount <= 0) return 0;

        if (totalOre < 0)
        {
            // Mỏ vô hạn
            return amount;
        }

        int mined = Mathf.Min(amount, totalOre);
        totalOre -= mined;

        if (totalOre == 0)
        {
            Debug.Log("Mỏ quặng đã cạn.");
            // TODO: đổi sprite / disable collider / Destroy(gameObject) tùy ý
        }

        return mined;
    }
}
