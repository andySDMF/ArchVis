using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Procedure;

namespace MWM.Procedure
{
	public interface IProcedure
	{
		bool HasStarted { get; }

		bool Done { get; }

		void Begin();

		void Handle(string procedure);

		void Reset();
	}
}

public static class ProcedureUtils
{
	public static Dictionary<string, IProcedure> Interfaces { get; private set; }

	public static string Add(string id, IProcedure i)
	{
		if (Interfaces == null) Interfaces = new Dictionary<string, IProcedure>();

		if (!Interfaces.ContainsKey(id))
		{
			Interfaces.Add(id, i);
		}

		return id;
	}
}
