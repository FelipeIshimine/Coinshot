using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterPhysicsController : MonoBehaviour
{
   [field:SerializeField] public Rigidbody2D Rb { get; private set; }
   [field:SerializeField] public BoxCollider2D BoxCol { get; private set; }

   public Vector2 MoveInput { get; set; }
   public Vector2 LookDir { get; set; }

   [Header("Ground")]
   [SerializeField] private float groundedSpeed = 8;
   
   [Header("Air")]
   [SerializeField] private float airSpeed = 5;
   [SerializeField] private float airDrag = .5f;

   [Header("Raycast")]
   [SerializeField, Range(2,20)] public int horizontalRays = 5;
   [SerializeField, Range(2,20)] public int verticalRays = 5;
   public float skin = .1f;
   public float minRay = .1f;
   public LayerMask GroundLayers;
   public int groundMaxAngle;

   [Header("Info")]
   [SerializeField] public StateInfo State;
   [SerializeField] public StateInfo LastState;
   
   [ShowInInspector,ReadOnly] private int _forwardDir;

   private bool _jumping;
   
   private void OnEnable()
   {
      CreateInfo();
   }

   private void CreateInfo()
   {
      State = new StateInfo
      {
         GroundAngles = new float[verticalRays],
         GroundNormals = new Vector2[horizontalRays],
         WallAngles = new float[horizontalRays],
      };
      LastState = new StateInfo
      {
         GroundAngles = new float[verticalRays],
         GroundNormals = new Vector2[horizontalRays],
         WallAngles = new float[horizontalRays]
      };
   }

   private void OnValidate()
   {
      CreateInfo();
   }

   public void FixedUpdate()
   {
      LastState = State;
      State.Clear();
      Raycast();

      HorizontalMovement(State.IsGrounded ? groundedSpeed : airSpeed);
      if(!State.IsGrounded) ApplyAirDrag();
   }

   private void ApplyAirDrag()
   {
      if(airDrag >= 0)
         return;
      // Calculate the drag force
      Vector2 dragForce = -Rb.velocity * airDrag;

      // Apply the drag force to the character
      Rb.AddForce(dragForce);
   }

   private void HorizontalMovement(float speed)
   {
      if (MoveInput.x > 0)
      {
         var vel = Rb.velocity;
         vel.x = Mathf.Max(vel.x, speed * MoveInput.x);
         Rb.velocity = vel;
      }
      else if (MoveInput.x < 0)
      {
         var vel = Rb.velocity;
         vel.x = Mathf.Min(vel.x, speed * MoveInput.x);
         Rb.velocity = vel;
      }
   }

   private void DescentSlope()
   {
      if (LastState.IsGrounded && State.GroundCollider)
      {
         var velChange = Vector2.right * Rb.velocity.x;
         velChange = RotateVector2(velChange, State.GroundAngles[0]);
         Rb.velocity = Rb.velocity - Vector2.right * Rb.velocity.x + velChange;
      }
   }

   private void Raycast()
   {
      //Rays Down
      Vector2 startPosition = (Vector2)BoxCol.transform.position + BoxCol.offset - BoxCol.size/2 + Vector2.up * skin;
      
      float step = (1f / (verticalRays-1)) * BoxCol.size.x;

      var delta = Rb.velocity * Time.fixedDeltaTime;

      var rayLength = Mathf.Max(-delta.y, 0, minRay) + skin;

      float closest = float.MaxValue;
      for (int i = 0; i < verticalRays; i++)
      {
         var rayOrigin = startPosition + Vector2.right * (step * i);
         var hit = Physics2D.Raycast(rayOrigin, Vector2.down,  rayLength, GroundLayers);
         
         var angle = Vector3.Angle(hit.normal, Vector3.up);
         if(hit && angle < groundMaxAngle)
         {
            State.Below = true;
            if (IsLayerIncluded(GroundLayers, hit.collider.gameObject.layer))
            {
               State.IsGrounded = true;
               State.GroundAngles[i] = angle;

               if (Rb.velocity.y < 0) _jumping = false;
               
               if (closest > hit.distance)
               {
                  closest = hit.distance;
                  State.GroundCollider = hit.collider;
                  State.GroundAngle = angle;
                  State.GroundNormal = State.GroundNormals[i];
               }
            }
            Debug.DrawLine(rayOrigin, hit.point, Color.green);
         }
         else
            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
      }
      
      //Rays Up
      startPosition = (Vector2)BoxCol.transform.position + BoxCol.offset + new Vector2(-BoxCol.size.x,BoxCol.size.y)/2 + Vector2.down * skin;
      step = (1f / (verticalRays-1)) * BoxCol.size.x;
      rayLength = Mathf.Max(delta.y, 0, minRay) + skin;
      
      for (int i = 0; i < verticalRays; i++)
      {
         var rayOrigin = startPosition + Vector2.right * (step * i);
         var hit = Physics2D.Raycast(rayOrigin, Vector2.down,  rayLength, GroundLayers);
         if(hit)
         {
            rayLength = hit.distance;
            Debug.DrawLine(rayOrigin, hit.point, Color.green);
         }
         else
            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
      }

      _forwardDir = Mathf.CeilToInt(MoveInput.x);

      //Rays Forward
      /*startPosition = (Vector2)BoxCol.transform.position + BoxCol.offset + new Vector2(BoxCol.size.x * _forwardDir, -BoxCol.size.y)/2 + Vector2.right * (skin * -_forwardDir);
      step = (1f / (horizontalRays-1)) * BoxCol.size.x;
      rayLength = Mathf.Max(delta.y, 0, minRay) + skin;
      
      for (int i = 0; i < horizontalRays; i++)
      {
         var rayOrigin = startPosition + Vector2.up * (step * i);
         var hit = Physics2D.Raycast(rayOrigin, Vector2.right * _forwardDir,  rayLength, GroundLayers);
         if(hit)
         {
            Debug.DrawLine(rayOrigin, hit.point, Color.green);
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle < groundMaxAngle && IsLayerIncluded(GroundLayers, hit.collider.gameObject.layer))
            {
               State.IsGrounded = true;
               State.WallAngles[i] = angle;
               
               if (closest > hit.distance)
               {
                  closest = hit.distance;
                  State.GroundCollider = hit.collider;
                  State.GroundAngle = angle;
                  State.WallAngles[i] = angle;
                  State.GroundNormal = hit.normal;
               }
            }
            else
               Debug.DrawRay(rayOrigin, rayLength * _forwardDir * Vector2.right , Color.yellow);
         }
         else
            Debug.DrawRay(rayOrigin, rayLength * _forwardDir * Vector2.right , Color.red);
      }*/
      
   }
   
   private bool IsLayerIncluded(LayerMask layerMask, int layer)
   {
      int layerMaskValue = 1 << layer;
      return (layerMask & layerMaskValue) != 0;
   }

   private Vector2 RotateVector2(Vector2 vector, float angle)
   {
      // Create a quaternion rotation using the specified angle around the Z-axis
      Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

      // Convert the vector to a Vector3 with Z = 0
      Vector3 vector3 = new Vector3(vector.x, vector.y, 0f);

      // Rotate the vector using the quaternion rotation
      Vector3 rotatedVector3 = rotation * vector3;

      // Convert the rotated vector back to Vector2
      Vector2 rotatedVector = new Vector2(rotatedVector3.x, rotatedVector3.y);

      return rotatedVector;
   }
   
   public void Jump(float jumpVelocity)
   {
      Debug.Log($"JUMP:{jumpVelocity}");
      if (State.IsGrounded)
      {
         _jumping = true;
         var vel = new Vector2(Rb.velocity.x, jumpVelocity);
         Rb.velocity = vel;
      }
   }
}

[System.Serializable]
public struct StateInfo
{
   public bool Below;
   public Collider2D GroundCollider;
   public Vector2 GroundNormal;
   public float GroundAngle;
   public bool IsGrounded;
   
   public float[] GroundAngles;
   public Vector2[] GroundNormals;
   public float[] WallAngles;

   public void Clear()
   {
      for (int i = 0; i < GroundAngles.Length; i++)
      {
         GroundAngles[i] = WallAngles[i] = 0;
         GroundNormals[i] = Vector2.zero;
      }

      IsGrounded = false;
      GroundCollider = null;
      Below = false;
   }
}