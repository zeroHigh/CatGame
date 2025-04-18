using UnityEngine;

namespace Game
{

[RequireComponent(typeof(Rigidbody2D))]
public class UltimateAntiGravityBall : MonoBehaviour
{
    [Header("物理参数")]
    [Range(0.001f, 2f)] public float mass = 0.5f;      // 质量（支持极小数）
    public float minBounceHeight = 1f;                  // 最小弹跳高度
    public float maxBounceHeight = 3f;                  // 最大弹跳高度

    [Header("高级控制")]
    public AnimationCurve massToForceMultiplier =       // 质量-反弹力曲线
        new AnimationCurve(new Keyframe(0.001f, 2f), new Keyframe(1f, 1f), new Keyframe(2f, 0.5f));
    public float speedToForceRatio = 0.3f;              // 速度对反弹的影响

    private Rigidbody2D rb;
    private float initialGravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
        ApplyMassSettings();
    }

    void ApplyMassSettings()
    {
        rb.mass = mass;
        // 重力补偿公式（关键优化点！）
        rb.gravityScale = initialGravityScale * Mathf.Pow(mass, 0.33f); // 立方根补偿
        rb.drag = 0.01f * Mathf.Sqrt(mass);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(IsGroundCollision(collision))
        {
            Vector2 normal = collision.contacts[0].normal;
            float impactSpeed = Mathf.Max(0, -Vector2.Dot(rb.velocity, normal));

            // 动态计算反弹力（三部分叠加）
            float force = CalculateOptimizedForce(impactSpeed);
            Vector2 forceDir = Vector2.Reflect(Vector2.up, normal).normalized;

            rb.AddForce(forceDir * force, ForceMode2D.Impulse);
        }
    }

    float CalculateOptimizedForce(float impactSpeed)
    {
        // 1. 基础力（保证最小高度）
        float baseForce = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * rb.gravityScale) * minBounceHeight);

        // 2. 质量补偿（从曲线获取）
        float massFactor = massToForceMultiplier.Evaluate(mass);

        // 3. 速度补偿
        float speedFactor = 1 + impactSpeed * speedToForceRatio;

        return baseForce * massFactor * speedFactor;
    }

    bool IsGroundCollision(Collision2D collision)
    {
        return collision.contacts[0].normal.y > 0.7f;
    }

    // 调试信息
    void OnGUI()
    {
        GUILayout.Label($"质量: {mass}\n当前速度: {rb.velocity.magnitude:F2}");
    }
}
}