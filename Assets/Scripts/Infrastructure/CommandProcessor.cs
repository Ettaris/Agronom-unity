using Infrastructure;

public static class CommandProcessor
{
    public static void Execute(ICommand command)
    {
        command?.Execute();
    }
}