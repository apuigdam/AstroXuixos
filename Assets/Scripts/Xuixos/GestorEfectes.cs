using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Va a la nau (el GameManager l'hi posa sol).
/// Quan s'agafa un xuixo, tria un efecte a l'atzar i el controla mentre dura.
/// Regles del GDD:
///  - Els efectes es poden acumular i cadascun dura els seus segons.
///  - Un mateix efecte pot sortir com a màxim 2 vegades per partida.
///  - Si et torna a sortir un efecte que ja tens actiu, se li reinicia el temps.
/// </summary>
public class GestorEfectes : MonoBehaviour
{
    public int maxVegadesPerEfecte = 2;

    /// <summary>Un efecte que està actiu ara mateix i els segons que li queden.</summary>
    public class EfecteActiu
    {
        public EfecteXuixo efecte;
        public float tempsRestant;
    }

    // Tots els efectes que poden sortir. Per afegir-ne un, posa'l aquí.
    private readonly List<EfecteXuixo> disponibles = TotsElsEfectes();

    /// <summary>Una llista nova amb tots els efectes (també la fa servir la XUIXOPEDIA del menú).</summary>
    public static List<EfecteXuixo> TotsElsEfectes()
    {
        return new List<EfecteXuixo>
        {
            new EfecteTeclesInvertides(),
            new EfecteHipervelocitat(),
            new EfecteTrencaAsteroides(),
            new EfecteDisparTriple(),
            new EfecteVidaExtra(),
            new EfectePlacebo(),
            new EfecteModeMirall(),
            new EfecteCameraLenta(),
            new EfectePuntsInvertits(),
            new EfecteBalesDePlastic(),
            new EfecteMonocromatisme(),
            new EfecteLlaonaDeGirona(),
            new EfecteSenseHUD(),
        };
    }

    private readonly Dictionary<EfecteXuixo, int> vegades = new Dictionary<EfecteXuixo, int>();
    private readonly List<EfecteActiu> actius = new List<EfecteActiu>();

    public IReadOnlyList<EfecteActiu> Actius => actius;

    private PlayerScript nau;
    private GameManager gm;
    private bool netejat = false;

    void Start()
    {
        nau = GetComponent<PlayerScript>();
        gm = GameManager.Instance;
        foreach (EfecteXuixo e in disponibles) vegades[e] = 0;
    }

    public void ActivarEfecteAleatori()
    {
        // Només els que encara no han sortit 2 vegades
        List<EfecteXuixo> candidats = new List<EfecteXuixo>();
        foreach (EfecteXuixo e in disponibles)
            if (vegades[e] < maxVegadesPerEfecte) candidats.Add(e);

        // Si ja han sortit tots, toca un placebo
        EfecteXuixo triat = candidats.Count > 0
            ? candidats[Random.Range(0, candidats.Count)]
            : new EfectePlacebo();

        Activar(triat);
    }

    public void Activar(EfecteXuixo efecte)
    {
        if (nau == null || gm == null) return;

        if (vegades.ContainsKey(efecte)) vegades[efecte]++;
        gm.MostrarMissatge(efecte.Missatge, ColorDe(efecte.Tipus));

        // Instantani (Vida extra, Placebo...): s'aplica i ja està
        if (efecte.Durada <= 0f)
        {
            efecte.Activar(nau, gm);
            return;
        }

        // Si ja està actiu, només li reiniciem el temps (no l'activem dues vegades)
        foreach (EfecteActiu a in actius)
        {
            if (a.efecte == efecte)
            {
                a.tempsRestant = efecte.Durada;
                return;
            }
        }

        efecte.Activar(nau, gm);
        actius.Add(new EfecteActiu { efecte = efecte, tempsRestant = efecte.Durada });
    }

    void Update()
    {
        if (gm != null && gm.PartidaAcabada)
        {
            // En acabar la partida traiem tots els efectes (que no quedi res a mitges)
            if (!netejat) { DesactivarTots(); netejat = true; }
            return;
        }

        // Comptem en segons reals (la Càmera lenta no allarga els altres efectes),
        // però si el joc està en pausa (timeScale 0) el temps s'atura
        float dt = Time.timeScale > 0f ? Time.unscaledDeltaTime : 0f;

        // Recorrem la llista al revés perquè podem treure'n elements
        for (int i = actius.Count - 1; i >= 0; i--)
        {
            actius[i].tempsRestant -= dt;
            if (actius[i].tempsRestant <= 0f)
            {
                actius[i].efecte.Desactivar(nau, gm);
                actius.RemoveAt(i);
            }
        }
    }

    public void DesactivarTots()
    {
        foreach (EfecteActiu a in actius) a.efecte.Desactivar(nau, gm);
        actius.Clear();
    }

    public static Color ColorDe(TipusEfecte tipus)
    {
        switch (tipus)
        {
            case TipusEfecte.Buff: return new Color(0.5f, 1f, 0.5f);
            case TipusEfecte.Debuff: return new Color(1f, 0.4f, 0.4f);
            default: return new Color(1f, 0.85f, 0.35f);
        }
    }
}
