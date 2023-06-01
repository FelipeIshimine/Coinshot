using System;
using System.Collections;
using System.Collections.Generic;
using Nrjwolf.Tools.AttachAttributes;
using UnityEngine;

public class Radar : MonoBehaviour
{
   [SerializeField,GetComponent] private CircleCollider2D col;
   public List<Metal> metalSources = new List<Metal>();

   public float Radius => col.radius;

   private void OnTriggerEnter2D(Collider2D col)
   {
      if (col.TryGetComponent(out Metal metal) ||
          (col.attachedRigidbody && col.attachedRigidbody.TryGetComponent(out metal)))
      {
         metalSources.Add(metal);
      }
   }

   private void OnTriggerExit2D(Collider2D col)
   {
      if (col.TryGetComponent(out Metal metal) ||
          (col.attachedRigidbody && col.attachedRigidbody.TryGetComponent(out metal)))
      {
         metalSources.Remove(metal);
      }
   }

   private void OnDrawGizmosSelected()
   {
      if (metalSources.Count == 0)
         Gizmos.color = Color.green;
      else
         Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, col.radius);
      
      
      Gizmos.color = Color.blue;

      foreach (Metal metalSource in metalSources)
         Gizmos.DrawLine(transform.position, metalSource.GetWorldCenterMass());
   }
}
