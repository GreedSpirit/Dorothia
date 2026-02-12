public interface ICSVLoad
{
    // CSV의 한 줄(문자열 배열)을 받아서 내 변수에 채워넣는 함수
    void LoadFromCsv(string[] values);
}
