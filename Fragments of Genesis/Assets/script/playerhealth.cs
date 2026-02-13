using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    public float invincibleTime = 1f;

    [Header("Knockback")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.15f;

    private int currentHealth;
    private bool isInvincible = false;
    private bool isKnockback = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector2 hitPosition)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("현재 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibleCoroutine());
        StartCoroutine(KnockbackCoroutine(hitPosition));
    }

    IEnumerator KnockbackCoroutine(Vector2 hitPosition)
    {
        isKnockback = true;

        rb.velocity = Vector2.zero;

        float direction = transform.position.x > hitPosition.x ? 1f : -1f;
        rb.AddForce(new Vector2(direction * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockback = false;
    }

    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < invincibleTime)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
        // 리스폰 or 게임오버 처리
    }

    public bool IsKnockback()
    {
        return isKnockback;
    }
}
