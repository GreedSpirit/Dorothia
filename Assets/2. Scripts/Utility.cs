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


        // 복리 성장 계산
        public static BigInteger Growth(BigInteger baseValue, int level, float rate)
        {
            if (level <= 1)
                return baseValue;

            // 소수점 3자리까지 정밀도를 유지하기 위해 1000을 곱함
            // 예: rate가 0.1(10%)이면 multiplier는 1100이 됨
            BigInteger multiplier = (BigInteger)((1.0f + rate) * 1000);
            BigInteger divisor = 1000;

            BigInteger result = baseValue;

            // 한 번에 거듭제곱(Pow)한 뒤 나누지 않고, 레벨만큼 반복하며 곱하고 나눔
            for (int i = 1; i < level; i++)
            {
                result = (result * multiplier) / divisor;

                // 혹시라도 수치가 너무 작아져서 0이 되는 것을 방지
                if (result < 1)
                {
                    result = 1;
                    break;
                }
            }

            return result;
        }
    }
}