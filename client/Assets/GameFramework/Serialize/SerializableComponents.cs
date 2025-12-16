using System;
using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// Vector3 序列化
    /// </summary>
    [Serializable]
    public struct Vector3Serializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Vector3Serializable(Vector3 vector3)
        {
            X = vector3.x;
            Y = vector3.y;
            Z = vector3.z;
        }

        public Vector3 ToVector3() => new Vector3(X, Y, Z);

    
    }

    /// <summary>
    /// Vector2 序列化
    /// </summary>
    [Serializable]
    public struct Vector2Serializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vector2Serializable(Vector2 vector2)
        {
            X = vector2.x;
            Y = vector2.y;
        }
    
        public Vector2 ToVector2() => new Vector2(X, Y);
    }

    /// <summary>
    /// Quaternion 序列化
    /// </summary>
    [Serializable]
    public struct QuaternionSerializable
    {
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public QuaternionSerializable(Quaternion quaternion)
        {
            W = quaternion.w;
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
        }
    
        public Quaternion ToQuaternion() => new Quaternion(X, Y, Z, W);
    }

    /// <summary>
    /// Transform组件序列化
    /// </summary>
    [Serializable]
    public class TransformSerializable
    {
        public Vector3Serializable Position { get; set; }
        public Vector3Serializable LocalPosition { get; set; }
        public QuaternionSerializable Rotation { get; set; }
        public QuaternionSerializable LocalRotation { get; set; }
        public Vector3Serializable LocalScale { get; set; }

        public TransformSerializable(Transform transform)
        {
            Position = new Vector3Serializable(transform.position);
            LocalPosition = new Vector3Serializable(transform.localPosition);
            Rotation = new QuaternionSerializable(transform.rotation);
            LocalRotation = new QuaternionSerializable(transform.localRotation);
            LocalScale = new Vector3Serializable(transform.localScale);
        }

        public TransformSerializable()
        {
        
        }
    }

    /// <summary>
    /// Rigidbody组件序列化
    /// </summary>
    [Serializable]
    public class RigidbodySerializable
    {
        public Vector3Serializable Position { get; set; }
        public QuaternionSerializable Rotation { get; set; }
        public Vector3Serializable Velocity { get; set; }
        public float AngularDrag { get; set; }
        public Vector3Serializable AngularVelocity { get; set; }
        public bool UseGravity { get; set; }
        public bool IsKinematic { get; set; }
        public bool FreezeRotation { get; set; }
        public float Mass { get; set; }
        public RigidbodyConstraints Constraints { get; set; }

        public RigidbodySerializable(Rigidbody rigidbody)
        {
            Position = new Vector3Serializable(rigidbody.position);
            Rotation = new QuaternionSerializable(rigidbody.rotation);
            Velocity = new Vector3Serializable(rigidbody.velocity);
            AngularDrag = rigidbody.angularDrag;
            AngularVelocity = new Vector3Serializable(rigidbody.angularVelocity);
            UseGravity = rigidbody.useGravity;
            IsKinematic = rigidbody.isKinematic;
            FreezeRotation = rigidbody.freezeRotation;
            Mass = rigidbody.mass;
            Constraints = rigidbody.constraints;
        }

        public RigidbodySerializable()
        {
        
        }
    }

    /// <summary>
    /// Rigidbody2D组件序列化
    /// </summary>
    [Serializable]
    public class Rigidbody2DSerializable
    {
        public Vector2Serializable Position { get; set; }
        public float Rotation { get; set; }
        public Vector2Serializable Velocity { get; set; }
        public float AngularDrag { get; set; }
        public float AngularVelocity { get; set; }
        public float GravityScale { get; set; }
        public bool IsKinematic { get; set; }
        public bool FreezeRotation { get; set; }
        public float Mass { get; set; }
        public RigidbodyConstraints2D Constraints { get; set; }

        public Rigidbody2DSerializable(Rigidbody2D rigidbody2D)
        {
            Position = new Vector2Serializable(rigidbody2D.position);
            Rotation = rigidbody2D.rotation;
            Velocity = new Vector2Serializable(rigidbody2D.velocity);
            AngularDrag = rigidbody2D.angularDrag;
            AngularVelocity = rigidbody2D.angularVelocity;
            GravityScale = rigidbody2D.gravityScale;
            IsKinematic = rigidbody2D.isKinematic;
            FreezeRotation = rigidbody2D.freezeRotation;
            Mass = rigidbody2D.mass;
            Constraints = rigidbody2D.constraints;
        }

        public Rigidbody2DSerializable()
        {
        
        }
    }

    /// <summary>
    /// Collider组件序列化
    /// </summary>
    public class ColliderSerializable
    {
        public ColliderSerializable(Collider collider)
        {
        }

        public ColliderSerializable()
        {
        }
    }
}