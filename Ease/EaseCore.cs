using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Ease;

namespace MWM.Ease
{
	public enum EaseStyle 
	{ 
		_Linear,
		_EaseInBack, 
		_EaseOutBack,
		_EaseInBounce,
		_EaseOutBounce,
		_EaseInQuad,
		_EaseOutQuad
	}

	public interface IEase
	{
		string Name { get; }

        bool State { get; }

		bool IsEasing { get; }

		void EaseIn();

		void EaseOut();

        void Jump(bool state);

        void Toggle(bool state);
	}

	public class EaseParams
	{
		private EASE.EaseFunction easeFunc;

		public EASE.EaseFunction EaseFunc { get; private set; }

		public void Set(EaseStyle style)
		{
			switch (style) 
			{
				case EaseStyle._EaseInBack:

				EaseFunc = new EASE.EaseFunction(EASE.EaseInBack);
				break;

				case EaseStyle._EaseOutBack:

				EaseFunc = new EASE.EaseFunction(EASE.EaseOutBack);
				break;

				case EaseStyle._EaseInQuad:

				EaseFunc = new EASE.EaseFunction(EASE.EaseInQuad);
				break;

				case EaseStyle._EaseOutQuad:

				EaseFunc = new EASE.EaseFunction(EASE.EaseOutQuad);
				break;

				case EaseStyle._EaseInBounce:

				EaseFunc = new EASE.EaseFunction(EASE.EaseInBounce);
				break;

				case EaseStyle._EaseOutBounce:

				EaseFunc = new EASE.EaseFunction(EASE.EaseOutBounce);
				break;

				default :

				EaseFunc = new EASE.EaseFunction(EASE.Linear);
				break;
			}
		}
	}
}

public static class EASE
{
	public delegate float EaseFunction(float start, float end, float percent);

	public static float Linear(float start, float end, float val)
	{
		return Mathf.Lerp (start, end, val);
	}

	public static float EaseInBack(float start, float end, float val)
	{
		end -= start;
		val /= 1.0f;
		float s = 1.78158f;
		return end * (val) * val * ((s + 1) * val - s) + start;
	}

	public static float EaseOutBack(float start, float end, float val)
	{
		float s = 1.78158f;
		end -= start;
		val = (val) - 1;
		return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
	}

	public static float EaseInBounce(float start, float end, float val)
	{
		end -= start;
		float d = 1.0f;
		return end - EaseOutBounce (0, end, d - val) + start;
	}

	public static float EaseOutBounce(float start, float end, float val)
	{
		val /= 1.0f;
		end -= start;

		if (val < (1 / 2.75f))
			return end * (7.5625f * val * val) + start;
		else if (val < (2 / 2.75f)) 
		{
			val -= (1.5f / 2.75f);
			return end * (7.5625f * (val) * val + 0.75f) + start;
		}
		else if (val < (2.5 / 2.75f)) 
		{
			val -= (2.25f / 2.75f);
			return end * (7.5625f * (val) * val + 0.9375f) + start;
		}
		else
		{
			val -= (2.625f / 2.75f);
			return end * (7.5625f * (val) * val + 0.984375f) + start;
		}
	}

	public static float EaseInQuad(float start, float end, float val)
	{
		end -= start;
		return end * val * val + start;
	}

	public static float EaseOutQuad(float start, float end, float val)
	{
		end -= start;
		return -end * val * (val - 2) + start;
	}
}
