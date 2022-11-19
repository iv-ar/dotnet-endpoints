using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace I2R.Endpoints;

public static class Helpers
{
    public static bool Inherits(this ClassDeclarationSyntax source, string name) {
        return source.BaseList?.Types.Select(baseType => baseType)
            .Any(baseType => baseType.ToString() == name) ?? false;
    }
}