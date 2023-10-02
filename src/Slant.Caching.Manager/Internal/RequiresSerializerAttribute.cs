using System;
using System.Linq;

namespace Slant.Caching.Manager.Internal;

/// <summary>
/// Can be used to decorate cache handles which require serialization
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequiresSerializerAttribute : Attribute
{
}