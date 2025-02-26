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
            _speed = PlayerPrefs.GetInt(GlobalGameSetting.SettingsKey.SPEED_SETTINGS, 1);
            GameStart.Instance.StartCoroutine(MoveSmoothly());
        }

        private IEnumerator MoveSmoothly()
        {
            while (CatGameManager.Instance.IsGameRunning())
            {
                if (!CatGameManager.Instance.IsGameRunning() || transform == null)
                {
                    yield break;
                }

                _targetPosition = transform.position + new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), 0).normalized * 2.0f;
                var distance = Vector3.Distance(transform.position, _targetPosition);
                while (distance > 0.1f)
                {
                    CheckCollision();
                    if (!CatGameManager.Instance.IsGameRunning() || transform == null)
                    {
                        yield break;
                    }
                    transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
                    distance = Vector3.Distance(transform.position, _targetPosition);
                    yield return null; // 减少 WaitForSeconds() 的间隔
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

        public void ChangeSpeed()
        {
            _speed = PlayerPrefs.GetInt(GlobalGameSetting.SettingsKey.SPEED_SETTINGS, 1);
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
            Debug.LogError("Point");
            CatGameManager.Instance.RemovePoint(_pointId);
            Dispose();
        }
    }
}