public interface IBloomFilter
{
    bool HasData { get; }
    void Add(string item);

    bool Contains(string item);

    Task LoadData();
}