using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour
    {
        [Header("物理设置")]
        public float moveSpeed = 8f;          // 恒定运动速度
        public float gravityScale = 1f;       // 重力缩放
        public Vector2 initialForce = new Vector2(10f, 5f); // 初始力

        [Header("高级设置")]
        public bool showDebugInfo = true;     // 显示调试信息

        private Rigidbody2D rb;
        private Vector2 lastFrameVelocity;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // 初始物理设置
            rb.gravityScale = gravityScale;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 创建完美弹性材质
            PhysicsMaterial2D material = new PhysicsMaterial2D();
            material.bounciness = 1f;
            material.friction = 0f;
            GetComponent<Collider2D>().sharedMaterial = material;

            // 施加初始力
            rb.AddForce(initialForce, ForceMode2D.Impulse);
        }

        void Update()
        {
            // 记录上一帧速度用于碰撞计算
            lastFrameVelocity = rb.velocity;

            // // 调试信息
            // if(showDebugInfo) {
            //     Debug.DrawRay(transform.position, rb.velocity.normalized * 2f, Color.red);
            // }
        }

        void FixedUpdate()
        {
            // 保持恒定速度（同时允许重力影响方向）
            if(rb.velocity.magnitude > 0.1f)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // 计算碰撞反射方向
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectedDir = Vector2.Reflect(lastFrameVelocity.normalized, normal);

            // 应用新速度（保持大小不变）
            rb.velocity = reflectedDir * moveSpeed;

            // 调试碰撞信息
            if(showDebugInfo) {
                Debug.Log($"碰撞物体: {collision.gameObject.name} | 新速度: {rb.velocity}");
            }
        }
    }
}