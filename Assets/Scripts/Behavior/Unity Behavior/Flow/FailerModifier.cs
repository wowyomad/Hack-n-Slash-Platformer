using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Failer", category: "Flow", id: "a426fcb0d3b51b102acdb5ea4852472e")]
public partial class FailerModifier : Modifier
{

   protected override Status OnStart()
        {
            if (Child == null)
            {
                return Status.Failure; 
            }
            Status childStatus = StartNode(Child);
            return FailIfChildComplete(childStatus);
        }

        protected override Status OnUpdate()
        {
            return FailIfChildComplete(Child.CurrentStatus);
        }

        private Status FailIfChildComplete(Status childStatus)
        {
            if (childStatus is Status.Success or Status.Failure)
            {
                return Status.Failure;
            }
            return Status.Waiting;
        }
}

