using System;

// ReSharper disable once CheckNamespace
namespace GodotSharpExtras.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ResourceAttribute : Attribute { }
