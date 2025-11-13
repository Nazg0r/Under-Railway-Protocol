using System;
using System.Collections.Generic;

public static class ObjectResolver
{
	private static readonly Dictionary<Type, object> _bindings = new();

	public static void Register<T>(T instance) =>
		_bindings.Add(typeof(T), instance);

	public static void Unregister<T>() =>
		_bindings.Remove(typeof(T));

	public static T Resolve<T>()
	{
		var instanceType = typeof(T);
		var isRegister = _bindings.TryGetValue(typeof(T), out var instance);

		if (!isRegister)
			throw new Exception($"Can`t resolve type: {instanceType}");

		return (T)instance;
	}
}
