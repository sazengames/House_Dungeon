using UnityEngine;

namespace GameCreator.Runtime.Common
{
    public interface ISpatialHash
    {
        sealed Vector3 Position => ((Component) this).transform.position;

        sealed EntityId UniqueCode => ((Component) this).transform.GetEntityId();
    }
}