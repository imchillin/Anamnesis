// © Anamnesis.
// Licensed under the MIT license.

using Scripts;

var scripts = ScriptBase.GetAllScripts();
int selected = -1;

while (selected == -1)
{
	Console.WriteLine("Select Script:");
	for (int i = 0; i < scripts.Count; i++)
	{
		Console.WriteLine($"[{i}] - {scripts[i].Name}");
	}

	Console.Write($"Enter a number: ");
	if (!int.TryParse(Console.ReadLine(), out selected) || selected >= scripts.Count)
	{
		Console.WriteLine($"Invalid input.");
		selected = -1;
	}

	try
	{
		scripts[selected].Run();
	}
	catch (Exception ex)
	{
		Exception? inner = ex;
		while (inner != null)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(inner.Message);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(inner.StackTrace);

			inner = inner.InnerException;
		}
	}
}