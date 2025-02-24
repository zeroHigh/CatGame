using UnityEngine;
 
namespace UnityEditor.UI
{
    [CustomEditor(typeof(ButtonExtension), true)]
    [CanEditMultipleObjects]
    public class ButtonExInspector : SelectableEditor
    {
        private ButtonExtension buttonExtension;
 
        SerializedProperty onDoubleClickProperty;
        SerializedProperty onClickProperty;
        SerializedProperty onLongPressProperty;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            onDoubleClickProperty = serializedObject.FindProperty("doubleClickEvent");
            onClickProperty = serializedObject.FindProperty("m_OnClick");
            onLongPressProperty = serializedObject.FindProperty("longPressEvent");
        }
 
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            buttonExtension = (ButtonExtension)target;
            EditorGUILayout.Space();
 
            buttonExtension.singleClickEnabled = EditorGUILayout.Toggle("启用单击", buttonExtension.singleClickEnabled);
            if (buttonExtension.singleClickEnabled)
            {
                buttonExtension.doubleClickEnabled = false;
                buttonExtension.longPressEnabled = false;
                EditorGUILayout.PropertyField(onClickProperty);  
            }
            
            buttonExtension.doubleClickEnabled = EditorGUILayout.Toggle("启用双击", buttonExtension.doubleClickEnabled);
            if (buttonExtension.doubleClickEnabled)
            {
                buttonExtension.doubleClickTime = EditorGUILayout.FloatField("双击间隔", buttonExtension.doubleClickTime);
                buttonExtension.singleClickEnabled = false;
                buttonExtension.longPressEnabled = false;
                EditorGUILayout.PropertyField(onDoubleClickProperty);
            }
            
            buttonExtension.longPressEnabled = EditorGUILayout.Toggle("启用长按", buttonExtension.longPressEnabled);
            if(buttonExtension.longPressEnabled)
            {
                buttonExtension.minPressTime = EditorGUILayout.FloatField("长按时间", buttonExtension.minPressTime);
                buttonExtension.singleClickEnabled = false;
                buttonExtension.doubleClickEnabled = false;
                EditorGUILayout.PropertyField(onLongPressProperty);
            }
            
            serializedObject.ApplyModifiedProperties();
            if(GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }   
}
 
 