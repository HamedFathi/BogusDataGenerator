using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace BogusDataGenerator.Extensions
{
    internal static class RoslynExtensions
    {
        public static Type ToType(this string source, string @namespace, string @class, out List<string> failures, List<string> assembliesLocations = null)
        {
            assembliesLocations = assembliesLocations ?? new List<string>();
            var mscorlib = typeof(object).Assembly.Location;
            var netstandard = Path.Combine(Path.GetDirectoryName(mscorlib), "netstandard.dll");
            // var runtime = Path.Combine(Path.GetDirectoryName(mscorlib), "System.Runtime.dll");
            // var collections = Path.Combine(Path.GetDirectoryName(mscorlib), "System.Collections.dll");


            var allSystems = Directory.EnumerateFiles(Path.GetDirectoryName(mscorlib), "System.*", SearchOption.TopDirectoryOnly).ToList();

            assembliesLocations = assembliesLocations.Concat(new List<string>() { mscorlib, netstandard /*, runtime, collections*/ }).Concat(allSystems).ToList();
            var portableExecutableReferences = new List<PortableExecutableReference>();

            foreach (var location in assembliesLocations.Distinct())
            {
                portableExecutableReferences.Add(MetadataReference.CreateFromFile(location));
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            string assemblyName = Path.GetRandomFileName();
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                 portableExecutableReferences.ToArray(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    var errors = new List<string>();
                    var diagnostics = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    foreach (Diagnostic diagnostic in diagnostics)
                        errors.Add(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                    failures = errors;
                }
                else
                {
                    failures = null;
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(ms.ToArray()));
                    var name = string.IsNullOrEmpty(@namespace) ? @class : $"{@namespace}.{@class}";
                    Type type = assembly.GetType(name);
                    return type;
                }
            }

            return null;
        }
    }
}