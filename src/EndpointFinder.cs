using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace I2R.Endpoints;

public class EndpointFinder : ISyntaxReceiver
{
    public HashSet<ClassDeclarationSyntax> AsyncEndpoints { get; } = new();
    public HashSet<ClassDeclarationSyntax> SyncEndpoints { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
        if (syntaxNode is not ClassDeclarationSyntax endpoint) return;
        if (endpoint.BaseList?.Types.Any(c => c.ToString().StartsWith("AsyncEndpoint")) ?? false) {
            AsyncEndpoints.Add(endpoint);
        } else if (endpoint.BaseList?.Types.Any(c => c.ToString().StartsWith("SyncEndpoint")) ?? false) {
            SyncEndpoints.Add(endpoint);
        }
    }
}