namespace EventFlowSharp.Events;

public interface ILinearEvent
{
    CafeEvent? NextEvent { get; set; }
}