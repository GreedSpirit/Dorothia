public interface IPlayerState<T>
{
    void Enter(T Parameter); //상태진입
    void Execute(T Parameter); //상태진행중
    void Exit(T Parameter);    //상태아웃
}
