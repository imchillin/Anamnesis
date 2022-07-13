// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Extensions;

using System;
using System.Windows;

public static class ResourceExtensions
{
	public static T GetResource<T>(this FrameworkElement self, string name)
	{
		if (!self.Resources.Contains(name))
			throw new Exception($"Resource with Key: \"{name}\" not found on element: {self}");

		object resource = self.Resources[name];

		if (resource is not T tResource)
			throw new Exception($"Resource with Key: \"{name}\" is not of type: {typeof(T)}");

		return tResource;
	}
}
