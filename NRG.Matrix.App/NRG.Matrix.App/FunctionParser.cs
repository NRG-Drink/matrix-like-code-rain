using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace NRG.Matrix.App;

public static class FunctionParser
{
	public static T? ParseFuncOrNull<T>(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return default;
		}

		try
		{
			return CSharpScript.EvaluateAsync<T>(str).Result;
		}
		catch
		{
			return default;
		}
	}
}
