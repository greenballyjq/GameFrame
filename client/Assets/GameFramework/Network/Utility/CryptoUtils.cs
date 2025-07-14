using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameFramework.Utility
{
    /// <summary>
    /// 加密工具类 - 实现DH密钥交换、HMAC和DES加密
    /// </summary>
    public static class CryptoUtils
    {
        /// <summary>
        /// 生成8字节随机数
        /// </summary>
        /// <returns>8字节随机数</returns>
        public static byte[] GenerateRandomBytes(int length = 8)
        {
            byte[] randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        
        /// <summary>
        /// DH密钥交换 - 生成客户端密钥对
        /// </summary>
        /// <param name="clientKey">输出客户端公钥</param>
        /// <param name="clientPrivateKey">输出客户端私钥</param>
        public static void GenerateClientKeyPair(out byte[] clientKey, out byte[] clientPrivateKey)
        {
            // 简化的DH实现，使用8字节密钥
            clientPrivateKey = GenerateRandomBytes(8);
            clientKey = GenerateRandomBytes(8);
            
            // 简单的变换作为公钥生成（实际项目中应使用标准DH算法）
            for (int i = 0; i < clientKey.Length; i++)
            {
                clientKey[i] = (byte)(clientPrivateKey[i] ^ 0x5A);
            }
        }
        
        /// <summary>
        /// DH密钥交换 - 计算共享密钥
        /// </summary>
        /// <param name="clientPrivateKey">客户端私钥</param>
        /// <param name="serverPublicKey">服务器公钥</param>
        /// <returns>共享密钥</returns>
        public static byte[] ComputeSharedSecret(byte[] clientPrivateKey, byte[] serverPublicKey)
        {
            if (clientPrivateKey == null || serverPublicKey == null)
            {
                throw new ArgumentNullException("密钥不能为空");
            }
            
            if (clientPrivateKey.Length != 8 || serverPublicKey.Length != 8)
            {
                throw new ArgumentException("密钥长度必须为8字节");
            }
            
            byte[] sharedSecret = new byte[8];
            
            // 简化的DH共享密钥计算（实际项目中应使用标准DH算法）
            for (int i = 0; i < 8; i++)
            {
                sharedSecret[i] = (byte)(clientPrivateKey[i] ^ serverPublicKey[i] ^ 0xA5);
            }
            
            return sharedSecret;
        }
        
        /// <summary>
        /// 计算HMAC
        /// </summary>
        /// <param name="data">要计算HMAC的数据</param>
        /// <param name="key">密钥</param>
        /// <returns>HMAC值</returns>
        public static byte[] ComputeHMAC(byte[] data, byte[] key)
        {
            if (data == null || key == null)
            {
                throw new ArgumentNullException("数据和密钥不能为空");
            }
            
            using (var hmac = new HMACSHA256(key))
            {
                byte[] hash = hmac.ComputeHash(data);
                
                // 截取前8字节作为HMAC结果
                byte[] result = new byte[8];
                Array.Copy(hash, 0, result, 0, 8);
                return result;
            }
        }
        
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="key">8字节密钥</param>
        /// <returns>加密后的数据</returns>
        public static byte[] DESEncrypt(byte[] data, byte[] key)
        {
            if (data == null || key == null)
            {
                throw new ArgumentNullException("数据和密钥不能为空");
            }
            
            if (key.Length != 8)
            {
                throw new ArgumentException("DES密钥长度必须为8字节");
            }
            
            try
            {
                using (var des = DES.Create())
                {
                    des.Key = key;
                    des.Mode = CipherMode.ECB;
                    des.Padding = PaddingMode.PKCS7;
                    
                    using (var encryptor = des.CreateEncryptor())
                    {
                        return encryptor.TransformFinalBlock(data, 0, data.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CryptoUtils] DES加密失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="encryptedData">加密的数据</param>
        /// <param name="key">8字节密钥</param>
        /// <returns>解密后的数据</returns>
        public static byte[] DESDecrypt(byte[] encryptedData, byte[] key)
        {
            if (encryptedData == null || key == null)
            {
                throw new ArgumentNullException("数据和密钥不能为空");
            }
            
            if (key.Length != 8)
            {
                throw new ArgumentException("DES密钥长度必须为8字节");
            }
            
            try
            {
                using (var des = DES.Create())
                {
                    des.Key = key;
                    des.Mode = CipherMode.ECB;
                    des.Padding = PaddingMode.PKCS7;
                    
                    using (var decryptor = des.CreateDecryptor())
                    {
                        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CryptoUtils] DES解密失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <returns>Base64编码后的字符串</returns>
        public static string ToBase64(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("数据不能为空");
            }
            
            return Convert.ToBase64String(data);
        }
        
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="base64String">Base64编码的字符串</param>
        /// <returns>解码后的数据</returns>
        public static byte[] FromBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                throw new ArgumentException("Base64字符串不能为空");
            }
            
            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CryptoUtils] Base64解码失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 字符串转字节数组
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] StringToBytes(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new byte[0];
            }
            
            return Encoding.UTF8.GetBytes(str);
        }
        
        /// <summary>
        /// 字节数组转字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>字符串</returns>
        public static string BytesToString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }
            
            return Encoding.UTF8.GetString(bytes);
        }
        
        /// <summary>
        /// 字节数组转十六进制字符串（调试用）
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>十六进制字符串</returns>
        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }
            
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
} 