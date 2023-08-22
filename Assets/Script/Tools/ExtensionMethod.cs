using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//扩展方法不用继承任何其他的类，必须为static
public static class ExtensionMethod
{
    private const float dotThreshold = 0.5f;

    public static bool IsFacingTarget(this Transform transform,Transform target)    //this Transform transform是指要被扩展的类，Transform target才是要传入的变量
    {
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorToTarget);

        return dot >= dotThreshold;
    }
}
