using UnityEngine;

namespace Managers.Player
{
    public class PlayerManager
    {
        public void PlacePlayer(Transform viewer, LayerMask layerMask) 
        {
            Rigidbody rigidBody = viewer.GetComponentInParent<Rigidbody>();
            Collider collider = rigidBody.GetComponent<Collider>();
            collider.enabled = false;

            Ray ray = new (viewer.position + Vector3.up * 100f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
            {
                rigidBody.MovePosition(
                    new Vector3(rigidBody.position.x, hit.point.y + 1.5f, rigidBody.position.z)
                );
            }
            
            collider.enabled = true;
        }
    }
}