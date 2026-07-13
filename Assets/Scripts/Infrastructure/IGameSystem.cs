namespace Infrastructure
{
    public interface IGameSystem
    {
        void Initialize(); // вызывается при старте игры
        void Dispose();    // вызывается при выгрузке или завершении забега
    }
}