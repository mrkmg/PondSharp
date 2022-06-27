using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Security.Cryptography;
using System.Text;

namespace PondSharp.Client.IDE
{
    public sealed class CSharpClassCompiler
    {
        private readonly Uri _baseUri;
        private readonly IBinaryCache _binaryCache;
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
        
        public static async Task<CSharpClassCompiler> Make(Uri baseUri, IBinaryCache binaryCache)
        {
            var compiler = new CSharpClassCompiler(baseUri, binaryCache);
            await compiler.SetReferences().ConfigureAwait(false);
            return compiler;
        }

        private CSharpClassCompiler(Uri baseUri, IBinaryCache binaryCache)
        {
            _binaryCache = binaryCache;
            _baseUri = baseUri;
        }

        public async Task Compile(Dictionary<string, string> sourceTexts)
        {
            using var hasher = SHA256.Create();
            var combined = sourceTexts.Values
                .Select(Encoding.UTF8.GetBytes)
                .Select(hasher.ComputeHash)
                .Select(Encoding.UTF8.GetString)
                .OrderBy(s => s)
                .Aggregate((a, b) => a + b);
            var hash = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(combined)));

            try
            {
                if (await _binaryCache.HasBinary(hash).ConfigureAwait(false))
                {
                    var bytes = await _binaryCache.GetBinary(hash).ConfigureAwait(false);
                    _assembly = Assembly.Load(bytes);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            
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
                new(OutputKind.DynamicallyLinkedLibrary, concurrentBuild: false) 
            );
            await using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            var errors = result.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(GetDiagnosticError)
                .ToArray();
            if (errors.Length > 0)
                throw new CompileException(errors);

            ms.Seek(0, SeekOrigin.Begin);
            var byteArray = ms.ToArray();
            await _binaryCache.StoreBinary(hash, byteArray).ConfigureAwait(false);
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
            if (_assembly.CreateInstance(instanceName, false, BindingFlags.Default, null, args, null, null) is not T instance)
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
                // ReSharper disable once AccessToDisposedClosure
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