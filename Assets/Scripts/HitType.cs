// HitType.cs
public enum HitType
{
    Unknown,    // Para casos onde o tipo de acerto não é especificado
    Headshot,   // Acerto na cabeça
    BodyShot,   // Acerto no corpo (ou qualquer outra parte que não seja cabeça)
    Melee       // Acerto de ataque corpo a corpo
}
