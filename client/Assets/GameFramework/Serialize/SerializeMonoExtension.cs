// using Newtonsoft.Json;
using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// Unity组件的序列化扩展方法
    /// </summary>
    public static class SerializeMonoExtension
    {
        public static Vector3Serializable ToVector3Serializable(this Vector3 vector3)
        {
            return new Vector3Serializable(vector3);
        }

        public static Vector2Serializable ToVector2Serializable(this Vector2 vector2)
        {
            return new Vector2Serializable(vector2);
        }

        public static QuaternionSerializable ToQuaternionSerializable(this Quaternion quaternion)
        {
            return new QuaternionSerializable(quaternion);
        }

        public static ColorSerializable ToColorSerializable(this Color color)
        {
            return new ColorSerializable(color);
        }

        public static string Serialize(this Transform target)
        {
            // return JsonConvert.SerializeObject(new TransformSerializable(target));
            return "";
        }

        public static void Deserialize(this Transform target, string json)
        {
            // var transformSerializable = JsonConvert.DeserializeObject<TransformSerializable>(json);
            // target.position = transformSerializable.Position.ToVector3();
            // target.rotation = transformSerializable.Rotation.ToQuaternion();
            // target.localPosition = transformSerializable.LocalPosition.ToVector3();
            // target.localRotation = transformSerializable.LocalRotation.ToQuaternion();
            // target.localScale = transformSerializable.LocalScale.ToVector3();
        }

        public static string Serialize(this Rigidbody2D target)
        {
            // return JsonConvert.SerializeObject(new Rigidbody2DSerializable(target));
            return "";
        }

        public static void Deserialize(this Rigidbody2D target, string json)
        {
            // var rigidbody2DSerializable = JsonConvert.DeserializeObject<Rigidbody2DSerializable>(json);
            // target.velocity = rigidbody2DSerializable.Velocity.ToVector2();
            // target.angularDrag = rigidbody2DSerializable.AngularDrag;
            // target.gravityScale = rigidbody2DSerializable.GravityScale;
            // target.isKinematic = rigidbody2DSerializable.IsKinematic;
            // target.mass = rigidbody2DSerializable.Mass;
            // target.constraints = rigidbody2DSerializable.Constraints;
            // target.angularVelocity = rigidbody2DSerializable.AngularVelocity;
            // target.position = rigidbody2DSerializable.Position.ToVector2();
            // target.rotation = rigidbody2DSerializable.Rotation;
        }

        public static string Serialize(this Rigidbody target)
        {
            // return JsonConvert.SerializeObject(new RigidbodySerializable(target));
            return "";
        }

        public static void Deserialize(this Rigidbody target, string json)
        {
            // var rigidbodySerializable = JsonConvert.DeserializeObject<RigidbodySerializable>(json);
            // target.velocity = rigidbodySerializable.Velocity.ToVector3();
            // target.angularDrag = rigidbodySerializable.AngularDrag;
            // target.drag = rigidbodySerializable.Drag;
            // target.freezeRotation = rigidbodySerializable.FreezeRotation;
            // target.isKinematic = rigidbodySerializable.IsKinematic;
            // target.mass = rigidbodySerializable.Mass;
            // target.useGravity = rigidbodySerializable.UseGravity;
            // target.position = rigidbodySerializable.Position.ToVector3();
            // target.rotation = rigidbodySerializable.Rotation.ToQuaternion();
            // target.constraints = rigidbodySerializable.Constraints;
        }

        public static void Deserialize(this Collider target, string json)
        {
            // var colliderSerializable = JsonConvert.DeserializeObject<ColliderSerializable>(json);
            // target.enabled = colliderSerializable.Enabled;
            // target.isTrigger = colliderSerializable.IsTrigger;
        }

        public static string Serialize(this BoxCollider target)
        {
            // return JsonConvert.SerializeObject(new BoxColliderSerializable(target));
            return "";
        }

        public static void Deserialize(this BoxCollider target, string json)
        {
            // var boxColliderSerializable = JsonConvert.DeserializeObject<BoxColliderSerializable>(json);
            // (target as Collider).Deserialize(json);
            // target.center = boxColliderSerializable.Center.ToVector3();
            // target.size = boxColliderSerializable.Size.ToVector3();
        }

        public static string Serialize(this SphereCollider target)
        {
            // return JsonConvert.SerializeObject(new SphereColliderSerializable(target));
            return "";
        }

        public static void Deserialize(this SphereCollider target, string json)
        {
            // var sphereColliderSerializable = JsonConvert.DeserializeObject<SphereColliderSerializable>(json);
            // (target as Collider).Deserialize(json);
            // target.center = sphereColliderSerializable.Center.ToVector3();
            // target.radius = sphereColliderSerializable.Radius;
        }

        public static string Serialize(this CapsuleCollider target)
        {
            // return JsonConvert.SerializeObject(new CapsuleColliderSerializable(target));
            return "";
        }

        public static void Deserialize(this CapsuleCollider target, string json)
        {
            // var capsuleColliderSerializable = JsonConvert.DeserializeObject<CapsuleColliderSerializable>(json);
            // (target as Collider).Deserialize(json);
            // target.center = capsuleColliderSerializable.Center.ToVector3();
            // target.radius = capsuleColliderSerializable.Radius;
            // target.height = capsuleColliderSerializable.Height;
            // target.direction = capsuleColliderSerializable.Direction;
        }

        public static string Serialize(this MeshCollider target)
        {
            // return JsonConvert.SerializeObject(new MeshColliderSerializable(target));
            return "";
        }

        public static void Deserialize(this MeshCollider target, string json)
        {
            // var meshColliderSerializable = JsonConvert.DeserializeObject<MeshColliderSerializable>(json);
            // (target as Collider).Deserialize(json);
            // target.convex = meshColliderSerializable.Convex;
        }

        public static void Deserialize(this Collider2D target, string json)
        {
            // var collider2DSerializable = JsonConvert.DeserializeObject<Collider2DSerializable>(json);
            // target.enabled = collider2DSerializable.Enabled;
            // target.isTrigger = collider2DSerializable.IsTrigger;
            // target.offset = collider2DSerializable.Offset.ToVector2();
            // target.usedByEffector = collider2DSerializable.UsedByEffector;
            // target.usedByComposite = collider2DSerializable.UsedByComposite;
        }

        public static string Serialize(this BoxCollider2D target)
        {
            // return JsonConvert.SerializeObject(new BoxCollider2DSerializable(target));
            return "";
        }

        public static void Deserialize(this BoxCollider2D target, string json)
        {
            // var boxCollider2DSerializable = JsonConvert.DeserializeObject<BoxCollider2DSerializable>(json);
            // (target as Collider2D).Deserialize(json);
            // target.size = boxCollider2DSerializable.Size.ToVector2();
            // target.autoTiling = boxCollider2DSerializable.AutoTiling;
            // target.edgeRadius = boxCollider2DSerializable.EdgeRadius;
        }

        public static string Serialize(this CircleCollider2D target)
        {
            // return JsonConvert.SerializeObject(new CircleCollider2DSerializable(target));
            return "";
        }

        public static void Deserialize(this CircleCollider2D target, string json)
        {
            // var circleCollider2DSerializable = JsonConvert.DeserializeObject<CircleCollider2DSerializable>(json);
            // (target as Collider2D).Deserialize(json);
            // target.radius = circleCollider2DSerializable.Radius;
        }

        public static string Serialize(this CapsuleCollider2D target)
        {
            // return JsonConvert.SerializeObject(new CapsuleCollider2DSerializable(target));
            return "";
        }

        public static void Deserialize(this CapsuleCollider2D target, string json)
        {
            // var capsuleCollider2DSerializable = JsonConvert.DeserializeObject<CapsuleCollider2DSerializable>(json);
            // (target as Collider2D).Deserialize(json);
            // target.size = capsuleCollider2DSerializable.Size.ToVector2();
            // target.direction = capsuleCollider2DSerializable.Direction;
        }

        public static string Serialize(this CompositeCollider2D target)
        {
            // return JsonConvert.SerializeObject(new CompositeCollider2DSerializable(target));
            return "";
        }

        public static void Deserialize(this CompositeCollider2D target, string json)
        {
            // var compositeCollider2DSerializable = JsonConvert.DeserializeObject<CompositeCollider2DSerializable>(json);
            // (target as Collider2D).Deserialize(json);
            // target.geometryType = compositeCollider2DSerializable.GeometryType;
            // target.generationType = compositeCollider2DSerializable.GenerationType;
            // target.vertexDistance = compositeCollider2DSerializable.VertexDistance;
            // target.edgeRadius = compositeCollider2DSerializable.EdgeRadius;
            // target.offsetDistance = compositeCollider2DSerializable.OffsetDistance;
        }

        public static string Serialize(this SpriteRenderer target)
        {
            // return JsonConvert.SerializeObject(new SpriteRendererSerializable(target));
            return "";
        }

        public static void Deserialize(this SpriteRenderer target, string json)
        {
            // var spriteRendererSerializable = JsonConvert.DeserializeObject<SpriteRendererSerializable>(json);
            // target.color = spriteRendererSerializable.Color.ToColor();
            // target.flipX = spriteRendererSerializable.FlipX;
            // target.flipY = spriteRendererSerializable.FlipY;
            // target.drawMode = spriteRendererSerializable.DrawMode;
        }

        public static string Serialize(this Animator target)
        {
            // return JsonConvert.SerializeObject(new AnimatorSerializable(target));
            return "";
        }

        public static void Deserialize(this Animator target, string json)
        {
            // var animatorSerializable = JsonConvert.DeserializeObject<AnimatorSerializable>(json);
            // target.applyRootMotion = animatorSerializable.ApplyRootMotion;
            // target.cullingMode = animatorSerializable.CullingMode;
            // target.updateMode = animatorSerializable.UpdateMode;
        }

        public static string Serialize(this Component target)
        {
            switch (target.GetType().Name)
            {
                case nameof(Transform): return (target as Transform).Serialize();
                case nameof(Rigidbody): return (target as Rigidbody).Serialize();
                case nameof(Rigidbody2D): return (target as Rigidbody2D).Serialize();
                case nameof(BoxCollider): return (target as BoxCollider).Serialize();
                case nameof(SphereCollider): return (target as SphereCollider).Serialize();
                case nameof(CapsuleCollider): return (target as CapsuleCollider).Serialize();
                case nameof(MeshCollider): return (target as MeshCollider).Serialize();
                case nameof(BoxCollider2D): return (target as BoxCollider2D).Serialize();
                case nameof(CircleCollider2D): return (target as CircleCollider2D).Serialize();
                case nameof(CapsuleCollider2D): return (target as CapsuleCollider2D).Serialize();
                case nameof(CompositeCollider2D): return (target as CompositeCollider2D).Serialize();
                case nameof(SpriteRenderer): return (target as SpriteRenderer).Serialize();
                case nameof(Animator): return (target as Animator).Serialize();
                default: return "";
            }
        }

        public static void Deserialize(this Component target, string json)
        {
            switch (target.GetType().Name)
            {
                case nameof(Transform): (target as Transform).Deserialize(json); break;
                case nameof(Rigidbody): (target as Rigidbody).Deserialize(json); break;
                case nameof(Rigidbody2D): (target as Rigidbody2D).Deserialize(json); break;
                case nameof(BoxCollider): (target as BoxCollider).Deserialize(json); break;
                case nameof(SphereCollider): (target as SphereCollider).Deserialize(json); break;
                case nameof(CapsuleCollider): (target as CapsuleCollider).Deserialize(json); break;
                case nameof(MeshCollider): (target as MeshCollider).Deserialize(json); break;
                case nameof(BoxCollider2D): (target as BoxCollider2D).Deserialize(json); break;
                case nameof(CircleCollider2D): (target as CircleCollider2D).Deserialize(json); break;
                case nameof(CapsuleCollider2D): (target as CapsuleCollider2D).Deserialize(json); break;
                case nameof(CompositeCollider2D): (target as CompositeCollider2D).Deserialize(json); break;
                case nameof(SpriteRenderer): (target as SpriteRenderer).Deserialize(json); break;
                case nameof(Animator): (target as Animator).Deserialize(json); break;
                default: return;
            }
        }
    }
}