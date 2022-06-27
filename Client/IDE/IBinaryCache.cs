using System.Threading.Tasks;

namespace PondSharp.Client.IDE
{
    public interface IBinaryCache
    {
        Task<bool> HasBinary(string hash);
        Task<byte[]> GetBinary(string hash);
        Task StoreBinary(string hash, byte[] binaryData);
    }
}