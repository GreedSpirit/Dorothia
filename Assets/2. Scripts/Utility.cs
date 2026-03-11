using System;
using System.Numerics;

namespace GameUtility
{
    public static class BigIntRandom
    {
        private static readonly Random _rng = new Random();

        public static BigInteger Range(BigInteger min, BigInteger max)
        {
            if (min >= max) return min;

            BigInteger offset = max - min;
            byte[] data = offset.ToByteArray();
            BigInteger res;

            do
            {
                _rng.NextBytes(data);
                data[data.Length - 1] &= 0x7F; // 부호 비트를 양수로 고정
                res = new BigInteger(data);
            } while (res >= offset); // 범위 내에 들어올 때까지 반복

            return res + min;
        }
    }
}