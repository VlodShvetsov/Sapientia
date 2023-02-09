﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ArrayExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Expand_WithPool<T>(ref T[] array, int newCapacity)
		{
			var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
			Array.Copy(array, newArray, array.Length);

			ArrayPool<T>.Shared.Return(array);
			array = newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Expand<T>(ref T[] array, int newCapacity)
		{
			var newArray = new T[newCapacity];
			Array.Copy(array, newArray, array.Length);

			array = newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Copy_WithPool<T>(this T[] array)
		{
			var newArray = ArrayPool<T>.Shared.Rent(array.Length);
			Array.Copy(array, newArray, array.Length);

			ArrayPool<T>.Shared.Return(array);
			return newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Copy<T>(this T[] array)
		{
			var newArray = new T[array.Length];
			Array.Copy(array, newArray, array.Length);

			return newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Fill<T>(this T[] array, T defaultValue)
		{
			Array.Fill(array, defaultValue);
		}
	}
}