#if FRAMEWORK_SERIALIZE

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

    [Serializable]
    public struct ColorSerializable
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public ColorSerializable(Color color)
        {
            R = color.r;
            G = color.g;
            B = color.b;
            A = color.a;
        }

        public Color ToColor() => new Color(R, G, B, A);
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
        public float Drag { get; set; }
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
            Drag = rigidbody.drag;
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
    [Serializable]
    public class ColliderSerializable
    {
        public bool Enabled { get; set; }
        public bool IsTrigger { get; set; }

        public ColliderSerializable(Collider collider)
        {
            Enabled = collider.enabled;
            IsTrigger = collider.isTrigger;
        }

        public ColliderSerializable()
        {
        }
    }

    /// <summary>
    /// BoxCollider组件序列化
    /// </summary>
    [Serializable]
    public class BoxColliderSerializable : ColliderSerializable
    {
        public Vector3Serializable Center { get; set; }
        public Vector3Serializable Size { get; set; }

        public BoxColliderSerializable(BoxCollider boxCollider) : base(boxCollider)
        {
            Center = boxCollider.center.ToVector3Serializable();
            Size = boxCollider.size.ToVector3Serializable();
        }

        public BoxColliderSerializable()
        {
        }
    }

    /// <summary>
    /// SphereCollider组件序列化
    /// </summary>
    [Serializable]
    public class SphereColliderSerializable : ColliderSerializable
    {
        public Vector3Serializable Center { get; set; }
        public float Radius { get; set; }

        public SphereColliderSerializable(SphereCollider sphereCollider) : base(sphereCollider)
        {
            Center = sphereCollider.center.ToVector3Serializable();
            Radius = sphereCollider.radius;
        }

        public SphereColliderSerializable()
        {
        }
    }

    [Serializable]
    public class CapsuleColliderSerializable : ColliderSerializable
    {
        public Vector3Serializable Center { get; set; }
        public int Direction { get; set; }
        public float Height { get; set; }
        public float Radius { get; set; }

        public CapsuleColliderSerializable(CapsuleCollider capsuleCollider) : base(capsuleCollider)
        {
            Center = capsuleCollider.center.ToVector3Serializable();
            Direction = capsuleCollider.direction;
            Height = capsuleCollider.height;
            Radius = capsuleCollider.radius;
        }

        public CapsuleColliderSerializable()
        {
        }
    }

    /// <summary>
    /// MeshCollider组件序列化
    /// </summary>
    [Serializable]
    public class MeshColliderSerializable : ColliderSerializable
    {
        public bool Convex { get; set; }

        public MeshColliderSerializable(MeshCollider meshCollider) : base(meshCollider)
        {
            Convex = meshCollider.convex;
        }

        public MeshColliderSerializable()
        {
        }
    }

    /// <summary>
    /// Collider2D组件序列化
    /// </summary>
    [Serializable]
    public class Collider2DSerializable
    {
        public bool Enabled { get; set; }
        public bool IsTrigger { get; set; }
        public bool UsedByComposite { get; set; }
        public bool UsedByEffector { get; set; }
        public Vector2Serializable Offset { get; set; }

        public Collider2DSerializable(Collider2D collider2D)
        {
            Enabled = collider2D.enabled;
            IsTrigger = collider2D.isTrigger;
            UsedByComposite = collider2D.usedByComposite;
            UsedByEffector = collider2D.usedByEffector;
            Offset = collider2D.offset.ToVector2Serializable();
        }

        public Collider2DSerializable()
        {
        }
    }

    /// <summary>
    /// BoxCollider2D组件序列化
    /// </summary>
    [Serializable]
    public class BoxCollider2DSerializable : Collider2DSerializable
    {
        public bool AutoTiling { get; set; }
        public Vector2Serializable Size { get; set; }
        public float EdgeRadius { get; set; }

        public BoxCollider2DSerializable(BoxCollider2D boxCollider2D) : base(boxCollider2D)
        {
            AutoTiling = boxCollider2D.autoTiling;
            Size = boxCollider2D.size.ToVector2Serializable();
            EdgeRadius = boxCollider2D.edgeRadius;
        }

        public BoxCollider2DSerializable()
        {
        }
    }

    /// <summary>
    /// CircleCollider2D组件序列化
    /// </summary>
    [Serializable]
    public class CircleCollider2DSerializable : Collider2DSerializable
    {
        public float Radius { get; set; }

        public CircleCollider2DSerializable(CircleCollider2D circleCollider2D) : base(circleCollider2D)
        {
            Radius = circleCollider2D.radius;
        }

        public CircleCollider2DSerializable()
        {
        }
    }

    /// <summary>
    /// CapsuleCollider2D组件序列化
    /// </summary>
    [Serializable]
    public class CapsuleCollider2DSerializable : Collider2DSerializable
    {
        public Vector2Serializable Size { get; set; }
        public CapsuleDirection2D Direction { get; set; }

        public CapsuleCollider2DSerializable(CapsuleCollider2D capsuleCollider2D) : base(capsuleCollider2D)
        {
            Size = capsuleCollider2D.size.ToVector2Serializable();
            Direction = capsuleCollider2D.direction;
        }
    }

    /// <summary>
    /// CompositeCollider2D组件序列化
    /// </summary>
    [Serializable]
    public class CompositeCollider2DSerializable : Collider2DSerializable
    {
        public CompositeCollider2D.GeometryType GeometryType { get; set; }
        public CompositeCollider2D.GenerationType GenerationType { get; set; }
        public float VertexDistance { get; set; }
        public float OffsetDistance { get; set; }
        public float EdgeRadius { get; set; }

        public CompositeCollider2DSerializable(CompositeCollider2D compositeCollider2D) : base(compositeCollider2D)
        {
            GeometryType = compositeCollider2D.geometryType;
            GenerationType = compositeCollider2D.generationType;
            VertexDistance = compositeCollider2D.vertexDistance;
            OffsetDistance = compositeCollider2D.offsetDistance;
            EdgeRadius = compositeCollider2D.edgeRadius;
        }

        public CompositeCollider2DSerializable()
        {
        }
    }

    /// <summary>
    /// SpriteRenderer 组件序列化
    /// </summary>
    [Serializable]
    public class SpriteRendererSerializable
    {
        public ColorSerializable Color { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public SpriteDrawMode DrawMode { get; set; }

        public SpriteRendererSerializable(SpriteRenderer spriteRenderer)
        {
            Color = spriteRenderer.color.ToColorSerializable();
            FlipX = spriteRenderer.flipX;
            FlipY = spriteRenderer.flipY;
            DrawMode = spriteRenderer.drawMode;
        }

        public SpriteRendererSerializable()
        {
        }
    }

    [Serializable]
    public class AnimatorSerializable
    {
        public bool ApplyRootMotion { get; set; }
        public AnimatorCullingMode CullingMode { get; set; }
        public AnimatorUpdateMode UpdateMode { get; set; }

        public AnimatorSerializable(Animator animator)
        {
            ApplyRootMotion = animator.applyRootMotion;
            CullingMode = animator.cullingMode;
            UpdateMode = animator.updateMode;
        }

        public AnimatorSerializable()
        {
        }
    }
}

#endif