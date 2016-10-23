namespace Inceptum.AppServer.Configuration
{
    public interface IConfigurationProviderFactory
    {
        IConfigurationProvider Create();

        void Release(IConfigurationProvider provider);
    }
}