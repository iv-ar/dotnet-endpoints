using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace I2R.Endpoints.Generator;

public class EndpointFinder : ISyntaxReceiver
{
    public HashSet<ClassDeclarationSyntax> Endpoints { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
        if (syntaxNode is not ClassDeclarationSyntax endpoint) return;
        if ((endpoint.BaseList?.Types.Any(c => EndpointGenerator.IsSyncEndpoint(c.ToString())) ?? false)
            || (endpoint.BaseList?.Types.Any(c => EndpointGenerator.IsAyncEndpoint(c.ToString())) ?? false)) {
            Endpoints.Add(endpoint);
        }
    }
}
