using System;
using System.Collections;
using System.Collections.Generic;
using Nrjwolf.Tools.AttachAttributes;
using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Metal : MonoBehaviour
{
    [GetComponent] public Rigidbody2D rb;
    [Required] public Collider2D col;

    public bool overrideMass;
    [ShowIf(nameof(overrideMass))]public float mass;

    public float GetMass() => overrideMass ? mass : rb.mass;
    public Vector3 GetWorldCenterMass() => rb.worldCenterOfMass;

    private static readonly RaycastHit2D[] Results = new RaycastHit2D[64];

    private PlayerCharacter character;
    
    public Vector3 ApplyForce(PlayerCharacter character, Vector3 force)
    {
        this.character = character;
        
        rb.AddForce(force);

        var dir = rb.velocity.normalized;

        var count = rb.Cast(rb.velocity * Time.fixedDeltaTime, Results);

        if (count > 0)
            return -dir * force;
        return Vector3.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!character) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            var pushBack = collision.contacts[i].relativeVelocity * Time.fixedDeltaTime;
            character.Rb.AddForce(pushBack * character.PushIntensity,ForceMode2D.Impulse);
            Debug.Log($"PushBack:{pushBack}");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(!character) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            character.Rb.AddForce(collision.contacts[i].normal * character.PushIntensity, ForceMode2D.Force);
        }
    }

    private void FixedUpdate()
    {
        character = null;
    }
}
