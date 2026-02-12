// id나 key가 없을 경우 대체할 수 있도록 만든 인터페이스
// 이미 존재한다면 똑같은 내용으로 할당
public interface ITableKey
{
    public int Id { get; }
    public string Key { get; }
}
