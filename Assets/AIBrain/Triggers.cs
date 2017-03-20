using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class TacticTrigger
{
    public virtual void Serialize(BinaryWriter writer)
    {
        if (behaviorModule != null)
            writer.Write(behaviorModule.GetType().FullName);
        else
            writer.Write("NONE");
    }

    public virtual void Deserialize(BinaryReader reader)
    {
        string fullName = reader.ReadString();

        if (fullName == "NONE")
        {
            behaviorModule = null;
        }
        else
        {
            var type = Type.GetType(fullName);
            behaviorModule = (BehaviorModule)Activator.CreateInstance(type);
        }
    }

    public abstract bool IsValid(Brain origin);

    public BehaviorModule behaviorModule;
}


public class DistanceTrigger : TacticTrigger
{
    public string target = "";
    public float distance = 0;

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);

        target = reader.ReadString();
        distance = reader.ReadSingle();   
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);

        writer.Write(target);
        writer.Write(distance);
    }

    public override string ToString()
    {
        return "When at distance " + distance + " of " + target;
    }

    public override bool IsValid(Brain origin)
    {
        if (behaviorModule == null)
            return false;

        for(int i = 0; i < origin.currentTargetables.Count; ++i)
        {
            Targetable targetable = origin.currentTargetables[i];
            if (targetable.category == target)
            {
                if((targetable.transform.position - origin.transform.position).sqrMagnitude <= distance*distance)
                {
                    origin.currentTarget = targetable;
                    return true;
                }
            }
        }

        return false;
    }
}