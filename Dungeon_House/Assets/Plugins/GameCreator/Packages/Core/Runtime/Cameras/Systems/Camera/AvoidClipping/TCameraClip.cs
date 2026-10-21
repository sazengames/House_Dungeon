using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Cameras
{
    [Title("Avoid Clipping")]
    
    [Serializable]
    public abstract class TCameraClip
    {
        public abstract Vector3 Update(TCamera camera, Vector3 point, Transform[] ignore);
        
        public abstract void OnDrawGizmos(TCamera camera);
    }
}