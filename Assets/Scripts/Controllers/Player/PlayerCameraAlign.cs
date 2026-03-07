using UnityEngine;

namespace Controllers.Player
{
    public class PlayerCameraAlign : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float rotateSpeed = 720f;

        void Update()
        {
            if (Input.GetMouseButton(1))
            {
                AlignPlayerToCamera();
            }
        }

        private void AlignPlayerToCamera()
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0;

            if (forward.sqrMagnitude < 0.01f)
                return;

            Quaternion target = Quaternion.LookRotation(forward);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}