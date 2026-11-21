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

    public override void Activar(PlayerScript nau, GameManager gm) { }
}
