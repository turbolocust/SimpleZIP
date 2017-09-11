using Windows.Security.Cryptography.Core;

namespace SimpleZIP_UI.Application.Hashing
{
    public interface IMessageDigestAlgorithm
    {
        /// <summary>
        /// Calculates a hash value using the specified message digest algorithm.
        /// See <see cref="HashAlgorithmNames"/> for the correct string representations.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <param name="algorithmName">The name of the message digest algorithm.</param>
        /// <param name="hashString">Hexadecimal string representation (in upper case) of the hash value.</param>
        /// <returns>The hashed data.</returns>
        byte[] CalculateHashValue(byte[] data, string algorithmName, out string hashString);
    }
}
