using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections;
using ApiSample.Models;
using System.Text.Json;

namespace ApiSample.Services
{
    public class DataProcessingService : IDataServices
    {
        private readonly ConcurrentDictionary<string, Entity> _store = new();
        private readonly int _defaultExpiration;
        private readonly int _maxExpiration;

        private readonly string _storageFilePath;
        private readonly object _fileLock = new();

        public DataProcessingService(IOptions<Settings> settings)
        {
            var defaultValues = settings.Value;
            _storageFilePath = defaultValues.StorageFilePath;
            _defaultExpiration = defaultValues.DefaultExpirationInSeconds;
            _maxExpiration = defaultValues.MaxExpirationInSeconds;

            LoadFromFile();
        }

        public void Create(string key, List<object> values, int expirationSeconds)
        {
            if (expirationSeconds > _maxExpiration)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationSeconds), $"Expiration cannot exceed {_maxExpiration} seconds.");
            }

            var entry = new Entity
            {
                Key = key,
                Values = values,
                ExpirationTime = DateTime.UtcNow.AddSeconds(expirationSeconds)
            };

            _store.AddOrUpdate(key, entry, (k, oldEntry) => entry);
            SaveToFile();
        }

        public void Append(string key, List<object> values, int expirationSeconds)
        {
            if (expirationSeconds > _maxExpiration)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationSeconds), $"Expiration cannot exceed {_maxExpiration} seconds.");
            }

            _store.AddOrUpdate(key,
                k => new Entity
                {
                    Key = key,
                    Values = values,
                    ExpirationTime = DateTime.UtcNow.AddSeconds(expirationSeconds <= 0 ? _defaultExpiration : expirationSeconds)
                },
                (k, existingEntry) =>
                {
                    existingEntry.Values.AddRange(values);
                    existingEntry.ExpirationTime = DateTime.UtcNow.AddSeconds(expirationSeconds <= 0 ? _defaultExpiration : expirationSeconds);
                    return existingEntry;
                });
            SaveToFile();
        }

        public bool Delete(string key)
        {
            var removed = _store.TryRemove(key, out _);
            if (removed)
            {
                SaveToFile();
            }
            return removed;
        }

        public bool Get(string key, out List<object> values)
        {
            values = null;
            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTime != null && entry.ExpirationTime < DateTime.UtcNow)
                {
                    _store.TryRemove(key, out _);
                    SaveToFile();
                    return false;
                }
                values = entry.Values;
                ResetExpiration(entry.Key);
                return true;
            }
            return false;
        }

        private void ResetExpiration(string key)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                entry.ExpirationTime = null;
                SaveToFile();
            }
        }

        public void CleanupData()
        {
            var now = DateTime.UtcNow;
            var keysToRemove = _store.Where(pair => pair.Value.ExpirationTime != null && pair.Value.ExpirationTime < now)
                                     .Select(pair => pair.Key)
                                     .ToList();
            bool changed = false;
            foreach (var key in keysToRemove)
            {
                if (_store.TryRemove(key, out _))
                {
                    changed = true;
                }
            }
            if (changed)
            {
                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            try
            {
                lock (_fileLock)
                {
                    var entries = _store.Values.ToList();
                    var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_storageFilePath, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
        private void LoadFromFile()
        {
            if (File.Exists(_storageFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_storageFilePath);
                    var entries = JsonSerializer.Deserialize<List<Entity>>(json);
                    if (entries != null)
                    {
                        foreach (var entry in entries)
                        {
                            _store[entry.Key] = entry;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading persisted data: {ex.Message}");
                }
            }
        }
    }
}
