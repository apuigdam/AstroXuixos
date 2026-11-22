using UnityEngine;

// =====================================================================
//  Efectes dels xuixos (GDD). Cada classe és FILLA d'EfecteXuixo.
//  Per afegir-ne un de nou:
//   1. Copia una d'aquestes classes i canvia-li el nom i el que fa.
//   2. Afegeix-la a la llista "disponibles" de GestorEfectes.cs.
// =====================================================================

/// <summary>Debuff (8 s): amunt i avall van al revés.</summary>
public class EfecteTeclesInvertides : EfecteXuixo
{
    public override string Nom => "TECLES INVERTIDES";
    public override string Missatge => "TECLES INVERTIDES!";
    public override TipusEfecte Tipus => TipusEfecte.Debuff;
    public override float Durada => 8f;
    public override string Descripcio => "Amunt i avall van al revés. Compte!";

    public override void Activar(PlayerScript nau, GameManager gm) { nau.controlsInvertits = true; }
    public override void Desactivar(PlayerScript nau, GameManager gm) { nau.controlsInvertits = false; }
}

/// <summary>Risc (8 s): la nau va el doble de ràpid, però tots els punts es dupliquen.</summary>
public class EfecteHipervelocitat : EfecteXuixo
{
    public override string Nom => "HIPERVELOCITAT";
    public override string Missatge => "HIPERVELOCITAT! PUNTS x2";
    public override TipusEfecte Tipus => TipusEfecte.Neutre;
    public override float Durada => 8f;
    public override string Descripcio => "La nau va el doble de ràpid, però tots els punts valen x2.";

    public override void Activar(PlayerScript nau, GameManager gm)
    {
        nau.multiplicadorVelocitat = 2f;
        gm.MultiplicadorPunts = 2;
    }

    public override void Desactivar(PlayerScript nau, GameManager gm)
    {
        nau.multiplicadorVelocitat = 1f;
        gm.MultiplicadorPunts = 1;
    }
}

/// <summary>Buff (6 s): ets invencible i destrosses els meteorits en xocar-hi.</summary>
public class EfecteTrencaAsteroides : EfecteXuixo
{
    public override string Nom => "TRENCA-ASTEROIDES";
    public override string Missatge => "TRENCA-ASTEROIDES!";
    public override TipusEfecte Tipus => TipusEfecte.Buff;
    public override float Durada => 6f;
    public override string Descripcio => "Ets invencible i rebentes els meteorits només de tocar-los.";

    public override void Activar(PlayerScript nau, GameManager gm) { if (gm.Xocs != null) gm.Xocs.trencaAsteroides = true; }
    public override void Desactivar(PlayerScript nau, GameManager gm) { if (gm.Xocs != null) gm.Xocs.trencaAsteroides = false; }
}

/// <summary>Buff (5 s): dispara tres bales en lloc d'una.</summary>
public class EfecteDisparTriple : EfecteXuixo
{
    public override string Nom => "DISPAR TRIPLE";
    public override string Missatge => "DISPAR TRIPLE!";
    public override TipusEfecte Tipus => TipusEfecte.Buff;
    public override float Durada => 5f;
    public override string Descripcio => "Dispares tres míssils alhora, en ventall.";

    public override void Activar(PlayerScript nau, GameManager gm) { nau.dispararTriple = true; }
    public override void Desactivar(PlayerScript nau, GameManager gm) { nau.dispararTriple = false; }
}

/// <summary>Buff (per sempre): una vida més.</summary>
public class EfecteVidaExtra : EfecteXuixo
{
    public override string Nom => "VIDA EXTRA";
    public override string Missatge => "VIDA EXTRA! +1";
    public override TipusEfecte Tipus => TipusEfecte.Buff;
    public override float Durada => 0f;   // instantani
    public override string Descripcio => "Recuperes una vida al moment.";

    public override void Activar(PlayerScript nau, GameManager gm) { gm.SumarVida(); }
}

/// <summary>Neutre: diu que t'ha tocat un poder... i no fa res.</summary>
public class EfectePlacebo : EfecteXuixo
{
    static readonly string[] Missatges =
    {
        "SUPERPODER ACTIVAT!",
        "ARA ETS IMPARABLE!",
        "PODER DEL XUIXO DAURAT!",
    };

    public override string Nom => "PLACEBO";
    public override string Missatge => Missatges[Random.Range(0, Missatges.Length)];
    public override TipusEfecte Tipus => TipusEfecte.Buff;   // es mostra com a buff per enganyar
    public override float Durada => 0f;
    public override string Descripcio => "Diu que t'ha tocat un superpoder... però no fa res.";

    public override void Activar(PlayerScript nau, GameManager gm) { }
}

// ---------------------------------------------------------------------
//  Efectes nous (també del GDD)
// ---------------------------------------------------------------------

/// <summary>Debuff (8 s): la nau es gira i dispara cap enrere.</summary>
public class EfecteModeMirall : EfecteXuixo
{
    public override string Nom => "MODE MIRALL";
    public override string Missatge => "MODE MIRALL! DISPARES ENRERE";
    public override TipusEfecte Tipus => TipusEfecte.Debuff;
    public override float Durada => 8f;
    public override string Descripcio => "La nau es gira i dispara cap enrere.";

