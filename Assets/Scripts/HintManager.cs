using UnityEngine;

public class HintManager : MonoBehaviour
{
    public ScrubGhostHint scrubGhostHint;
    public ShakeHint shakeHint;
    public float scrubIdleTimeout = 4.0f;

    bool _ready;
    bool _scrubLocked;
    bool _scrubHintActive;
    bool _rinsePhase;
    bool _demoDone;
    float _idleTimer;
    Vector3[] _currentPath;

    // Called by Level2Manager once the title card finishes.
    public void StartHinting()
    {
        _ready       = true;
        _scrubLocked = true;
        _demoDone    = false;
        _currentPath = BuildTeethArcPath();
        ShowScrubHint(lockUntilDone: true);
    }

    void Update()
    {
        if (!_ready || _rinsePhase || _scrubHintActive || !_demoDone) return;
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= scrubIdleTimeout)
        {
            _idleTimer = 0f;
            ShowScrubHint(lockUntilDone: false);
        }
    }

    public bool IsScrubLocked() => _scrubLocked;

    public void OnScrubActivity()
    {
        if (!_ready) return;
        _idleTimer = 0f;
        if (_scrubHintActive && _demoDone) HideScrubHint();
    }

    public void EnterRinsePhase()
    {
        _rinsePhase  = true;
        _scrubLocked = false;
        HideScrubHint();
        shakeHint?.Show();
    }

    public void ExitRinsePhase()
    {
        _rinsePhase  = false;
        _demoDone    = false;
        _scrubLocked = true;
        _idleTimer   = 0f;
        shakeHint?.Hide();
        _currentPath = BuildTonguePath();
        ShowScrubHint(lockUntilDone: true);
    }

    public void OnShakeActivity() => shakeHint?.Hide();

    void ShowScrubHint(bool lockUntilDone)
    {
        if (scrubGhostHint == null || _currentPath == null) return;
        _scrubHintActive = true;

        if (lockUntilDone)
        {
            _scrubLocked = true;
            scrubGhostHint.OnOneSweepDone -= OnFirstSweepDone;
            scrubGhostHint.OnOneSweepDone += OnFirstSweepDone;
        }

        scrubGhostHint.Show(_currentPath);
    }

    void OnFirstSweepDone()
    {
        scrubGhostHint.OnOneSweepDone -= OnFirstSweepDone;
        _scrubLocked = false;
        _demoDone    = true;
        HideScrubHint();
    }

    void HideScrubHint()
    {
        _scrubHintActive = false;
        scrubGhostHint?.Hide();
    }

    static Vector3[] BuildTeethArcPath()
    {
        const float z = -0.5f;
        const int segs = 12;

        var zones = Object.FindObjectsByType<ToothScrubZone>(FindObjectsSortMode.None);
        float upperY = 0.28f, lowerY = -1.55f, leftX = -2.3f, rightX = 2.3f;

        if (zones.Length > 0)
        {
            float sU = 0, sL = 0;
            int cU = 0, cL = 0;
            float mnX = float.MaxValue, mxX = float.MinValue;

            foreach (var z2 in zones)
            {
                var p = z2.transform.position;
                mnX = Mathf.Min(mnX, p.x);
                mxX = Mathf.Max(mxX, p.x);
                if (p.y > -0.5f) { sU += p.y; cU++; }
                else             { sL += p.y; cL++; }
            }

            if (cU > 0) upperY = sU / cU;
            if (cL > 0) lowerY = sL / cL;
            leftX  = mnX - 0.2f;
            rightX = mxX + 0.2f;
        }

        float mx = (leftX + rightX) * 0.5f;
        var up   = Bezier(new Vector3(leftX,  upperY, z), new Vector3(mx, upperY + 0.3f, z), new Vector3(rightX, upperY, z), segs);
        var lo   = Bezier(new Vector3(rightX, lowerY, z), new Vector3(mx, lowerY - 0.3f, z), new Vector3(leftX,  lowerY, z), segs);
        var path = new Vector3[up.Length + lo.Length];
        up.CopyTo(path, 0);
        lo.CopyTo(path, up.Length);
        return path;
    }

    static Vector3[] BuildTonguePath()
    {
        const float z = -0.5f;
        const int segs = 8;

        var zones = Object.FindObjectsByType<TongueScrubZone>(FindObjectsSortMode.None);
        if (zones.Length == 0)
            return Bezier(new Vector3(-1.5f, 0.3f, z), new Vector3(0f, -1f, z), new Vector3(1.5f, -2.5f, z), segs);

        float mnX = float.MaxValue, mxX = float.MinValue;
        float mnY = float.MaxValue, mxY = float.MinValue;
        foreach (var tz in zones)
        {
            var p = tz.transform.position;
            mnX = Mathf.Min(mnX, p.x); mxX = Mathf.Max(mxX, p.x);
            mnY = Mathf.Min(mnY, p.y); mxY = Mathf.Max(mxY, p.y);
        }

        return Bezier(
            new Vector3(mnX, mxY, z),
            new Vector3((mnX + mxX) * 0.5f, (mnY + mxY) * 0.5f, z),
            new Vector3(mxX, mnY, z),
            segs);
    }

    static Vector3[] Bezier(Vector3 p0, Vector3 ctrl, Vector3 p2, int steps)
    {
        var pts = new Vector3[steps];
        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            float u = 1f - t;
            pts[i] = u * u * p0 + 2 * u * t * ctrl + t * t * p2;
        }
        return pts;
    }
}
