﻿
using Newtonsoft.Json.Linq;
using System.Collections;
using TMPro;
using UdonSharp;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using VRC.SDK3.ClientSim;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Serialization.OdinSerializer;

public class DoubleJump : UdonSharpBehaviour
{
    private readonly float averageFrameTime = (float)1 / 60;
    private readonly float ymovement = 6.0f;
    private readonly float movementlimit = 3.0f;
    private readonly float targetScaling = 0.05f;
    private float maths;

    private VRCPlayerApi    player;
    private bool            spaceIsHeld;

    float                   playerMoveHorizontal = 0.0f;
    float                   playerMoveVertical = 0.0f;
    float[]                 defaultMovementSettings = new float[4];

    public TextMeshProUGUI  rotationText;
    Vector3 currentRotation;

    void Start()
    {
        maths = targetScaling * averageFrameTime;
        player = Networking.LocalPlayer;
        player.SetGravityStrength(1);

        defaultMovementSettings[0] = 3; //for some reason getting the default is not working here
        defaultMovementSettings[1] = 3;
        defaultMovementSettings[2] = 4;
        defaultMovementSettings[3] = 3;

        player.SetJumpImpulse(defaultMovementSettings[0]);
        player.SetWalkSpeed(defaultMovementSettings[1]);
        player.SetRunSpeed(defaultMovementSettings[2]);
        player.SetStrafeSpeed(defaultMovementSettings[3]);
    }

    
    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        spaceIsHeld = value;
    }

    public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
    {
        playerMoveHorizontal = value;
    }

    public override void InputMoveVertical(float value, UdonInputEventArgs args)
    {
        playerMoveVertical = value;
    }

    public void SetMovementParams(bool state)
    {
        if (state) //true means player is grounded
        {
            player.SetJumpImpulse(defaultMovementSettings[0]);
            player.SetWalkSpeed(defaultMovementSettings[1]);
            player.SetRunSpeed(defaultMovementSettings[2]);
            player.SetStrafeSpeed(defaultMovementSettings[3]);
            player.SetGravityStrength(1);
        }
        else
        {
            player.SetJumpImpulse(0);
            player.SetWalkSpeed(0);
            player.SetRunSpeed(0);
            player.SetStrafeSpeed(0);
            player.SetGravityStrength(0);
        }
    }


    private void Update()
    {
        currentRotation = player.GetRotation().eulerAngles;
        float currentRotationRadians = currentRotation[1] * Mathf.PI / 180;
        float sinOfRotation = Mathf.Sin(currentRotationRadians);
        float cosinOfRotation = Mathf.Cos(currentRotationRadians);

        Vector3 playerMovementVector = player.GetVelocity();

        SetMovementParams(player.IsPlayerGrounded());

        //Debug.Log("targetScaling = " + targetScaling);
        //Debug.Log("averageFrameTime = " + averageFrameTime);
        //Debug.Log("maths = "+ maths);
        //Debug.Log(newTargetScaling);
        //Debug.Log(1 - newTargetScaling);

        float newTargetScaling = Time.deltaTime / averageFrameTime;

        //Debug.Log((float)0.1 / newTargetScaling);

        //rotationText.text = Mathf.Pow(0.95f, newTargetScaling).ToString();
        rotationText.text = playerMovementVector[0].ToString();

        if (!player.IsPlayerGrounded())
        {
            if (playerMoveHorizontal == 0 && playerMoveVertical == 0)
            {
                playerMovementVector[0] = playerMovementVector[0] * Mathf.Pow(0.95f, newTargetScaling);
                playerMovementVector[2] = playerMovementVector[2] * Mathf.Pow(0.95f, newTargetScaling);
            }
            else
            {
                float xlimit = (movementlimit * cosinOfRotation * playerMoveHorizontal) + (movementlimit * sinOfRotation * playerMoveVertical);
                float ylimit = (movementlimit * sinOfRotation * -1 * playerMoveHorizontal) + (movementlimit * cosinOfRotation * playerMoveVertical);

                if (!(playerMovementVector[0] == xlimit))
                {
                    float difference = xlimit - playerMovementVector[0];
                    playerMovementVector[0] += (difference * ((float)0.1 * newTargetScaling));
                    rotationText.text = difference.ToString("0.0000") + "\n";
                    rotationText.text += ((float)0.1 * newTargetScaling).ToString("0.0000") + "\n";
                    rotationText.text += "\n" + "\n";
                }
                if (!(playerMovementVector[2] == ylimit))
                {
                    float difference = ylimit - playerMovementVector[2];
                    playerMovementVector[2] += (difference * ((float)0.1 * newTargetScaling));
                    rotationText.text += difference.ToString("0.0000") + "\n";
                    rotationText.text += ((float)0.1 * newTargetScaling).ToString("0.0000") + "\n";
                }
             
            }

            if (spaceIsHeld)
            {
                playerMovementVector[1] += ymovement * Time.deltaTime;
            }
            else
            {
                playerMovementVector[1] -= ymovement * Time.deltaTime;
            }

            if (playerMovementVector[1] > 10.0f)
            {
                playerMovementVector[1] = 10.0f;
            }
            player.SetVelocity(playerMovementVector);
        }
    }
}
