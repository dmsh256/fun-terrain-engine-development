using UnityEngine;

namespace Managers.Player
{
    public class PlayerManager
    {
        public void PlacePlayer(Transform viewer, LayerMask layerMask) 
        {
            CharacterController characterController = viewer.GetComponentInParent<CharacterController>();
            characterController.enabled = false;
            
            Ray ray = new (viewer.position + Vector3.up * 10000f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 15000f, layerMask))
            {
                Transform player = characterController.transform;
                player.position = new Vector3(player.position.x, hit.point.y + 1.5f, player.position.z);
            }
            
            characterController.enabled = true;
        }
    }
}