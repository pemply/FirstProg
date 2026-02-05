using Unity.VisualScripting;
using UnityEngine;

namespace CodeBase.CameraLogic
{
    public class CameraFollow : MonoBehaviour
    {
        public float RotationAngleX = 40f;
        public float DistanceOffset = 15f;
        public float OffsetY = 0.6f;

         private Transform _following;
        private void LateUpdate()
        {
            if (_following == null)
                return;
            
            Quaternion rotation = Quaternion.Euler(RotationAngleX, 0, 0);
            Vector3 position = rotation * new Vector3(0, 0, -DistanceOffset) + FollowingPointPosition();
            
            transform.rotation  = rotation;
            transform.position  = position;
          

        }

        public void Follow(GameObject followingObject) => 
            _following = followingObject.transform;

        private Vector3 FollowingPointPosition()
        {
            Vector3 followingPosition =  _following.position;
            followingPosition.y += OffsetY;
            
            return followingPosition;
        }
    }
}