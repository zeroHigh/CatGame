using UnityEngine;

namespace Game
{
    public static class UnityObjectHelper
    {
        public static void DestroyGameObjectSafe(GameObject gameObject)
        {
            if (gameObject == null)
                return;
            GameObject.Destroy(gameObject);
        }

        public static void DestroyObjectAllChild(Transform root)
        {
            if (root == null)
                return;
            while (root.childCount > 0)
                GameObject.Destroy(root.GetChild(0));
        }

        public static Texture2D CreateExternalTexture(Texture2D srcTex, TextureFormat textureFormat)
        {
            return Texture2D.CreateExternalTexture(srcTex.width, srcTex.height, textureFormat, false, false, srcTex.GetNativeTexturePtr());
        }

        public static void SetParent(Transform childTransform, Transform parent, Vector3 pos = default, bool resetScale = true)
        {
            childTransform.SetParent(parent, false);
            childTransform.localPosition = pos;
            childTransform.localEulerAngles = Vector3.zero;
            if(resetScale)
                childTransform.localScale = Vector3.one;
        }

        public static T Find<T>(Transform root, string path) where T : Object
        {
            T t = root.Find(path)?.GetComponent<T>();
            return t;
        }
    }
}