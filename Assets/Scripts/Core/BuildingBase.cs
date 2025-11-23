using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("Kích thước tính theo block (ô grid). Ví dụ tường = (1,1), turret = (2,2).")]
    public Vector2Int size = Vector2Int.one;

    // Ô anchor (góc dưới - trái) của building trên grid
    public Vector2Int AnchorCell { get; private set; }

    [Header("Build Cost")]
    public int buildCostOre = 0;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("SFX")]
    [Tooltip("Âm thanh phát khi công trình này bị phá hủy.")]
    public AudioClip destroyedSfx;
    [Range(0f, 1f)]
    public float destroyedSfxVolume = 1f;


    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void PlayDestroyedSfx()
    {
        if (destroyedSfx == null) return;
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySfx(destroyedSfx, destroyedSfxVolume);
    }


    public virtual void OnPlaced(Vector2Int anchorCell)
    {
        AnchorCell = anchorCell;
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (currentHealth == 0)
        {
            OnDestroyed();
        }
    }

    public virtual void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        //// nếu bạn có tham chiếu healthBar trong BuildingBase:
        //if (healthBar != null)
        //{
        //    float normalized = currentHealth / (float)maxHealth;
        //    healthBar.SetValue(normalized);
        //}
    }


    protected virtual void OnDestroyed()
    {
        PlayDestroyedSfx();
        // Xoá khỏi grid
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearBuilding(this, AnchorCell);
        }

        Destroy(gameObject);
    }
    // Nếu sau này cần di chuyển building, thể thêm OnMoved, OnRemoved...
}
