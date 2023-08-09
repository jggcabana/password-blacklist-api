public interface IBloomFilter
{
    void Add(string item);

    bool Contains(string item);

    Task LoadData();
}