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
// TODO freeze the RB while loading and remove 10/10k
            Ray ray = new (viewer.position + Vector3.up * 10000f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 15000f, layerMask))
            {
                rigidBody.MovePosition(
                    new Vector3(rigidBody.position.x, hit.point.y + 1.5f, rigidBody.position.z)
                );
            }
            
            collider.enabled = true;
        }
    }
}