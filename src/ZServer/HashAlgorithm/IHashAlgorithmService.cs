namespace ZServer.HashAlgorithm
{
    public interface IHashAlgorithmService
    {
        byte[] ComputeHash(byte[] bytes);
    }
}