using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PondSharp.Client.IDE
{
    public sealed class CSharpClassCompiler
    {
        private readonly Uri _baseUri;
        private List<MetadataReference> _references;
        private Assembly _assembly;

        private static readonly string[] Dlls = {
            "mscorlib.dll",
            "netstandard.dll",
            "System.Collections.dll",
            "System.Console.dll",
            "System.Drawing.Primitives.dll",
            "System.Linq.dll",
            // "System.Linq.Expressions.dll",
            "System.Private.CoreLib.dll",
            // "System.ObjectModel.dll",
            "System.Runtime.dll",
            "PondSharp.UserScripts.dll"
        };

        public bool HasAssembly => _assembly != null;
        
        public static async Task<CSharpClassCompiler> Make(Uri baseUri)
        {
            var compiler = new CSharpClassCompiler(baseUri);
            await compiler.SetReferences().ConfigureAwait(false);
            return compiler;
        }

        private CSharpClassCompiler(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public void Compile(Dictionary<string, string> sourceTexts)
        {
            _assembly = null;
            var assemblyName = Path.GetRandomFileName();
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            
            var trees = sourceTexts.Select(st =>
                CSharpSyntaxTree.ParseText(st.Value, options, st.Key));

            var compilation = CSharpCompilation.Create(
                assemblyName,
                trees,
                _references,
                // must use concurrentBuild:false in blazor due to threading limitations
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, concurrentBuild: false) 
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

        private static string GetDiagnosticError(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetMappedLineSpan();
            return
                lineSpan.Path + "[Line " +
                (lineSpan.StartLinePosition.Line + 1) + " Pos " +
                (lineSpan.StartLinePosition.Character + 1) + "]: " +
                diagnostic.GetMessage();
        }

        public IEnumerable<Type> AvailableInstances(Type targetType)
        {
            if (_assembly is null) return new Type[0];

            return _assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => targetType.IsClass && t.IsSubclassOf(targetType) ||
                            targetType.IsInterface && t.GetInterfaces().Contains(targetType))
                .Where(t => !t.IsAbstract && t.IsPublic);
        }

        public T New<T>(string instanceName, params object[] args) where T : class
        {
            if (_assembly is null) 
                throw new InvalidOperationException("No compiled assembly present");
            if (!(_assembly.CreateInstance(instanceName, false, BindingFlags.Default, null, args, null, null) is T instance))
                throw new ApplicationException($"{instanceName} failed to initialize");
            return instance;
        }

        private async Task SetReferences()
        {
            using var client = new HttpClient
            {
                BaseAddress = _baseUri
            };

            var downloads = await Task.WhenAll(
                Dlls.Select(dll => client.GetStreamAsync($"_framework/{dll}"))
            ).ConfigureAwait(false);
            _references = downloads.Select(dl => (MetadataReference) MetadataReference.CreateFromStream(dl)).ToList();
        }
    }

    public sealed class CompileException : Exception
    {
        public IList<string> Errors { get; }

        public CompileException(IList<string> errors) : base("Compilation Exception")
        {
            Errors = errors;
        }
    }
}