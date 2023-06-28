namespace VBCodeCompliancer.ChainOfResponsability;
public interface IHandler
{
    IHandler SetNext(IHandler iHandler);
    void Handle(HandlingFile handlingFile);
}
