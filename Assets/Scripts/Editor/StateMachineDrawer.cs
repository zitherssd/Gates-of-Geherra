//using Assets.Scripts.Battle.State;
//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(StateMachine))]
//public class StateMachineDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        EditorGUI.BeginProperty(position, label, property);

//        // Find the "CurrentState" property
//        SerializedObject currentState = property.serializedObject.
//        SerializedPropertyType currentStateProperty = property.FindPropertyRelative("CurrentState").propertyType;

//        // Ensure currentStateProperty is not null
//        if (currentStateProperty != null)
//        {
//            // Get the object reference from the property
//            Object currentStateObject = currentStateProperty.ToString();

//            // Get the type name or "None" if currentStateObject is null
//            string currentStateName = currentStateObject != null ? currentStateObject.GetType().Name : "None";

//            // Display the current state in the inspector
//            EditorGUI.LabelField(position, new GUIContent("Current State"), new GUIContent(currentStateName));
//        }
//        else
//        {
//            // Display a warning if currentStateProperty is null
//            EditorGUI.LabelField(position, new GUIContent("Current State"), new GUIContent("Error: CurrentState property not found"));
//        }

//        EditorGUI.EndProperty();
//    }
//}
