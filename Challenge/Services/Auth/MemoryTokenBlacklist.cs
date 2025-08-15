using System;
using System.Collections.Concurrent;

namespace Challenge.Services.Auth
{
   public class MemoryTokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

    public void BlacklistToken(string jti, DateTime expiry)
    {
        // Solo agregar a la lista negra si el token no ha expirado
        if (expiry > DateTime.UtcNow)
        {
            _blacklistedTokens.TryAdd(jti, expiry);
        }
    }

    public bool IsTokenBlacklisted(string jti)
    {
        // Si el token está en la lista negra y aún no ha expirado, entonces está blacklisted
        if (_blacklistedTokens.TryGetValue(jti, out var expiry))
        {
            if (expiry >= DateTime.UtcNow)
            {
                return true;
            }
            // Si ha expirado, removerlo de la lista negra
            _blacklistedTokens.TryRemove(jti, out _);
        }
        return false;
    }

    // Método opcional para limpiar tokens expirados
    public void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        foreach (var token in _blacklistedTokens)
        {
            if (token.Value < now)
            {
                _blacklistedTokens.TryRemove(token.Key, out _);
            }
        }
    }
}
}