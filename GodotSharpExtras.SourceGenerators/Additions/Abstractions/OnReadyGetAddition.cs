using GodotSharpExtras.Attributes;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Extensions.CaseExtensions;
using GodotSharpExtras.SourceGenerators.Models;
using GodotSharpExtras.SourceGenerators.Utilities;

namespace GodotSharpExtras.SourceGenerators.Additions.Abstractions;

internal abstract class OnReadyGetAddition : PartialClassAddition
{
    public VariableSymbol Variable { get; }
    public string Name { get; }
    public string PascalName { get; }
    public string UniqueName { get; }
    public string? Path { get; }
    public bool OrNull { get; }

    protected OnReadyGetAddition(MemberAttributeSite memberSite)
        : base(memberSite.AttributeSite.ClassSymbol)
    {
        Variable = memberSite.Variable;
        Name = Variable.Name;
        PascalName = Name.ToPascalCase();
        UniqueName = $"%{PascalName}";

        var attribute = memberSite.AttributeSite.Attribute.MapToType<OnReadyGetAttribute>();

        Path = attribute.Path;
        OrNull = attribute.OrNull;
    }

    protected virtual void WriteMemberNullCheck(SourceStringBuilder g, string exportMemberName)
    {
        g.Line("if (", Variable.Name, " == null)");
        g.BlockBrace(() =>
        {
            g.Line(
                "throw new NullReferenceException($\"",
                "'{((Resource)GetScript()).ResourcePath}' member '",
                Variable.Name,
                "' is unexpectedly null in '{GetPath()}', '{this}'. Ensure '",
                exportMemberName,
                "' is set correctly, or set OrNull = true in the attribute to allow null.\");"
            );
        });
    }
}
