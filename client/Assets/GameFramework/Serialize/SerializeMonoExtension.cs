using Newtonsoft.Json;
using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// Unity组件的序列化扩展方法
    /// </summary>
    public static class SerializeMonoExtension
    {
        public static Vector3Serializable ConvertToVector3Serializable(this Vector3 vector3)
        {
            return new Vector3Serializable(vector3);
        }

        public static string Serialize(this Transform target)
        {
            return JsonConvert.SerializeObject(new TransformSerializable(target));
            // return "";
        }

        public static void Deserialize(this Transform target, string json)
        {
            var transformSerializable = JsonConvert.DeserializeObject<TransformSerializable>(json);
            target.position = transformSerializable.Position.ToVector3();
            target.rotation = transformSerializable.Rotation.ToQuaternion();
            target.localPosition = transformSerializable.LocalPosition.ToVector3();
            target.localRotation = transformSerializable.LocalRotation.ToQuaternion();
            target.localScale = transformSerializable.LocalScale.ToVector3();
        }

        public static string Serialize(this Rigidbody2D target)
        {
            return JsonConvert.SerializeObject(new Rigidbody2DSerializable(target));
            // return "";
        }

        public static void Deserialize(this Rigidbody2D target, string json)
        {
            var rigidbody2DSerializable = JsonConvert.DeserializeObject<Rigidbody2DSerializable>(json);
            target.velocity = rigidbody2DSerializable.Velocity.ToVector2();
            target.angularDrag = rigidbody2DSerializable.AngularDrag;
            target.gravityScale =  rigidbody2DSerializable.GravityScale;
            target.isKinematic = rigidbody2DSerializable.IsKinematic;
            target.mass = rigidbody2DSerializable.Mass;
            target.constraints = rigidbody2DSerializable.Constraints;
            target.angularVelocity = rigidbody2DSerializable.AngularVelocity;
            target.position = rigidbody2DSerializable.Position.ToVector2();
            target.rotation = rigidbody2DSerializable.Rotation;
        }

        public static string Serialize(this Component target)
        {
            switch (target.GetType().Name)
            {
                case nameof(Transform): return (target as Transform).Serialize();
                case nameof(Rigidbody2D): return (target as Rigidbody2D).Serialize();
                default: return "";
            }
        }

        public static void Deserialize(this Component target, string json)
        {
            switch (target.GetType().Name)
            {
                case nameof(Transform): (target as Transform).Deserialize(json); break;
                case nameof(Rigidbody2D): (target as Rigidbody2D).Deserialize(json); break;
                default: return;
            }
        }
    }
}