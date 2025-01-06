using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using UnityEngine;

namespace Higo.UI
{

    [CustomPropertyDrawer(typeof(CreateAssetAttribute))]
    public class CreateAssetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new PropertyField(property);
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != null)
            {
                return root;
            }

            var typedAttr = attribute as CreateAssetAttribute;
            var button = new Button(() =>
            {
                // if (AssetDatabase.IsValidFolder($"{typedAttr.Path}"))
                // {
                //     EditorUtility.DisplayDialog("Fail!", "The folder is not exist! (Inside the Assets)", "Ok");
                //     return;
                // }
                var newPath = $"Assets/{typedAttr.Path}/New Asset.{typedAttr.Ext}";
                var newObj = Activator.CreateInstance(typedAttr.Type) as UnityEngine.Object;
                ProjectWindowUtil.CreateAsset(newObj, newPath);
                property.objectReferenceValue = Selection.activeObject;
                Selection.activeObject = property.serializedObject.targetObject;
                // AssetDatabase.CreateAsset(newObj, newPath);
                // AssetDatabase.Refresh();
                // var asset = AssetDatabase.LoadMainAssetAtPath(newPath);
                // property.objectReferenceValue = asset;
                // Selection.activeObject = asset;
                // AssetDatabase.rename
            });
            root.Add(button);
            button.tooltip = $"Store in \"Assets/{typedAttr.Path}\"";
            button.style.width = 200;
            button.text = "Create";

            button.AddToClassList("unity-base-field__input");
            // button.AddToClassList("unity-object-field__input");

            button.style.right = 0;

            return root;
        }
    }

}