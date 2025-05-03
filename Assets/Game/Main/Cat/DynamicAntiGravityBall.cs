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
        public float energyConservation = 1f; // 能量守恒系数(0-1)
        public float velocityDeadZone = 0.5f;    // 速度死区阈值
        public float wallBounceForce = 5f;

        private Rigidbody2D rb;
        private float lastGroundHitTime;
        private Vector2 lastGroundNormal;
        private const int MaxSplitCount = 3; // 最大分裂次数
        private Transform ballParent;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // 物理初始化
            rb.gravityScale = gravityScale;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 初始斜向速度
            if (CatGameManager.Instance.IsNeedCreateBall)
            {
                CatGameManager.Instance.IsNeedCreateBall = false;

                rb.velocity = new Vector2(
                    baseHorizontalSpeed,
                    Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * gravityScale) * minBounceHeight)
                );
            }

            // 设置碰撞层
            gameObject.layer = LayerMask.NameToLayer("BallLayer");

            //获取第一个小球父节点
            ballParent = transform.parent;
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
            if (collision.gameObject.layer == LayerMask.NameToLayer("WallGroundLayer"))
            {
                Vector2 normal = collision.contacts[0].normal;

                if (IsGroundCollision(normal))
                {
                    lastGroundNormal = normal;
                    lastGroundHitTime = Time.time;

                    // 动态计算反重力力
                    float fallSpeed = Mathf.Max(0, -rb.velocity.y);
                    float requiredForce = CalculateDynamicForce(fallSpeed);

                    // 施加力（考虑碰撞法线角度）
                    Vector2 forceDir = Vector2.Reflect(Vector2.up, lastGroundNormal).normalized;
                    rb.AddForce(forceDir * requiredForce, ForceMode2D.Impulse);
                }
                else
                {
                    // 处理墙面碰撞
                    Vector2 reflectedVelocity = Vector2.Reflect(rb.velocity.normalized, normal) * rb.velocity.magnitude * energyConservation;
                    // 不直接设置 velocity，而是通过 AddForce 增强反弹
                    rb.velocity = rb.velocity.magnitude * reflectedVelocity.normalized; // 保留方向，归一化速度
                    rb.AddForce(-normal * wallBounceForce * 2, ForceMode2D.Impulse);

                }
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("BallLayer"))
            {
                // 忽略小球之间的碰撞
                return;
            }
            else // 处理其他类型的碰撞
            {
                Vector2 normal = collision.contacts[0].normal;
                rb.velocity = Vector2.Reflect(rb.velocity.normalized, normal) * rb.velocity.magnitude * energyConservation;
            }
        }

        bool IsGroundCollision(Vector2 normal)
        {
            // 通过法线方向判断是否是地面碰撞
            return normal.y > 0.7f;
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
            if (Mathf.Abs(rb.velocity.x) > velocityDeadZone)
            {
                float sign = Mathf.Sign(rb.velocity.x);
                rb.velocity = new Vector2(
                    sign * baseHorizontalSpeed,
                    rb.velocity.y
                );
            }
        }

        void OnMouseDown()
        {

            // 分裂
            var split1 = gameObject.name.Split("_")[0];
            var split2 = gameObject.name.Split("_")[1];
            if (int.Parse(split2) < MaxSplitCount)
            {
                var spBall = int.Parse(split2) + 1;
                Split(split1 + "_" + spBall);
                string pathHitEffect = "Prefabs/Effects/HitParticle";
                HitEffect(pathHitEffect);

            }
            else
            {

                CatGameManager.Instance.UpdateScore();
                Destroy(gameObject);
                var pathDestroyEffect = "Prefabs/Effects/DestroyParticle";
                HitEffect(pathDestroyEffect);
            }
            // 禁用 OnMouseDown 以避免无限递归
            enabled = false;
        }

        private void Split(string newBallName)
        {
            // 获取点击位置
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 创建两个新的小球
            GameObject newBall1 = Instantiate(gameObject, clickPosition, transform.rotation);
            GameObject newBall2 = Instantiate(gameObject, clickPosition, transform.rotation);

            // 确保两个小球都被正确实例化
            if (newBall1 == null || newBall2 == null)
            {
                Debug.LogError("Failed to instantiate one or both new balls.");
                return;
            }

            newBall1.name = newBallName;
            newBall2.name = newBallName;
            // 设置父节点
            newBall1.transform.parent = ballParent;
            newBall2.transform.parent = ballParent;

            // 设置大小
            newBall1.transform.localScale = transform.localScale * 0.8f;
            newBall2.transform.localScale = transform.localScale * 0.8f;

            // 获取新的 Rigidbody2D 组件
            var rb1 = newBall1.GetComponent<Rigidbody2D>();
            var rb2 = newBall2.GetComponent<Rigidbody2D>();

            // 设置新的小球的位置
            var upwardForce = 0.4f; // 可以根据需要调整这个值
            var horizontalForce = 5f; // 独立控制水平方向力

            upwardForce *= Random.Range(0.5f, 1.2f);
            horizontalForce *= Random.Range(0.5f, 1.2f);

            rb1.AddForce(new Vector2(-horizontalForce, upwardForce), ForceMode2D.Impulse);
            rb2.AddForce(new Vector2(horizontalForce, upwardForce), ForceMode2D.Impulse);

            // 禁用 OnMouseDown 以避免无限递归
            enabled = false;

            // 销毁原小球
            Destroy(gameObject);
        }

        private void HitEffect(string path)
        {
            Debug.Log("hit effect played!!");
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            GameObject hitParticle = (GameObject)Instantiate(Resources.Load(path));
            hitParticle.transform.SetParent(GameObject.Find("BottomRoot").transform);
            hitParticle.transform.position = clickPosition;
            hitParticle.transform.localScale = new Vector3(1, 1, 1);
            hitParticle.GetComponent<ParticleSystem>().Play();
        }


    }
}
