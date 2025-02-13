using UnityEngine;
using UnityEngine.InputSystem;

namespace Assignment3.SampleSolution
{
    public class TeleportNavigation : MonoBehaviour
    {
        public InputActionReference teleportAction;

        public Transform head;
        public Transform hand;

        public LayerMask groundLayerMask;

        public GameObject previewAvatar;
        public GameObject hitpoint;

        public float rayLength = 10.0f;
        private bool rayIsActive = false;

        public LineRenderer lineVisual;
        private float rayActivationThreshhold = 0.01f;
        private float teleportActivationThreshhold = 0.5f;

        // BEGIN SOLUTION
        private bool previewIsActive = false;
        private Vector3 currentHitPoint;
        private Vector3 targetPoint;
        // END SOLUTION

        // Start is called before the first frame update
        void Start()
        {
            lineVisual.positionCount = 2; // line between two vertices
            lineVisual.enabled = false;
            hitpoint.SetActive(false);
            previewAvatar.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            // activate line visual
            float teleportActionValue = teleportAction.action.ReadValue<float>();
            if (teleportActionValue > rayActivationThreshhold && !rayIsActive)
            {
                rayIsActive = true;
                lineVisual.enabled = rayIsActive;
                hitpoint.SetActive(true); // show
            }
            else if (teleportActionValue < rayActivationThreshhold && rayIsActive)
            {
                rayIsActive = false;
                lineVisual.enabled = rayIsActive;
                hitpoint.SetActive(false); // hide
            }

            if (rayIsActive)
            {
                UpdateLineVisual();
            }

            // BEGIN SOLUTION

            if (rayIsActive)
            {
                if (Physics.Raycast(hand.position, hand.forward * rayLength, out RaycastHit hit, 10f, groundLayerMask))
                {
                    currentHitPoint = hit.point;
                    Debug.Log("hit:" + hit.transform.name);
                    ShowHitpoint(currentHitPoint);
                    if (teleportActionValue > teleportActivationThreshhold && !previewIsActive)
                    {
                        previewIsActive = true;
                        SetTeleportTarget(currentHitPoint);

                        // Show Avatar
                        float userHeight = head.transform.position.y - this.transform.position.y;
                        Quaternion avatarOrientation = Quaternion.LookRotation(head.transform.position -
                                                                               new Vector3(currentHitPoint.x,
                                                                                   head.transform.position.y,
                                                                                   currentHitPoint.z));
                        ShowPreview(currentHitPoint + new Vector3(0, userHeight, 0), avatarOrientation);
                    }
                    else if (previewIsActive) // jump action active -> update
                    {
                        // Update Avatar
                        Vector3 previewAvatarPosition = new Vector3(previewAvatar.transform.position.x,
                            targetPoint.y + head.localPosition.y, previewAvatar.transform.position.z);
                        Quaternion avatarOrientation = Quaternion.LookRotation(previewAvatar.transform.position -
                                                                               new Vector3(currentHitPoint.x,
                                                                                   previewAvatar.transform.position.y,
                                                                                   currentHitPoint.z));
                        ShowPreview(previewAvatarPosition, avatarOrientation);
                    }
                    //else
                    //{
                    //    ShowHitpoint(currentHitPoint);
                    //}
                }
                else
                {
                    HideHitpoint();
                    HidePreview();
                }
            }
            else
            {
                HideHitpoint();
                HidePreview();
            }

            // jump action triggered and released -> perform jump
            if (previewIsActive && teleportActionValue < teleportActivationThreshhold)
            {
                PerformTeleport();
                previewIsActive = false;

                HideHitpoint();
                HidePreview();
            }

            // END SOLUTION
        }

        private void UpdateLineVisual()
        {
            // Implement
            // BEGIN SOLUTION
            lineVisual.SetPosition(0, hand.position);
            lineVisual.SetPosition(1, hand.position + hand.forward * rayLength);
            // END SOLUTION
        }

        // BEGIN SOLUTION

        private void PerformTeleport()
        {
            Quaternion goalOrientation = Quaternion.LookRotation(
                new Vector3(currentHitPoint.x, previewAvatar.transform.position.y, currentHitPoint.z) -
                previewAvatar.transform.position);
            Vector3 goalRotY = new Vector3(0f, goalOrientation.eulerAngles.y, 0f);
            Matrix4x4 goalMat = Matrix4x4.TRS(targetPoint, Quaternion.Euler(goalRotY), new Vector3(1, 1, 1));

            Vector3 headYRot = new Vector3(0f, head.transform.localRotation.eulerAngles.y, 0f);
            Vector3 headXZPos = new Vector3(head.transform.localPosition.x, 0f, head.transform.localPosition.z);
            Matrix4x4 headMat = Matrix4x4.TRS(headXZPos, Quaternion.Euler(headYRot), new Vector3(1, 1, 1));

            Matrix4x4 newMat = goalMat * Matrix4x4.Inverse(headMat);

            transform.position = newMat.GetColumn(3);
            transform.rotation = newMat.rotation;
            transform.localScale = newMat.lossyScale;
        }

        private void ShowHitpoint(Vector3 worldPos)
        {
            hitpoint.SetActive(true); // show
            hitpoint.transform.position = worldPos;
        }

        private void HideHitpoint()
        {
            hitpoint.SetActive(false); // hide
        }

        private void ShowPreview(Vector3 worldPos, Quaternion worldRot)
        {
            previewAvatar.SetActive(true); // show
            previewAvatar.transform.position = worldPos;
            previewAvatar.transform.rotation = worldRot;
        }

        private void HidePreview()
        {
            previewAvatar.SetActive(false); // hide
        }

        private void SetTeleportTarget(Vector3 targetPos)
        {
            targetPoint = targetPos;
        }

        // END SOLUTION
    }
}