using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace I2R.Endpoints;

[Generator]
public class EndpointGenerator : ISourceGenerator
{
    private const string SourceGenereatedComment = "// Generated, probably smart to leave it be";

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new EndpointFinder());
    }

    private Diagnostic CreateDebugDiagnostic(string message) {
        var descriptor = new DiagnosticDescriptor("debug", "Debug", message, "debug", DiagnosticSeverity.Warning, true);
        return Diagnostic.Create(descriptor, null, "");
    }

    public class NeededInfo
    {
        public string BaseEndpointClassName { get; set; }
        public string BaseEnpointNamespaceName { get; set; }
    }

    private NeededInfo ExtractNeededInfo(ClassDeclarationSyntax classDeclarationSyntax) {
        var baseType = classDeclarationSyntax.BaseList?.Types.FirstOrDefault();
        if (baseType == default) {
            return default;
        }

        var fullTypeName = baseType?.Type.ToString();
        var typeNamespace = GetNamespace(baseType);
        var className = Regex.Match(fullTypeName, "(!?<).+?(?=>)").Value.Replace("<", "");
        return new NeededInfo() {
            BaseEndpointClassName = className,
            BaseEnpointNamespaceName = typeNamespace
        };
    }

    public void Execute(GeneratorExecutionContext context) {
        var asyncEndpoints = ((EndpointFinder) context.SyntaxReceiver)?.AsyncEndpoints;
        var syncEndpoints = ((EndpointFinder) context.SyntaxReceiver)?.SyncEndpoints;
        if (asyncEndpoints == null) {
            context.ReportDiagnostic(CreateDebugDiagnostic("no endpoints were found"));
            return;
        }

        foreach (var endpoint in syncEndpoints) {
            var info = ExtractNeededInfo(endpoint);
            context.AddSource(info.BaseEndpointClassName + "s.g.cs", GetSyncSource(info.BaseEndpointClassName, info.BaseEnpointNamespaceName));
        }

        foreach (var endpoint in asyncEndpoints) {
            var info = ExtractNeededInfo(endpoint);
            context.AddSource(info.BaseEndpointClassName + "s.g.cs", GetAsyncSource(info.BaseEndpointClassName, info.BaseEnpointNamespaceName));
        }
    }

    private string GetSyncSource(string className, string namespaceName) {
        return $@"
namespace {namespaceName};
public static partial class SyncEndpoint<T{className}>
{{
    public static class Req<TRequest>
    {{
        public abstract class Res<TResponse> : {className}
        {{
            public abstract TResponse Handle(
                TRequest request
            );
        }}

        public abstract class NoRes : {className}
        {{
            public abstract void Handle(
                TRequest request
            );
        }}
    }}

    public static class NoReq
    {{
        public abstract class Res<TResponse> : {className}
        {{
            public abstract TResponse Handle();
        }}

        public abstract class NoRes : {className}
        {{
            public abstract void Handle();
        }}
    }}
}}
";
    }

    private string GetAsyncSource(string className, string namespaceName) {
        return $@"
namespace {namespaceName};
public static partial class AsyncEndpoint<T>
{{
    public static class Req<TRequest>
    {{
        public abstract class Res<TResponse> : {className}
        {{
            public abstract Task<TResponse> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }}

        public abstract class NoRes : {className}
        {{
            public abstract Task HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }}
    }}

    public static class NoReq
    {{
        public abstract class Res<TResponse> : {className}
        {{
            public abstract Task<TResponse> HandleAsync(
                CancellationToken cancellationToken = default
            );
        }}

        public abstract class NoRes : {className}
        {{
            public abstract Task HandleAsync(
                CancellationToken cancellationToken = default
            );
        }}
    }}
}}
";
    }

    // determine the namespace the class/enum/struct is declared in, if any
    static string GetNamespace(BaseTypeSyntax syntax) {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax) {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent) {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true) {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent) {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }
}


/*
    public static class Req<TRequest> : BaseEndpoint
    {
        public abstract class Res<TResponse> : BaseEndpoint
        {
            public abstract Task<TResponse> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }

        public abstract class NoRes
        {
            public abstract Task HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }
    }

    public static class NoReq
    {
        public abstract class Res<TResponse>
        {
            public abstract Task<TResponse> HandleAsync(
                CancellationToken cancellationToken = default
            );
        }

        public abstract class NoRes
        {
            public abstract Task HandleAsync(
                CancellationToken cancellationToken = default
            );
        }
    }
 */

/*
    public static class Req<TRequest>
    {
        public abstract class Res<TResponse>
        {
            public abstract TResponse Handle(
                TRequest request
            );
        }

        public abstract class NoRes
        {
            public abstract void Handle(
                TRequest request
            );
        }
    }

    public static class NoReq
    {
        public abstract class Res<TResponse>
        {
            public abstract TResponse Handle();
        }

        public abstract class NoRes
        {
            public abstract void Handle();
        }
    }
 */