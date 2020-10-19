using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PondSharp.Client
{
    public class CSharpClassCompiler
    {
        private readonly Uri _baseUri;
        private List<MetadataReference> _references;
        private static readonly Dictionary<string, MetadataReference> CachedReferences = new Dictionary<string, MetadataReference>();
        private Assembly _assembly;

        public bool HasAssembly => _assembly != null;
        
        public static async Task<CSharpClassCompiler> Make(IEnumerable<Type> referenceTypes, Uri baseUri)
        {
            var compiler = new CSharpClassCompiler(baseUri);
            await compiler.SetReferences(referenceTypes);
            return compiler;
        }

        private CSharpClassCompiler(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public void Compile(IEnumerable<(string, string)> sourceTexts)
        {
            var assemblyName = Path.GetRandomFileName();
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8);
            
            var trees = sourceTexts.Select(st =>
                CSharpSyntaxTree.ParseText(st.Item2, options, st.Item1));
            var compilation = CSharpCompilation.Create(
                assemblyName,
                trees,
                _references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            var errors = result.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(GetDiagnosticError)
                .ToArray();
            if (errors.Length > 0)
                throw new CompileException(errors);

            ms.Seek(0, SeekOrigin.Begin);
            _assembly = Assembly.Load(ms.ToArray());
        }

        private string GetDiagnosticError(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetMappedLineSpan();
            return
                lineSpan.Path + "[Line " +
                (lineSpan.StartLinePosition.Line + 1) + " Pos " +
                (lineSpan.StartLinePosition.Character + 1) + "]: " +
                diagnostic.GetMessage();
            
            return $"{diagnostic.Location.GetMappedLineSpan().Path}: {diagnostic.GetMessage()}";
        }

        public IEnumerable<string> AvailableInstances(Type targetType)
        {
            if (_assembly is null) 
                throw new InvalidOperationException("No compiled assembly present");

            return _assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => t.IsSubclassOf(targetType))
                .Where(t => !t.IsAbstract && t.IsPublic)
                .Select(t => t.FullName);
        }

        public T New<T>(string instanceName, params object[] args) where T : class
        {
            if (_assembly is null) 
                throw new InvalidOperationException("No compiled assembly present");
            if (!(_assembly.CreateInstance(instanceName, false, BindingFlags.Default, null, args, null, null) is T instance))
                throw new ApplicationException($"{instanceName} failed to initialize");
            return instance;
        }

        private async Task SetReferences(IEnumerable<Type> referenceTypes)
        {
            using var client = new HttpClient
            {
                BaseAddress = _baseUri
            };

            var neededAssemblyLocations = referenceTypes
                .Select(type => type.Assembly.Location)
                .Distinct()
#if DEBUG
                .Concat(new [] { "netstandard.dll" })
#endif
                .ToArray();

            var missingAssemblies = neededAssemblyLocations
                .Where(location => !CachedReferences.Keys.Contains(location));
            
            foreach (var missingAssemblyLocation in missingAssemblies)
            {
                var stream = await client.GetStreamAsync($"_framework/_bin/{missingAssemblyLocation}");
                CachedReferences.Add(missingAssemblyLocation, MetadataReference.CreateFromStream(stream));
            }

            _references = neededAssemblyLocations.Select(l => CachedReferences[l]).ToList();
        }
    }

    public class CompileException : Exception
    {
        public IList<string> Errors { get; private set; }

        public CompileException(IList<string> errors) : base("Compilation Exception")
        {
            Errors = errors;
        }
    }
}