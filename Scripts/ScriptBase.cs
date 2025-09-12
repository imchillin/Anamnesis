// © Anamnesis.
// Licensed under the MIT license.
namespace Scripts;

using System.Reflection;

public abstract class ScriptBase
{
	public abstract string Name { get; }
	public abstract void Run();

	public static List<ScriptBase> GetAllScripts()
	{
		var list = new List<ScriptBase>();

		Assembly asm = Assembly.GetExecutingAssembly();
		foreach (var type in asm.GetTypes())
		{
			if (type.BaseType == typeof(ScriptBase))
			{
				if (Activator.CreateInstance(type) is not ScriptBase script)
					continue;

				list.Add(script);
			}
		}

		return list;
	}

	protected static void Log(string message)
	{
		Console.WriteLine(message);
	}
}