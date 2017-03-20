using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Tactic", menuName = "Tactic")]
public class Tactic : ScriptableObject, ISerializationCallbackReceiver
{
    public TacticTrigger[] triggers;

    public byte[] data;

    public void OnBeforeSerialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        if (triggers != null)
        {
            writer.Write(triggers.Length);
            for (int i = 0; i < triggers.Length; ++i)
            {
                writer.Write(triggers[i].GetType().FullName);
                triggers[i].Serialize(writer);
            }
        }

        data = stream.ToArray();
    }

    public void OnAfterDeserialize()
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);

        int count = reader.ReadInt32();
        triggers = new TacticTrigger[count];
        for (int  i = 0; i < count; ++i)
        {
            string typeName = reader.ReadString();
            var type = Type.GetType(typeName);
            TacticTrigger t = (TacticTrigger)Activator.CreateInstance(type);

            triggers[i] = t;
            triggers[i].Deserialize(reader);
        }
    }
}
