using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Utils : MonoBehaviour
{

    public static Vector3 InputRelativeToCamera(Transform camTransform, Vector2 input)
    {
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        //proyectamos en plano
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        input.Normalize();

        //dirección en world space
        Vector3 desiredMoveDirection = forward * input.y + right * input.x;

        return desiredMoveDirection;
    }



#if UNITY_EDITOR
    [MenuItem("Extra Tools/Clear Console %q")] // CTRL + Q
    public static void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
#endif
}
