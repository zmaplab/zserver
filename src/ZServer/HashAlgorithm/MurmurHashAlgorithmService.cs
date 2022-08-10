using Murmur;

namespace ZServer.HashAlgorithm
{
    public class MurmurHashAlgorithmService : HashAlgorithmService
    {
        private readonly System.Security.Cryptography.HashAlgorithm _hashAlgorithm;

        public MurmurHashAlgorithmService()
        {
            _hashAlgorithm = MurmurHash.Create32();
        }

        protected override System.Security.Cryptography.HashAlgorithm GetHashAlgorithm()
        {
            return _hashAlgorithm;
        }
    }
}