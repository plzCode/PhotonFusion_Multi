﻿using PixelCrushers.DialogueSystem;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Art_Equilibrium
{
    public class AE_Door : NetworkBehaviour
    {
        [Networked]
        private NetworkBool open { get; set; } = false;
        bool trig;
        public float smooth = 2.0f;
        public float DoorOpenAngle = 87.0f;
        private Quaternion defaultRot;
        private Quaternion openRot;
        private Vector3 defaultLocalPos;
        private Vector3 targetLocalSlidePos;

        private bool isKeyPressed;

        [Header("Door Type")]
        public bool isSlidingDoor = false;                  // Тумблер: обычная или раздвижная
        public Vector3 slideOffset = new Vector3(1, 0, 0);  // Направление сдвига для раздвижной двери (в локальных координатах)

        [Header("GUI Settings")]
        public string openMessage = "Open E";
        public string closeMessage = "Close E";
        public Font messageFont;
        public int fontSize = 24;
        public Color fontColor = Color.white;
        public Vector2 messagePosition = new Vector2(0.5f, 0.5f);

        private string doorMessage = "";

        [Header("Audio Settings")]
        public AudioClip openSound;
        public AudioClip closeSound;
        private AudioSource audioSource;

        [SerializeField]
        private NavMeshObstacle doorObstacle;

        private void Start()
        {
            defaultRot = transform.rotation;
            openRot = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + DoorOpenAngle, transform.eulerAngles.z);
            defaultLocalPos = transform.localPosition;
            targetLocalSlidePos = defaultLocalPos + slideOffset;
            isKeyPressed = false;

            audioSource = gameObject.AddComponent<AudioSource>();
            doorObstacle = GetComponent<NavMeshObstacle>();
        }

        public void Update()
        {
            if(!Runner || !Object || !Object.IsValid) return;
            if (isSlidingDoor)
            {
                Vector3 targetPos = open ? targetLocalSlidePos : defaultLocalPos;
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smooth);
            }
            else
            {
                Quaternion targetRot = open ? openRot : defaultRot;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smooth);
            }

            //Debug.Log(Object.HasInputAuthority ? "Has Input Authority" : "Does not have Input Authority");
            if (Input.GetKeyDown(KeyCode.E) && trig && !isKeyPressed)
            {
                Debug.Log("E key pressed, toggling door state.");
                RPC_ToggleDoor();
                isKeyPressed = true;
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                isKeyPressed = false;
            }

            doorMessage = trig ? (open ? closeMessage : openMessage) : "";
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_ToggleDoor()
        {
            open = !open;
            doorObstacle.carving = !open;
            PlayDoorSound();
        }

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(doorMessage))
            {
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    normal = { textColor = fontColor }
                };

                if (messageFont != null)
                {
                    style.font = messageFont;
                }

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                Vector2 labelSize = style.CalcSize(new GUIContent(doorMessage));
                float labelX = screenWidth * messagePosition.x - labelSize.x / 2;
                float labelY = screenHeight * messagePosition.y - labelSize.y / 2;

                GUI.Label(new Rect(labelX, labelY, labelSize.x, labelSize.y), doorMessage, style);
            }
        }

        private void OnTriggerEnter(Collider coll)
        {
            if (coll.CompareTag("Player"))
            {
                //doorMessage = open ? closeMessage : openMessage;
                trig = true;
            }
        }

        private void OnTriggerExit(Collider coll)
        {
            if (coll.CompareTag("Player"))
            {
                doorMessage = "";
                trig = false;
            }
        }

        private void PlayDoorSound()
        {
            if (audioSource != null)
            {
                if (open && openSound != null)
                {
                    audioSource.clip = openSound;
                    audioSource.Play();
                }
                else if (!open && closeSound != null)
                {
                    audioSource.clip = closeSound;
                    audioSource.Play();
                }
            }
        }

    }
}
