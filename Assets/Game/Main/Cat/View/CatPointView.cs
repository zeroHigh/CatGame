using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CatPointView: UIBaseView
    {
        private Button _btnPoint;
        private int _pointId;
        private float _speed; // 增加速度以获得更加丝滑的运动
        private Vector3 _targetPosition;

        protected override void ParseComponent()
        {
            _btnPoint = transform.GetComponent<Button>();
            transform.GetComponent<Image>().SetNativeSize();
        }

        protected override void Refresh(params object[] arg)
        {
            _pointId = (int)arg[0];
            UpdateSpeed();
            GameStart.Instance.StartCoroutine(MoveSmoothly());
        }

        private IEnumerator MoveSmoothly()
        {
            while (CatGameManager.Instance.IsGameRunning() && transform != null)
            {
                _targetPosition = (Vector2)transform.position + Random.insideUnitCircle.normalized * 5.0f;
                float distance = Vector2.Distance(transform.position, _targetPosition);

                while (distance > 0.1f && CatGameManager.Instance.IsGameRunning() && transform != null)
                {
                    CheckCollision();
                    transform.position = Vector2.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
                    distance = Vector2.Distance(transform.position, _targetPosition);

                    // 计算旋转角度
                    float angle = Mathf.Atan2(_targetPosition.y - transform.position.y, _targetPosition.x - transform.position.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 2f);

                    yield return new WaitForEndOfFrame(); // 确保在每一帧结束时执行
                }

                yield return new WaitForSeconds(Random.Range(0.1f, 0.2f)); // 缩短移动间隔时间
            }
        }

        private void CheckCollision()
        {
            if(!CatGameManager.Instance.IsGameRunning() || transform == null) return;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, _targetPosition - transform.position, _speed * Time.deltaTime);
            if (hit.collider != null)
            {
                Vector3 newDirection = Vector3.Reflect(_targetPosition - transform.position, hit.normal).normalized;
                _targetPosition = (new Vector3(hit.point.x, hit.point.y) + newDirection * 0.5f); // 改变新的目标位置
            }
        }

        public void UpdateSpeed()
        {
            _speed = PlayerPrefs.GetInt(GlobalGameSetting.SettingsKey.SPEED_SETTINGS, 1) * 1.5f;
        }

        protected override void AddEvent()
        {
            ListenButton(_btnPoint, OnPointClick);
        }

        protected override void RemoveEvent()
        {
            UnListenButton(_btnPoint, OnPointClick);
        }

        private void OnPointClick()
        {
            CatGameManager.Instance.RemovePoint(_pointId);
            Dispose();
        }
    }
}