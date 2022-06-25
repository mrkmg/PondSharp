using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace PondSharp.Client.IDE;

internal sealed class LocalStorageBinaryCache : IBinaryCache
{
    private readonly ILocalStorageService _localStorage;

    public LocalStorageBinaryCache(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
        
    public Task<bool> HasBinary(string hash)
    {
        return _localStorage.ContainKeyAsync(GetKeyName(hash)).AsTask();
    }

    public async Task<byte[]> GetBinary(string hash)
    {
        return await _localStorage.GetItemAsync<byte[]>(GetKeyName(hash)).ConfigureAwait(false);
    }

    public Task StoreBinary(string hash, byte[] binaryData)
    {
        return _localStorage.SetItemAsync(GetKeyName(hash), binaryData).AsTask();
    }

    private string GetKeyName(string hash)
        => $"lsbc:binary:{hash}";
}