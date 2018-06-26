namespace Essentials.Api.Metadata
{
    public interface IMetadataStore
    {
        object this[string key] { get; set; }

        bool Has(string key);

        void Set(string key, object value);

        object Get(string key);

        T GetOrDefault<T>(string key, T defaultValue);

        T Get<T>(string key);

        bool Remove(string key);
    }
}