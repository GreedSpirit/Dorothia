using System;
using System.Numerics;

//BigInteger을 사용한 경우, 저장할 때 사용하기 위한 전용 Wrapper 클래스
[Serializable]
public class BigIntegerWrapper
{
    public string value;

    public BigIntegerWrapper(BigInteger bigInt)
    {
        value = bigInt.ToString();
    }

    public BigInteger ToBigInteger()
    {
        return BigInteger.Parse(value);
    }
}