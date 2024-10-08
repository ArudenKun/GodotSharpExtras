﻿using H;

namespace GodotSharpExtras.SourceGenerators.Extensions;

internal static class ResourceExtensions
{
    public static string AsGeneratedString(this Resource resource) =>
        resource.AsString().AddAutoGeneratedMessage();
}
