using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))] // 确保有碰撞器组件
    public class DynamicAntiGravityBall : MonoBehaviour
    {
        [Header("基础设置")]
        public float baseHorizontalSpeed = 8f;    // 水平基础速度
        public float gravityScale = 1f;          // 重力缩放
        public float minBounceHeight = 1.5f;     // 最小弹跳高度
        public float maxBounceHeight = 6f;       // 最大弹跳高度

        [Header("高级设置")]
        public float energyConservation = 0.95f; // 能量守恒系数(0-1)
        public float velocityDeadZone = 0.1f;    // 速度死区阈值

        [Header("调试")]
        public bool showDebugInfo = true;
        public Color debugRayColor = Color.cyan;

        private Rigidbody2D rb;
        private float lastGroundHitTime;
        private Vector2 lastGroundNormal;
        private int splitCount = 0; // 分裂次数计数器
        private const int maxSplitCount = 3; // 最大分裂次数

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // 物理初始化
            rb.gravityScale = gravityScale;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 初始斜向速度
            rb.velocity = new Vector2(
                baseHorizontalSpeed,
                Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * gravityScale) * minBounceHeight)
            );
        }

        public float speed = 90f; // 度/秒

        void Update()
        {
            transform.Rotate(0, 0, speed * Time.deltaTime);
        }

        void FixedUpdate()
        {
            MaintainHorizontalSpeed();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if(IsGroundCollision(collision))
            {
                lastGroundNormal = collision.contacts[0].normal;
                lastGroundHitTime = Time.time;

                // 动态计算反重力力
                float fallSpeed = Mathf.Max(0, -rb.velocity.y);
                float requiredForce = CalculateDynamicForce(fallSpeed);

                // 施加力（考虑碰撞法线角度）
                Vector2 forceDir = Vector2.Reflect(Vector2.up, lastGroundNormal).normalized;
                rb.AddForce(forceDir * requiredForce, ForceMode2D.Impulse);

                if(showDebugInfo)
                {
                    Debug.Log($"碰撞地面! 下落速度: {fallSpeed:F2} | 施加力: {requiredForce:F2}");
                }
            }
            else // 处理墙面碰撞
            {
                Vector2 normal = collision.contacts[0].normal;
                rb.velocity = Vector2.Reflect(rb.velocity.normalized, normal) * rb.velocity.magnitude * energyConservation;
            }
        }

        float CalculateDynamicForce(float fallSpeed)
        {
            // 基础力（保证最小高度）
            float baseForce = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * gravityScale) * minBounceHeight);

            // 动态补偿力（与下落速度成正比）
            float dynamicFactor = Mathf.Clamp01((fallSpeed - 2f) / 10f); // 下落速度2-12m/s映射到0-1
            float extraForce = dynamicFactor * Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * gravityScale) * (maxBounceHeight - minBounceHeight));

            return baseForce + extraForce;
        }

        void MaintainHorizontalSpeed()
        {
            // 仅当水平速度足够大时才修正
            if(Mathf.Abs(rb.velocity.x) > velocityDeadZone)
            {
                float sign = Mathf.Sign(rb.velocity.x);
                rb.velocity = new Vector2(
                    sign * baseHorizontalSpeed,
                    rb.velocity.y
                );
            }
        }

        bool IsGroundCollision(Collision2D collision)
        {
            // 通过法线方向和标签双重检测
            return collision.contacts[0].normal.y > 0.7f || collision.gameObject.CompareTag("Ground");
        }

        void OnMouseDown()
        {
            if (splitCount < maxSplitCount)
            {
                Split();
            }
            // 禁用 OnMouseDown 以避免无限递归
            enabled = false;
        }

        public void Split()
        {
            splitCount++; // 增加分裂次数计数器

            // 获取点击位置
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 创建两个新的小球
            GameObject newBall1 = Instantiate(gameObject, clickPosition, transform.rotation);
            GameObject newBall2 = Instantiate(gameObject, clickPosition, transform.rotation);

            // 设置父节点
            newBall1.transform.parent = transform.parent;
            newBall2.transform.parent = transform.parent;

            // 设置大小
            newBall1.transform.localScale = transform.localScale * 0.8f;
            newBall2.transform.localScale = transform.localScale * 0.8f;

            // 获取新的 Rigidbody2D 组件
            Rigidbody2D rb1 = newBall1.GetComponent<Rigidbody2D>();
            Rigidbody2D rb2 = newBall2.GetComponent<Rigidbody2D>();

            // 施加向上的力
            float upwardForce = 10f; // 向上的力可以根据需要调整
            rb1.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
            rb2.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);

            // 施加向左和向右的力
            float horizontalForce = 5f; // 水平力可以根据需要调整
            rb1.AddForce(Vector2.left * horizontalForce, ForceMode2D.Impulse);
            rb2.AddForce(Vector2.right * horizontalForce, ForceMode2D.Impulse);

            // 禁用碰撞器以防止分裂后的小球相互碰撞
            Collider2D collider1 = newBall1.GetComponent<Collider2D>();
            Collider2D collider2 = newBall2.GetComponent<Collider2D>();
            collider1.enabled = false;
            collider2.enabled = false;

            // 销毁原小球
            Destroy(gameObject);
        }
    }
}
