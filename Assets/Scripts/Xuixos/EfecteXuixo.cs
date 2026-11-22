public enum TipusEfecte { Buff, Debuff, Neutre }

/// <summary>
/// CLASSE MARE dels efectes dels xuixos.
/// Cada efecte és una classe filla (mireu EfectesXuixos.cs) que diu:
///  - Nom: el que surt a la llista d'efectes actius del HUD
///  - Missatge: el text gran que surt al mig de la pantalla
///  - Tipus: Buff (verd), Debuff (vermell) o Neutre (groc)
///  - Durada: segons que dura. 0 = s'aplica un cop i ja està (p. ex. Vida extra)
///  - Descripcio: el que explica la XUIXOPEDIA del menú
///  - Activar(): què canvia quan comença
///  - Desactivar(): com ho deixa tot com estava quan s'acaba
/// </summary>
public abstract class EfecteXuixo
{
    public abstract string Nom { get; }
    public abstract string Missatge { get; }
    public abstract TipusEfecte Tipus { get; }
    public abstract float Durada { get; }

    // Text que surt a la XUIXOPEDIA del menú (una o dues frases curtes)
    public virtual string Descripcio => "";

    public abstract void Activar(PlayerScript nau, GameManager gm);

    // "virtual": per defecte no fa res (els efectes instantanis no cal que el tinguin)
    public virtual void Desactivar(PlayerScript nau, GameManager gm) { }
}
