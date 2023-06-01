using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [field:SerializeField] public Rigidbody2D Rb { get; private set; }
    [SerializeField] private CharacterPhysicsController controller;
    [SerializeField] private Radar radar;

    public float forceMultiplier = 1;
    
    [SerializeField] private float jumpForce;
    public float RadarRadius => radar.Radius;

    [field:ShowInInspector, Joystick] public Vector2 LookDir { get; private set; }
    [field: ShowInInspector, ReadOnly] public bool IsPushing { get; private set; }
    public float PushIntensity { get; private set; }
    [field: ShowInInspector, ReadOnly] public bool IsPulling { get; private set; }
    public float PullIntensity { get; private set; }

    [SerializeField] private Metal targetMetal;
    
    public void Jump() => controller.Jump(jumpForce);

    public void StartPush()
    {
        IsPushing = true;
    }

    public void StopPush()
    {
        IsPushing = false;
    }

    public void StartPull()
    {
        IsPulling = true;
    }

    public void StopPull()
    {
        IsPulling = false;
    }

    public Vector3 GetCenterOfMass() => Rb.worldCenterOfMass;

    public void SetLookDirection(Vector3 value) => LookDir = value;

    public void SetMoveInput(Vector2 value) => controller.MoveInput = value;

    public void Update()
    {
        if (LookDir.magnitude >= .5f)
        {
            float dot = -1;
            targetMetal = null;
            Vector2 centerOfMass = Rb.worldCenterOfMass;
            foreach (Metal metalSource in radar.metalSources)
            {
                float aux = Vector2.Dot(((Vector2)metalSource.transform.position-centerOfMass).normalized, LookDir);
                if (aux > dot)
                {
                    dot = aux;
                    targetMetal = metalSource;
                }
            }
        }
    }


    public void FixedUpdate()
    {
        if (targetMetal)
        {
            Debug.Log($"TargetMetal:{targetMetal}");
            var dir = ((Vector2)targetMetal.GetWorldCenterMass() - Rb.worldCenterOfMass).normalized;
            if (IsPulling)
            {
                var force = -dir * (forceMultiplier * Rb.mass);
                var responseForce = targetMetal.ApplyForce(this, force);
                
                Debug.Log($"Pull {force} Response:{responseForce}");

                Rb.AddForce(responseForce);
            }
            if (IsPushing)
            {
                var force = dir * (forceMultiplier * Rb.mass);
                var responseForce = targetMetal.ApplyForce(this,force);
                
                Debug.Log($"Push {force} Response:{responseForce}");

                Rb.AddForce(responseForce);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (targetMetal)
        {
            var bounds = targetMetal.GetComponentInChildren<Collider2D>().bounds;
            Gizmos.DrawWireCube(bounds.center,bounds.size);
        }
    }

    public void SetPullIntensity(float value) => PullIntensity = value;

    public void SetPushIntensity(float value) => PushIntensity = value;
}
