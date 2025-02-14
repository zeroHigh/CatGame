using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class DefaultResLoader : BaseLoader
    {
        protected override T OnLoadResWithPath<T>(string fullPath, ResType type)
        {
            fullPath += GetFileExtension(type);
            return LoadResByFileName<T>(fullPath);
        }

        protected override T OnLoadResByFileName<T>(string fileName)
        {
            var fullPath = FindPath(fileName);
            if (string.IsNullOrEmpty(fullPath))
            {
                Logger.LogRed("[DefaultLoader.LoadRes() => 找不到路径，fileName: " + fileName + "]");
                return null;
            }
    #if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath(fullPath, typeof(T)) as T;
    #else
            return default(T);
    #endif
        }

        public override void Init()
        {
            base.Init();
            ReadQuestionFolderConfig();
        }

        private static string ConfigAssetPath = Application.dataPath + "/GameEditor/Editor/ResFolderRegexReplace/QuestionConfig.txt";
        private string[] QuestionSubFolders = null;
        /// <summary>
        /// 读取question下可能出现的文件夹
        /// </summary>
        private void ReadQuestionFolderConfig()
        {
            if (File.Exists(ConfigAssetPath))
            {
                QuestionSubFolders = File.ReadAllLines(ConfigAssetPath);
            }
        }

        private const string Question = "Question";
        private string FindPath(string fileName)
        {
            if (resName2Path.ContainsKey(fileName))
                return resName2Path[fileName];
            if (File.Exists(fileName))//兼容一下，如果一开始传入全路径的话，就不需要遍历了
            {
                resName2Path[fileName] = fileName;
                return fileName;
            }
            for (var i = 0; i < searchPaths.Count; i++)
            {
                string searchPath = searchPaths[i];
                string existPath = searchPath + fileName;    // 这里就是常规的路径匹配
                if (File.Exists(existPath))
                {
                    resName2Path[fileName] = existPath;
                    return existPath;
                }
                // 这里处理下JoJoReadRes/Res   /Question文件夹下多一层的异常  fileName带question
                // 大概逻辑就是在Question后插入一层可能出现的文件夹 这个走配置 编辑器下读取配置
                if (fileName.Contains(Question) && QuestionSubFolders != null)
                {
                    int questionIndex = fileName.IndexOf(Question);    // 不用判断-1
                    int insertIndex = questionIndex + Question.Length ;
                    foreach (string folderName in QuestionSubFolders)
                    {
                        string tempFileName = fileName.Insert(insertIndex, folderName);
                        string tempFullPath = searchPath + tempFileName;
                        if (File.Exists(tempFullPath))
                        {
                            resName2Path[fileName] = tempFullPath;
                            return tempFullPath;
                        }
                    }
                }
            }
            return null;
        }

        private string GetFileExtension(ResType type)
        {
            var suffix = "";
            if (type == ResType.Atlas)
                suffix = ".spriteAtlas";
            else if(type == ResType.Material)
                suffix = ".mat";
            else if(type == ResType.Video)
                suffix = ".mp4";
            else if(type == ResType.Bytes)
                suffix = ".bytes";
            else if(type == ResType.Txt)
                suffix = ".txt";
            else if(type == ResType.Asset)
                suffix = ".asset";
            else if(type == ResType.Audio)
                suffix = ".mp3";
            else
                suffix = ".prefab";
            return suffix;
        }

        public override void UnloadUnusedRes()
        {
            Resources.UnloadUnusedAssets();
        }
        
        public override void PreLoadAtlas(string atlasPath)
        {
            
        }

        public override void UnloadAtlas(string atlasPath)
        {
        }

        public override Sprite LoadSprite(string imgPath)
        {
            return ResourceLoader.Instance.LoadSprite(imgPath);
        }

         protected override void OnLoadResWithPathAsync(string path, System.Type type, System.Action<UnityEngine.Object> action, string assetbundlePath = null)
        {
            AssetBundleUtil.LoadResByFileAsync(path,type,action, assetbundlePath);
        }
    }
}