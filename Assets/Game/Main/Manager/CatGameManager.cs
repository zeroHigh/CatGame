using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class CatGameManager : ILSingleton<CatGameManager>
    {
        private Dictionary<int, CatPointView> _catPointViews;
        private List<int> _catPointIds;
        private Coroutine _timerCoroutine;
        private bool _isGameRunning;
        private CatMainView _catMainView;

        public int AddPoint(CatPointView pointView)
        {
            if (_catPointIds == null)
            {
                _catPointIds = new List<int>();
            }
            _catPointIds.Add(_catPointIds.Count);//id 从0开始, 一直增加，保证唯一性

            if (_catPointViews == null)
            {
                _catPointViews = new Dictionary<int, CatPointView>();
            }
            _catPointViews.Add(_catPointIds.Count, pointView);
            return _catPointIds.Count;
        }

        public bool IsGameRunning()
        {
            return _isGameRunning;
        }

        public void RemovePoint(int id)
        {
            if (_catPointViews.Keys.Contains(id))
            {
                _catPointViews.Remove(id);
            }
        }

        //开启计时器
        public void StartGame(CatMainView mainView)
        {
            _isGameRunning = true;
            _catMainView = mainView;
            _timerCoroutine = GameStart.Instance.StartCoroutine(RepeatCoroutineTimer());
        }

        //游戏结束
        public void ExitGame()
        {
            _isGameRunning = false;
            _catPointViews?.Clear();
            _catPointIds?.Clear();
            if(_timerCoroutine != null)
                GameStart.Instance.StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        public void ChangeSpeed()
        {
            if (_catPointViews != null)
            {
                foreach (var item in _catPointViews)
                {
                    item.Value.ChangeSpeed();
                }
            }
        }

        public void ChangeCount()
        {
            _catMainView?.ChangeCount();
        }


        private IEnumerator RepeatCoroutineTimer()
        {
            while (_isGameRunning) // 无限循环
            {
                yield return new WaitForSeconds(2f); // 等待2秒
                JudgePoint(); // 执行任务
            }
        }

        //判断当前目标是否需要创建
        private void JudgePoint()
        {
            if (_catPointViews != null && _catPointViews.Count < _catMainView.LastCount)
            {
                _catMainView.CreatePoint();
            }
        }
    }
}