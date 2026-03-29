public interface IFinishable
{
    bool IsFinishable();
    void EnterFinishableState();
    void GetFinished();
}
