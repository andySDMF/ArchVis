﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Grain.Gallery
{
	[System.Serializable]
	public enum GControlDirection { _Vertical, _Horizontal }

	public interface IGallery
	{
		string ID { get; }

		void Append(string data);

		void Publish(int control = -1); 

		void Clear();
	}

	public class GalleryAttributes
	{
		public float PointerPosition { get { return pPosition; } set { pPosition = value; } }
		public float CurrentPosition { get { return cPosition; } set { cPosition = value; } }

		public int Previous { get; set; }
		public int Index { get; set; }
		public int Next { get; set; }

		private float pPosition = 0.0f;
		private float cPosition = 0.0f;

		public string State { get; set; }
	}

	public interface IGalleryElement
	{
		string Reference { get; }

		void Recieve (string data);

		void Clear();
	}

	public class GalleryBaseEntryElement : MonoBehaviour
	{
		[SerializeField]
		protected string reference = "";
	}

	[System.Serializable]
	public class GalleryBounds
	{
		[SerializeField]
		private float top = 0.0f;

		[SerializeField]
		private float bottom = 0.0f;

		[SerializeField]
		private float left = 0.0f;

		[SerializeField]
		private float right = 0.0f;

		[SerializeField]
		private Vector2 delta;

		public float Top { get { return top; } }
		public float Bottom { get { return bottom; } }
		public float Left { get { return left; } }
		public float Right { get { return right; } }
		public Vector2 Delta { get { return delta; } }

		public GalleryBounds(float t, float b, float l, float r, Vector2 d)
		{
			top = t;
			bottom = b;
			left = l;
			right = r;
			delta = d;
		}
	}

	[System.Serializable]
	public class GEntryGroup
	{
		public string id;
		public List<GEntry> entries;
	}

	[System.Serializable]
	public class GEntry
	{
		public string id;
        public List<GEntryElement> elements = new List<GEntryElement>();
	}

	[System.Serializable]
	public class GEntryElement
	{
		public string id;
		public string data;
	}
}

public static class GalleryUtils
{
	public static List<T> GetInterfaces<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
		var mObjs = gObj.GetComponentsInChildren<MonoBehaviour>();

		return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToList();
	}
}
