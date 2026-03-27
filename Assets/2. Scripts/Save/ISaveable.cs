public interface ISaveable<T>
{
    T GetSaveData(); // 저장할 데이터 생성하기 위함
    void LoadFromSaveData(T data); // 저장되어있던 데이터 복원을 위함
}
