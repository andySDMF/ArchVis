using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MWM.Line
{
    public interface ILine
    {
		string ID { get; }

        void Append(string data);

		void Create (bool draw = false);

        void Draw();

        void Hide();

        void Clear ();

        void Animate(float delay);
    }

	[System.Serializable]
	public class LineGroup
	{
		public string id;
		public List<Line> lines;
	}

	[System.Serializable]
	public class Line
	{
		public string id;
		public LineSettings settings;
		public string material;
        public LineColor color;

		public bool IsActive { get; private set; }

		public string ID { get { return id; } }

		public GameObject Obj { get; set; }

		public void Clear()
		{
			if (Obj != null) 
			{
				MonoBehaviour.Destroy (Obj);
			}
		}
	}

    [System.Serializable]
    public class LineSettings
    {
		public List<LineVector> points;
		public LineDistribution distribution;
    }

    [System.Serializable]
    public class LineVector
    {
        public float x;
        public float y;
        public float z;

        public LineVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 Get()
        {
            return new Vector3(x, y, z);
        }
    }

	[System.Serializable]
	public class LineDistribution
	{
		public float speed = 1.0f;
		public float delay = 0.0f;
		public float width = 1.0f;
	}

    [System.Serializable]
    public class LineColor
    {
        public float r;
        public float g;
        public float b;

        public LineColor(float red, float green, float blue)
        {
            r = red;
            g = green;
            b = blue;
        }
    }
}
