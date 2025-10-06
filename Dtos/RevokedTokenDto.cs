using System;

namespace triage_backend.Dtos
{
    public class RevokedTokenDto
    {
        public int Id { get; set; }               // PK
        public string Jti { get; set; } = null!;  // Identificador único del token (claim "jti")
        public DateTime RevokedAt { get; set; }   // Cuándo se revocó
        public DateTime? ExpiresAt { get; set; }  // Fecha de expiración del token original (opcional)
    }
}
