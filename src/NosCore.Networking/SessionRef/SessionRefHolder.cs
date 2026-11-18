//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;

namespace NosCore.Networking.SessionRef;

/// <summary>
/// Manages session references and generates session identifiers that are unique and cannot be guessed.
/// </summary>
public class SessionRefHolder : ConcurrentDictionary<string, RegionTypeMapping>, ISessionRefHolder
{
    private const int IdBits = 30;
    private const int HalfBits = IdBits / 2;
    private const uint HalfMask = (1u << HalfBits) - 1;
    private const int Rounds = 4;

    private readonly byte[] _key = RandomNumberGenerator.GetBytes(32);
    private int _sessionCounter;

    /// <summary>
    /// Generates a new session identifier.
    /// </summary>
    /// <returns>A positive identifier, unique for the next 2^30 calls and not predictable from earlier ones.</returns>
    public int GenerateSessionId()
    {
        // A world entry is checked against this identifier, so it must not be predictable, and the master tells
        // players apart by it, so it must not repeat. A keyed permutation of a counter gives both.
        // Zero means "no session yet" to the decoders.
        var count = (uint)Interlocked.Increment(ref _sessionCounter) & ((1u << IdBits) - 1);
        return (int)Permute(count) + 1;
    }

    private uint Permute(uint value)
    {
        var left = value >> HalfBits;
        var right = value & HalfMask;
        Span<byte> input = stackalloc byte[5];
        Span<byte> hash = stackalloc byte[32];
        for (byte round = 0; round < Rounds; round++)
        {
            input[0] = round;
            BinaryPrimitives.WriteUInt32LittleEndian(input[1..], right);
            HMACSHA256.HashData(_key, input, hash);
            var next = left ^ (BinaryPrimitives.ReadUInt32LittleEndian(hash) & HalfMask);
            left = right;
            right = next;
        }

        return (left << HalfBits) | right;
    }

    /// <summary>
    /// Attempts to remove and return the value with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">The removed value, if found.</param>
    /// <returns>true if the element was removed successfully; otherwise, false.</returns>
    public new bool TryRemove(string key, out RegionTypeMapping? value)
    {
        var result = base.TryRemove(key, out var baseValue);
        value = baseValue;
        return result;
    }
}