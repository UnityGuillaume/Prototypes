using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviorModule
{
    public abstract void Execute(Brain origin);
}

public class FollowModule : BehaviorModule
{
    public override void Execute(Brain origin)
    {
        Vector3 towardTarget = (origin.currentTarget.transform.position - origin.transform.position).normalized;
        origin.transform.forward = Vector3.RotateTowards(origin.transform.forward, towardTarget, 45.0f * Time.deltaTime, 1.0f);
        origin.transform.position = Vector3.MoveTowards(origin.transform.position, origin.currentTarget.transform.position, 1.0f * Time.deltaTime);
    }

    public override string ToString()
    {
        return "Follow";
    }
}
