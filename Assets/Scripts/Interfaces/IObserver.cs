public interface IObserver
{
    void OnNotify(ObserverMessage message);
}

public enum ObserverMessage
{
    UpdateRow,
    MergeColor
}
