﻿using System;
using System.IO;
using System.Security.Cryptography;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	[Serializable]
	public struct AesParameters
	{
		public string Key { get; set; }
		public string Iv { get; set; }

		public byte[] KeyBytes => Convert.FromBase64String(Key);
		public byte[] IvBytes => Convert.FromBase64String(Iv);

		public AesParameters(Aes aes)
		{
			Key = Convert.ToBase64String(aes.Key);
			Iv = Convert.ToBase64String(aes.IV);
		}

		public static AesParameters Create()
		{
			var aes = Aes.Create();
			aes.GenerateKey();
			aes.GenerateIV();

			return new AesParameters(aes);
		}

		public Aes CreateAes()
		{
			var aes = Aes.Create();
			aes.Key = KeyBytes;
			aes.IV = IvBytes;

			return aes;
		}
	}

	public static class AesEncryptionExtensions
	{
		public static byte[] EncryptData(this AesParameters parameters, byte[] data)
		{
			using var aes = parameters.CreateAes();
			return EncryptData(aes, data);
		}

		public static byte[] EncryptData(this Aes aes, byte[] data)
		{
			var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			var encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

			return encryptedBytes;
		}

		public static string AesDecryptString(this Stream encryptedBytes, AesParameters parameters)
		{
			return AesDecryptString(encryptedBytes, parameters.KeyBytes, parameters.IvBytes);
		}

		public static string AesDecryptString(this Stream encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var sr = new StreamReader(csDecrypt);

			return sr.ReadToEnd();
		}

		public static string AesDecryptString(this byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var sr = new StreamReader(csDecrypt);

			return sr.ReadToEnd();
		}

		public static byte[] AesDecryptData(this Stream encryptedBytes, AesParameters parameters)
		{
			return AesDecryptData(encryptedBytes, parameters.KeyBytes, parameters.IvBytes);
		}

		public static byte[] AesDecryptData(this Stream encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var ms = new MemoryStream();

			csDecrypt.CopyTo(ms);
			return ms.ToArray();
		}

		public static byte[] AesDecryptData(this byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			using var csDecrypt = GetCryptoStream(encryptedBytes, key, iv);
			using var ms = new MemoryStream();

			csDecrypt.CopyTo(ms);
			return ms.ToArray();
		}

		private static CryptoStream GetCryptoStream(Stream encryptedBytes, byte[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var msDecrypt = new MemoryStream();
			encryptedBytes.CopyTo(msDecrypt);
			return new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
		}

		private static CryptoStream GetCryptoStream(byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var msDecrypt = new MemoryStream(encryptedBytes);
			return new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
		}

		public static void AesDecryptLines(this SimpleList<string> encryptedStrings, AesParameters parameters)
		{
			var aes = parameters.CreateAes();

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			for (var i = 0; i < encryptedStrings.Count; i++)
			{
				var data = Convert.FromBase64String(encryptedStrings[i]);

				using var msDecrypt = new MemoryStream(data);
				using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
				using var srDecrypt = new StreamReader(csDecrypt);

				encryptedStrings[i] = srDecrypt.ReadToEnd();
			}
		}
	}
}