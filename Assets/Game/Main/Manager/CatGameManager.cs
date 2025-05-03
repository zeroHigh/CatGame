using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class CatGameManager : Singleton<CatGameManager>
    {
        private Dictionary<int, CatPointViewOld> _catPointViews;
        private List<int> _catPointIds;
        private Coroutine _timerCoroutine;
        private bool _isGameRunning;
        private CatMainView _catMainView;
        private int _score;

        public int AddPoint(CatPointViewOld pointViewOld)
        {
            if (_catPointIds == null)
            {
                _catPointIds = new List<int>();
            }
            _catPointIds.Add(_catPointIds.Count);//id 从0开始, 一直增加，保证唯一性

            if (_catPointViews == null)
            {
                _catPointViews = new Dictionary<int, CatPointViewOld>();
            }
            _catPointViews.Add(_catPointIds.Count, pointViewOld);
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
                _score++;
                _catMainView?.UpdateScore(_score);
            }
        }

        public void UpdateScore()
        {
            _score++;
            _catMainView?.UpdateScore(_score);

            LastBallNum--; //场上剩余的球数量
            if (LastBallNum < MinBallCount)
            {
                CreateNewBall();
            }
        }

        //开启计时器
        public void StartGame(CatMainView mainView)
        {
            _isGameRunning = true;
            _score = 0;
            _catMainView = mainView;
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
                    item.Value.UpdateSpeed();
                }
            }
        }

        public bool IsNeedCreateBall = true;
        public const int BALL_LEVEL = 3; //小球是否需要重新创建条件

        /// <summary>
        /// 创建新的小球
        /// </summary>
        private void CreateNewBall()
        {
            IsNeedCreateBall = true;
            _catMainView.PutNewBall();
        }

        public int LastBallNum;
        private const int MinBallCount = 4; //最少场上的球数量
    }
}