    public override void Activar(PlayerScript nau, GameManager gm) { Girar(nau, true); }
    public override void Desactivar(PlayerScript nau, GameManager gm) { Girar(nau, false); }

    static void Girar(PlayerScript nau, bool girada)
    {
        if (nau == null) return;
        nau.dispararEnrere = girada;
        foreach (SpriteRenderer sr in nau.GetComponentsInChildren<SpriteRenderer>())
            sr.flipX = girada;
    }
}

/// <summary>Buff (6 s): tot va a la meitat de velocitat, tens més temps per reaccionar.</summary>
public class EfecteCameraLenta : EfecteXuixo
{
    public override string Nom => "CAMERA LENTA";
    public override string Missatge => "CAMERA LENTA!";
    public override TipusEfecte Tipus => TipusEfecte.Buff;
    public override float Durada => 6f;
    public override string Descripcio => "Tot va a la meitat de velocitat: tens més temps per reaccionar.";

    public override void Activar(PlayerScript nau, GameManager gm) { gm.CanviarEscalaTemps(0.5f); }
    public override void Desactivar(PlayerScript nau, GameManager gm) { gm.CanviarEscalaTemps(1f); }
}

/// <summary>Buff (5 s): els meteorits petits donen 200 punts i els grans 50.</summary>
public class EfectePuntsInvertits : EfecteXuixo
{
    public override string Nom => "PUNTS INVERTITS";
    public override string Missatge => "PUNTS INVERTITS! PETITS = 200";
    public override TipusEfecte Tipus => TipusEfecte.Buff;
    public override float Durada => 5f;
    public override string Descripcio => "Els meteorits petits donen 200 punts i els grans només 50.";

    public override void Activar(PlayerScript nau, GameManager gm) { gm.PuntsInvertits = true; }
    public override void Desactivar(PlayerScript nau, GameManager gm) { gm.PuntsInvertits = false; }
}

/// <summary>Debuff (5 s): les bales són de plàstic i no trenquen res.</summary>
public class EfecteBalesDePlastic : EfecteXuixo
{
    public override string Nom => "BALES DE PLASTIC";
    public override string Missatge => "BALES DE PLASTIC!";
    public override TipusEfecte Tipus => TipusEfecte.Debuff;
    public override float Durada => 5f;
    public override string Descripcio => "Els míssils són de plàstic i no trenquen res.";

    public override void Activar(PlayerScript nau, GameManager gm) { nau.balesDePlastic = true; }
    public override void Desactivar(PlayerScript nau, GameManager gm) { nau.balesDePlastic = false; }
}

/// <summary>Debuff (4 s): els meteorits es tornen foscos, gairebé del color del fons.</summary>
public class EfecteMonocromatisme : EfecteXuixo
{
    static readonly Color ColorAmagat = new Color(0.25f, 0.22f, 0.35f, 1f);

    public override string Nom => "MONOCROMATISME";
    public override string Missatge => "ON SON ELS METEORITS?";
    public override TipusEfecte Tipus => TipusEfecte.Debuff;
    public override float Durada => 4f;
    public override string Descripcio => "Els meteorits es tornen foscos i gairebé no es veuen.";

    public override void Activar(PlayerScript nau, GameManager gm) { Pintar(ColorAmagat); }
    public override void Desactivar(PlayerScript nau, GameManager gm) { Pintar(Color.white); }

    static void Pintar(Color color)
    {
        foreach (Meteor m in Object.FindObjectsByType<Meteor>(FindObjectsSortMode.None))
        {
            SpriteRenderer sr = m.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
    }
}

/// <summary>Buff (5 s): una nau petita t'acompanya i dispara sola.</summary>
public class EfecteLlaonaDeGirona : EfecteXuixo
{
    private NauAjudant ajudant;

    public override string Nom => "LA LLAONA DE GIRONA";
    public override string Missatge => "LA LLAONA DE GIRONA T'AJUDA!";
    public override TipusEfecte Tipus => TipusEfecte.Buff;
    public override float Durada => 5f;
    public override string Descripcio => "Una nau petita t'acompanya i dispara sola.";

    public override void Activar(PlayerScript nau, GameManager gm) { ajudant = NauAjudant.Crear(nau); }
    public override void Desactivar(PlayerScript nau, GameManager gm)
    {
        if (ajudant != null) Object.Destroy(ajudant.gameObject);
        ajudant = null;
    }
}

/// <summary>Neutre (5 s): desapareix tot el HUD.</summary>
public class EfecteSenseHUD : EfecteXuixo
{
    public override string Nom => "HUD?";
    public override string Missatge => "ON ES EL HUD?";
    public override TipusEfecte Tipus => TipusEfecte.Neutre;
    public override float Durada => 5f;
    public override string Descripcio => "Desapareix tot el HUD: ni vides, ni punts, ni xuixos.";

    public override void Activar(PlayerScript nau, GameManager gm) { HUDJoc.Amagat = true; }
    public override void Desactivar(PlayerScript nau, GameManager gm) { HUDJoc.Amagat = false; }
}
