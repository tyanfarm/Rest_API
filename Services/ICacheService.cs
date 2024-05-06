namespace Rest_API.Services;

public interface ICacheService {
    // T - Generic Type Parameter
    // string temp = GetData<string>("key1")
    T GetData<T>(string key);

    bool SetData<T>(string key, T value, DateTimeOffset expirationTime);

    object RemoveData(string key);

}